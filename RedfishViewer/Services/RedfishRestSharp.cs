// Copyright (c) 2023-2026 Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Text.Json;
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
using System.Reflection;
using System.Text;
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


        // RestClient キャッシュ（プロキシ/タイムアウト設定が変わるまで再利用）
        private RestClient? _cachedClient;
        private string _cachedClientKey = string.Empty;

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
                    Configure = JsonSerializer.Deserialize<Configure>(item.Json, JsonHelper.Options) ?? new();
                    Configure.ProxyPassword = CryptoAes.Decrypt(Configure.ProxyPassword) ?? string.Empty;
                }
                else
                {
                    // DB はあるが設定未記録(初回起動) → システムテーマを反映
                    Configure.IsDark = IsSystemDarkMode();
                }
            }
            else
            {
                // DB 未作成(初回起動) → システムテーマを反映
                Configure.IsDark = IsSystemDarkMode();
            }
            SetSwatch();

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
            _cachedClient?.Dispose();
            _cachedClient = null;
            _disposables.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// プロキシ・タイムアウト設定が変わるまで RestClient を再利用する
        /// </summary>
        private RestClient GetOrCreateClient()
        {
            var key = $"{Configure.MaxTimeout}|{Configure.ProxyEnabled}|{Configure.ProxyUri}|{Configure.ProxyUsername}|{Configure.ProxyPassword}";
            if (_cachedClient != null && _cachedClientKey == key)
                return _cachedClient;

            _cachedClient?.Dispose();
            var options = new RestClientOptions()
            {
                Timeout = 0 < Configure.MaxTimeout ? TimeSpan.FromSeconds(Configure.MaxTimeout) : Timeout.InfiniteTimeSpan,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
            };
            if (Configure.ProxyEnabled)
            {
                options.Proxy = new WebProxy(Configure.ProxyUri);
                if (!string.IsNullOrEmpty(Configure.ProxyUsername))
                    options.Credentials = new NetworkCredential(Configure.ProxyUsername, Configure.ProxyPassword);
            }
            _cachedClient = new RestClient(options);
            _cachedClientKey = key;
            return _cachedClient;
        }

        /// <summary>
        /// システムのダークモード設定を取得する
        /// AppsUseLightTheme: 0 = ダーク、1 = ライト（省略時はライト扱い）
        /// </summary>
        private static bool IsSystemDarkMode()
            => Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", 1) is int v && v == 0;

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
                using var _ = JsonDocument.Parse(jsonText, JsonHelper.DocumentOptions);
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

                // Basic 認証ヘッダをリクエストに付与する（クライアントはキャッシュするため per-request で設定）
                if (!string.IsNullOrEmpty(search.Username))
                    request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{search.Username}:{search.Password}")));

                // リクエスト実行（RestClient はプロキシ/タイムアウト設定が変わるまで再利用）
                var client = GetOrCreateClient();
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

                // Content-Type ヘッダまたはボディ内容で JSON 判定する
                content = response.Content ?? string.Empty;
                isJsonText = headers.Any(x => x.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                                           && x.Value.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                             || IsJsonText(content);

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
                        InHeaders = JsonSerializer.Serialize(search.Headers, JsonHelper.Options),
                        Parameters = JsonSerializer.Serialize(search.Parameters, JsonHelper.Options),
                        JsonBody = search.JsonBody,
                        StatusCode = (int)response.StatusCode,
                        OutHeaders = JsonSerializer.Serialize(headers, JsonHelper.Options),
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
                        HttpStatusCode.RequestTimeout => "リクエストがタイムアウトしました。",
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
                    InHeaders = JsonSerializer.Serialize(search.Headers, JsonHelper.Options),
                    Parameters = JsonSerializer.Serialize(search.Parameters, JsonHelper.Options),
                    JsonBody = search.JsonBody,
                    OutHeaders = JsonSerializer.Serialize(headers, JsonHelper.Options),
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
            var list = new ObservableCollection<string>();
            try
            {
                var baseUri = new Uri(result.Uri);
                using var doc = JsonDocument.Parse(result.Content ?? "{}", JsonHelper.DocumentOptions);
                CollectOdataIds(doc.RootElement, baseUri, list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"@odata.id / href の解析に失敗しました。({result.Uri})");
                _logger.Error(result.Content);
            }
            return list;
        }

        private static void CollectOdataIds(JsonElement element, Uri baseUri, ObservableCollection<string> list)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        if ((prop.Name == "@odata.id" || prop.Name == "href") &&
                            prop.Value.ValueKind == JsonValueKind.String)
                        {
                            var raw = prop.Value.GetString();
                            if (raw != null)
                            {
                                var uri2 = new Uri($"{baseUri.Scheme}://{baseUri.Authority}{raw}".TrimEnd('/'));
                                if (baseUri.AbsoluteUri != uri2.AbsoluteUri)
                                    list.Add(uri2.AbsoluteUri);
                            }
                        }
                        else
                        {
                            CollectOdataIds(prop.Value, baseUri, list);
                        }
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                        CollectOdataIds(item, baseUri, list);
                    break;
            }
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

            try
            {
                using var doc = JsonDocument.Parse(content, JsonHelper.DocumentOptions);
                return FindOdataEtag(doc.RootElement) ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "@odata.etag の解析に失敗しました。");
                _logger.Error(content);
            }
            return string.Empty;
        }

        private static string? FindOdataEtag(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Name == "@odata.etag" && prop.Value.ValueKind == JsonValueKind.String)
                        return prop.Value.GetString();
                    var found = FindOdataEtag(prop.Value);
                    if (found != null)
                        return found;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var found = FindOdataEtag(item);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }
    }
}
