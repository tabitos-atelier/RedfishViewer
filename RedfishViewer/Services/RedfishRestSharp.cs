using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using NLog;
using Prism.Dialogs;
using Prism.Events;
using RedfishViewer.Events;
using RedfishViewer.Models;
using RedfishViewer.Plugins;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace RedfishViewer.Services
{
    /// <summary>
    /// Redfish アクセス(RestSharp版)
    /// </summary>
    public class RedfishRestSharp : IRedfishAdapter
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        private readonly IDialogService _dialogService;                     // IDialogService
        private readonly IEventAggregator _eventAggregator;                 // IEventAggregator

        // メソッドデータ
        private readonly Dictionary<string, Method> _methods
            = new(StringComparer.OrdinalIgnoreCase)
            {
                { "GET", Method.Get },
                { "POST", Method.Post },
                { "PATCH", Method.Patch },
                { "PUT", Method.Put },
                { "DELETE", Method.Delete },
            };

        public ObservableCollection<HttpError> HttpErrors { get; private set; } = [];
        public Configure Configure { get; } = new();                        // アプリ構成
        public Dictionary<string, Swatch> Swatches { get; }                 // カラーマップ
            = new SwatchesProvider().Swatches.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        public ObservableCollection<string> Colors { get; }                 // 色一覧
            = ["Yellow", "Amber", "DeepOrange", "LightBlue", "Teal", "Cyan", "Pink", "Green", "DeepPurple", "Indigo", "LightGreen", "Blue", "Lime", "Red", "Orange", "Purple", "BlueGrey", "Grey", "Brown"];
        public Func<List<Setting>>? GetSettings { get; set; }               // DB保存用の設定情報
        public List<KeyValuePair<string, BasePlugin?>> Plugins { get; }     // プラグイン

        /// <summary>
        /// RedfishAdapter
        /// </summary>
        /// <param name="redfishContext"></param>
        public RedfishRestSharp(IUnityContainer unityContainer)
        {
            _logger.Trace($"{this.GetType().Name}.");

            _dialogService = unityContainer.Resolve<IDialogService>();
            _eventAggregator = unityContainer.Resolve<IEventAggregator>();
            var dbAgent = unityContainer.Resolve<IDatabaseAgent>();

            // アプリ構成を読み込む
            if (dbAgent.IsCreated())
            {
                var item = dbAgent.GetSetting(1);
                if (item != null && item.Json != null)
                {
                    Configure = JsonConvert.DeserializeObject<Configure>(item.Json) ?? new();
                    Configure.ProxyPassword = CryptoAes.Decrypt(Configure.ProxyPassword) ?? string.Empty;
                    SetSwatch();
                }
            }

            // 拡張プラグイン読み込み
            var plugin = new DefaultPlugin(_dialogService);
            Plugins =
            [
                new("None", null),
                new(plugin.PluginName, plugin)
            ];
            LoadPlugin();
        }

        void IDisposable.Dispose()
        {
            _logger.Trace("Dispose.");
            _disposables.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 色設定
        /// </summary>
        private void SetSwatch()
        {
            var palette = new PaletteHelper();
            var theme = palette.GetTheme();
            theme.SetBaseTheme(Configure.IsDark ? BaseTheme.Dark : BaseTheme.Light);            // ライト or ダークモード
            theme.SetPrimaryColor(Swatches[Configure.PrimaryColor].PrimaryHues[5].Color);       // プライマリ色
            theme.SetSecondaryColor(Swatches[Configure.SecondaryColor].PrimaryHues[5].Color);   // セカンダリ色
            theme.ColorAdjustment = !Configure.IsColorAdjustment ? null :
                new ColorAdjustment
                {
                    DesiredContrastRatio = 4.5f,    // コントラスト比
                    Contrast = Contrast.Medium,     // コントラスト
                    Colors = ColorSelection.All     // 対象範囲
                };
            palette.SetTheme(theme);
        }

        /// <summary>
        /// プラグイン読み込み
        /// </summary>
        private void LoadPlugin()
        {
            _logger.Trace("LoadPlugin.");

            // RedfishViewer のディレクトリを取得する
            var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            if (dir == null)
                return;

            // プラグインフォルダ(なければ作成する)
            var pluginPath = System.IO.Path.Combine(dir.FullName, "Plugins");
            dir = new DirectoryInfo(pluginPath);
            if (!dir.Exists)
                dir.Create();

            // DLL を読み込んでプラグインを取得する
            foreach (var file in dir.GetFiles("*.dll"))
            {
                var asm = Assembly.LoadFrom(file.FullName);
                foreach (var type in asm.GetTypes())
                {
                    if (type.BaseType != typeof(BasePlugin))
                        continue;

                    // プラグインのインスタンスを作成
                    object? obj = Activator.CreateInstance(type, new object[] { _dialogService });
                    if (obj == null)
                        continue;

                    // プラグイン格納
                    dynamic plugin = obj;
                    Plugins.Add(new KeyValuePair<string, BasePlugin?>(plugin.PluginName, plugin));
                    _logger.Info($"Add plugin: {plugin.PluginName}");
                }
            }
        }

        /// <summary>
        /// JSON文字列の判定
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static bool IsJsonText(string jsonText)
        {
            try
            {
                JsonConvert.DeserializeObject(jsonText);    // 成功すれば JSON 文字列
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ルートURI生成
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GenerateRootUri(Uri uri)
            => $"{uri.Scheme}://{uri.Authority}";

        /// <summary>
        /// Webにリクエストを送信する
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<HttpResponse> RequestAsync(Search search)
        {
            var msg = $"{search.Method}, {search.Keyword}";
            if (!string.IsNullOrEmpty(search.ParentUri))
                msg += $" ({search.ParentUri})";
            _logger.Info(msg);

            var uri = new Uri(search.Keyword.TrimEnd('/'));
            RestResponse? response = null;
            var headers = new List<KeyValue>();
            var isJsonText = false;
            var content = string.Empty;
            try
            {
                // HTTPリクエスト作成
                var request = new RestRequest(uri, _methods[search.Method]);
                // ヘッダ
                foreach (var param in search.Headers)
                    request.AddHeader(param.Key, param.Value);          // ヘッダ
                foreach (var param in search.Parameters)
                    request.AddParameter(param.Key, param.Value);       // パラメータ
                if (0 < search.JsonBody?.Length)
                    request.AddJsonBody(search.JsonBody);               // JsonBody

                // クライアント設定
                var options = new RestClientOptions()
                {
                    Timeout = 0 < Configure.MaxTimeout ? TimeSpan.FromSeconds(Configure.MaxTimeout) : Timeout.InfiniteTimeSpan,
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    Authenticator = new HttpBasicAuthenticator(search.Username, search.Password),
                };

                // プロキシ設定
                if (Configure.ProxyEnabled)
                {
                    options.Proxy = new WebProxy(Configure.ProxyUri);
                    if (!string.IsNullOrEmpty(Configure.ProxyUsername))
                        options.Credentials = new NetworkCredential(Configure.ProxyUsername, Configure.ProxyPassword);
                }

                // リクエスト実行
                var client = new RestClient(options);
                response = await client.ExecuteAsync(request) ?? throw new Exception("HTTPリクエストに失敗しました。");
                _logger.Info($"Http Status Code: {(int)response.StatusCode}, {response.StatusCode}");
                if (!string.IsNullOrEmpty(response.Content))
                    _logger.Info(response.Content);

                // ヘッダの取得
                response.ContentHeaders?
                    .ToList()
                    .ForEach(x => headers.Add(new KeyValue(x.Name, x.Value as string ?? string.Empty)));
                response.Headers?
                    .ToList()
                    .ForEach(x => headers.Add(new KeyValue(x.Name, x.Value as string ?? string.Empty)));

                // ヘッダが JSON 以外の場合、JSONかを検証する
                content = response.Content ?? string.Empty;
                isJsonText = (headers.FirstOrDefault(x => x.Key.Equals("application/json")) != null) || IsJsonText(content);    // 成功すれば JSON 文字列

                // HTTP レスポンス エラー判定
                if (response.ErrorException != null)
                    throw new Exception($"{response.StatusCode}");

                // レスポンスを返す
                return new HttpResponse
                {
                    Uri = uri,
                    RestResponse = response,
                    IsJsonText = isJsonText,
                    Result = new Result
                    {
                        Uri = uri.AbsoluteUri,
                        Method = request.Method.ToString(),
                        InHeaders = JsonConvert.SerializeObject(search.Headers),
                        Parameters = JsonConvert.SerializeObject(search.Parameters),
                        JsonBody = search.JsonBody,
                        StatusCode = (int)response.StatusCode,
                        OutHeaders = JsonConvert.SerializeObject(headers),
                        IsJsonText = isJsonText,
                        Updated = DateTime.Now,
                        Content = content,
                    }
                };
            }
            catch (Exception ex)
            {
                string errMsg;
                var statusCode = 0;
                if (response == null)
                {
                    _logger.Error(ex, "HTTPリクエストに失敗しました。");
                    errMsg = ex.Message;
                }
                else
                {
                    statusCode = (int)response.StatusCode;
                    errMsg = (0 < statusCode) ? $"{response.StatusCode}, " : string.Empty;
                    errMsg += response.StatusCode switch
                    {
                        HttpStatusCode.BadRequest => "リクエストが不正です。",
                        HttpStatusCode.Unauthorized => "認証に失敗またはアクセス権がありません。",
                        HttpStatusCode.Forbidden => "閲覧権限がありません。",
                        HttpStatusCode.NotFound => "ページが見つかりません。",
                        HttpStatusCode.MethodNotAllowed => "メソッドが許可されていません。",
                        HttpStatusCode.ProxyAuthenticationRequired => "プロキシサーバの認証情報が不足しています。",
                        HttpStatusCode.RequestTimeout => "プロキシサーバの認証情報が不足しています。",
                        _ => "アクセスに失敗しました。",
                    };
                    _logger.Error(response.ErrorException, $"{statusCode}:{errMsg}");
                }

                // エラーメッセージの先頭にステータスコードを付ける
                var error = new HttpError
                {
                    Created = DateTime.Now,
                    StatusCode = statusCode,
                    Message = errMsg,
                    Method = search.Method,
                    ProxyEnabled = Configure.ProxyEnabled,
                    Uri = uri.AbsoluteUri,
                    InHeaders = JsonConvert.SerializeObject(search.Headers),
                    Parameters = JsonConvert.SerializeObject(search.Parameters),
                    JsonBody = search.JsonBody,
                    OutHeaders = JsonConvert.SerializeObject(headers),
                    IsJsonText = isJsonText,
                    Content = content,
                };
                HttpErrors.Add(error);

                // エラー通知
                _eventAggregator
                    .GetEvent<HttpErrorEvent<HttpError>>()
                    .Publish(error);

                // レスポンス返却
                return new HttpResponse {
                    RestResponse = response,
                    HttpError = error,
                };
            }
        }

        /// <summary>
        /// Webレスポンスから自動リクエスト用の '@odata.id' or 'href' を取得する
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public ObservableCollection<string> GetOdataIds(Result result)
        {
            var jsonTags = new HashSet<JsonToken>() { JsonToken.StartObject, JsonToken.EndObject, JsonToken.StartArray, JsonToken.EndArray };
            var list = new ObservableCollection<string>();
            string? tag = null;
            try
            {
                // '@odata.id' のURIを格納する
                var uri1 = new Uri(result.Uri);
                var reader = new JsonTextReader(new StringReader(result.Content ?? ""));
                while (reader.Read())
                {
                    // プロパティ以外
                    if (reader.TokenType != JsonToken.PropertyName)
                        continue;

                    // "@odata.id" or "href" 以外
                    var value = reader.Value;
                    if (value == null)
                        continue;
                    tag = (string)value;
                    if (!(tag.Equals("@odata.id") || tag.Equals("href")))
                        continue;

                    // プロパティの値を読み込む
                    reader.Read();

                    // {} or [] 出現
                    value = reader.Value;
                    if (value == null || jsonTags.Contains(reader.TokenType))
                        continue;

                    // 同じURIは格納しない
                    var uri2 = new Uri($"{uri1.Scheme}://{uri1.Authority}{value}".TrimEnd('/'));
                    if (uri1.AbsoluteUri == uri2.AbsoluteUri)
                        continue;                       // 自分自身は格納しない

                    // 自動HTTPリクエスト用キューに追加する
                    list.Add(uri2.AbsoluteUri);
                }
            }
            catch (Exception ex)
            {
                tag ??= "'@odata.id' or 'href'";
                _logger.Error(ex, $"{tag} の解析に失敗しました。({result.Uri})");
                _logger.Error(result.Content);
            }
            return list;
        }

        /// <summary>
        /// @odata.etag を取得する
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string GetOdataEtag(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            var jsonTags = new HashSet<JsonToken>() { JsonToken.StartObject, JsonToken.EndObject, JsonToken.StartArray, JsonToken.EndArray };
            try
            {
                // '@odata.id' のURIを格納する
                var reader = new JsonTextReader(new StringReader(content));
                while (reader.Read())
                {
                    var value = reader.Value;
                    if (reader.TokenType != JsonToken.PropertyName)
                        continue;                   // プロパティ以外
                    if (value == null || !((string)value).Equals("@odata.etag"))
                        continue;                   // @odata.etag 以外

                    // プロパティの値を読み込む
                    reader.Read();
                    value = reader.Value;
                    if (value == null || jsonTags.Contains(reader.TokenType))
                        continue;                   // {} or [] 出現
                    return (string)value;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"@odata.etag の解析に失敗しました。");
                _logger.Error(content);
            }
            return string.Empty;
        }
    }
}
