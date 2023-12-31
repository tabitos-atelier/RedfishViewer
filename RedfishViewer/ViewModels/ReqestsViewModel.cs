using Newtonsoft.Json;
using NLog;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedfishViewer.Events;
using RedfishViewer.Models;
using RedfishViewer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// HTTPリクエスト VM
    /// </summary>
    public class ReqestsViewModel : BindableBase, IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly AssemblyName _asm = Assembly.GetExecutingAssembly().GetName();
        private readonly CompositeDisposable _disposables = [];

        private readonly IRegionManager _regionManager;                                 // IRegionManager
        private readonly IDialogService _dialogService;                                 // IDialogService
        private readonly IEventAggregator _eventAggregator;                             // IEventAggregator
        private readonly IRedfishAdapter _redfishAdapter;                               // Redfish RestAPI
        private readonly IDatabaseAgent _dbAgent;                                       // DBアクセス

        public ReactiveCommandSlim<string> MenuChangedCommand { get; }                  // メニュー選択(アイコン共用)

        // Web＆キーワード検索
        public ReadOnlyReactiveCollection<string> HttpMethods { get; private set; }     // HTTPリクエストメソッド
        public ReactivePropertySlim<string> HttpMethod { get; set; }                    // 選択中のHTTPリクエストメソッド
        public ReactivePropertySlim<string> Keyword { get; private set; }               // キーワード(URL/検索ワード)
        public ReactiveCollection<string> Keywords { get; private set; }                // キーワード一覧
        public ReactiveCommandSlim SearchCommand { get; }                               // 検索ボタン
        public ReactivePropertySlim<Visibility> SearchProgressBar { get; private set; } // 検索中のぐるぐる

        // 自動検索
        public ReactivePropertySlim<bool> AutoDiveChecked { get; private set; }         // 自動検索
        public ReactivePropertySlim<bool> AutoDiveEnabled { get; private set; }         // 自動検索の有効・無効

        // アカウント
        public ReactivePropertySlim<string> Username { get; private set; }              // HTTPユーザ名
        public ReactivePropertySlim<string> Password { get; private set; }              // HTTPパスワード(DB保存時は暗号化)

        // HTTPリクエスト:ヘッダ
        public ReactiveCollection<ParameterViewModel> HttpHeaders { get; set; }         // HTTPリクエストヘッダ
        public ReactiveCommandSlim AddHttpHeaderCommand { get; }                        // HTTPリクエストヘッダ追加

        // HTTPリクエスト:パラメータ
        public ReactiveCollection<ParameterViewModel> HttpParameters { get; set; }      // HTTPリクエストパラメータ
        public ReactiveCommandSlim AddHttpParameterCommand { get; }                     // HTTPリクエストパラメータ追加

        // HTTPリクエスト:JSONボディ
        public ReactivePropertySlim<string> HttpJsonBody { get; private set; }          // リクエストJSONボディ
        public ReactiveCommandSlim HttpJsonBodyFormatCommand { get; }                   // JSON整形

        // View表示後
        public AsyncReactiveCommand<RoutedEventArgs> LoadedCommand { get; }             // View表示後

        /// <summary>
        /// ReqestsViewModel
        /// </summary>
        /// <param name="unityContainer"></param>
        public ReqestsViewModel(IUnityContainer unityContainer)
        {
            _logger.Trace($"{this.GetType().Name}.");

            _regionManager = unityContainer.Resolve<IRegionManager>();
            _dialogService = unityContainer.Resolve<IDialogService>();
            _eventAggregator = unityContainer.Resolve<IEventAggregator>();
            _redfishAdapter = unityContainer.Resolve<IRedfishAdapter>();
            _dbAgent = unityContainer.Resolve<IDatabaseAgent>();

            // 初期メニューの表示
            _regionManager.RegisterViewWithRegion("ContentBody", typeof(Views.Responses));
            // メニュー選択(アイコン共用)・・・ユーザコントロールの切替え
            MenuChangedCommand = new ReactiveCommandSlim<string>()
                .WithSubscribe(MenuChanged)
                .AddTo(_disposables);

            // View表示後
            LoadedCommand = new AsyncReactiveCommand<RoutedEventArgs>()
                .WithSubscribe(async _ => await LoadedAsync())
                .AddTo(_disposables);

            // HTTPリクエストメソッド
            HttpMethods = new ObservableCollection<string> { "GET", "POST", "PATCH", "PUT", "DELETE" }
                .ToReadOnlyReactiveCollection()
                .AddTo(_disposables);
            HttpMethod = new ReactivePropertySlim<string>()
                .AddTo(_disposables);
            HttpMethod
                .ObserveProperty(x => x.Value)
                .Subscribe(MethodSelectionChanged)
                .AddTo(_disposables);
            // キーワード
            Keyword = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(_disposables);
            // キーワード一覧
            Keywords = new ReactiveCollection<string>()
                .AddTo(_disposables);
            // コマンド設定
            SearchCommand = Keyword
                .Select(x => !string.IsNullOrWhiteSpace(x))
                .ToReactiveCommandSlim()
                .WithSubscribe(StartSearch)
                .AddTo(_disposables);
            // ぐるぐる
            SearchProgressBar = new ReactivePropertySlim<Visibility>(Visibility.Collapsed)
                .AddTo(_disposables);
            // 検索終了イベント
            _eventAggregator
                .GetEvent<PostSearchEvent<string?>>()
                .Subscribe(EndOfSearch, ThreadOption.UIThread)
                .AddTo(_disposables);

            // 自動検索
            AutoDiveChecked = new ReactivePropertySlim<bool>(false)
                .AddTo(_disposables);
            AutoDiveChecked
                .ObserveProperty(x => x.Value)
                .Subscribe(AutoDiveStatus)
                .AddTo(_disposables);
            // 自動検索の有効・無効
            AutoDiveEnabled = new ReactivePropertySlim<bool>(true)
                .AddTo(_disposables);
            // 自動検索オン
            _eventAggregator
                .GetEvent<AutoDiveOnEvent<bool>>()
                .Subscribe(x => AutoDiveChecked.Value = x, ThreadOption.UIThread)
                .AddTo(_disposables);

            // ユーザ名
            Username = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(_disposables);
            // パスワード(DB保存時は暗号化)
            Password = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(_disposables);
            // アカウント設定イベント
            _eventAggregator
                .GetEvent<SetAccountEvent<KeyValuePair<string?, string?>>>()
                .Subscribe(x =>
                {
                    Username.Value = x.Key ?? string.Empty;
                    Password.Value = x.Value ?? string.Empty;
                }, ThreadOption.UIThread)
                .AddTo(_disposables);

            // HTTPリクエストヘッダ
            HttpHeaders = new ReactiveCollection<ParameterViewModel>()
                .AddTo(_disposables);
            InitInHeader();
            // HTTPリクエストヘッダ追加
            AddHttpHeaderCommand = new ReactiveCommandSlim()
                .WithSubscribe(AddInHeader)
                .AddTo(_disposables);
            // 最新の @odata.etag を追加
            _eventAggregator
                .GetEvent<SetEtagEvent<string>>()
                .Subscribe(SetEtag, ThreadOption.UIThread)
                .AddTo(_disposables);

            // HTTPリクエストパラメータ
            HttpParameters = new ReactiveCollection<ParameterViewModel>()
                .AddTo(_disposables);
            InitParameter();
            // HTTPリクエストパラメータ追加
            AddHttpParameterCommand = new ReactiveCommandSlim()
                .WithSubscribe(AddParameter)
                .AddTo(_disposables);

            // HTTPリクエストJSONボディ
            HttpJsonBody = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(_disposables);
            // JSON整形ボタン
            HttpJsonBodyFormatCommand = new ReactiveCommandSlim()
                .WithSubscribe(JsonFormat)
                .AddTo(_disposables);

            // 終了処理時に保存する設定情報を渡す
            _redfishAdapter.GetSettings = CreateSttings;

            // 再リクエスト時にHTTPリクエスト情報を変更する
            _eventAggregator
                .GetEvent<ReRequestEvent<Search>>()
                .Subscribe(ReRequest, ThreadOption.UIThread)
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();

        /// <summary>
        /// View表示後
        /// </summary>
        /// <returns></returns>
        private async Task LoadedAsync()
        {
            try
            {
                // 新たにデータベースを作成する。
                if (await _dbAgent.CreateDatabaseAsync())
                    return;

                // キーワード履歴を読み込む
                var item = _dbAgent.GetSetting(2);
                if (item != null && item.Json != null)
                    Keywords.AddRange(JsonConvert.DeserializeObject<List<string>>(item.Json));
            }
            catch (Exception ex)
            {
                var errMsg = "データベース作成または読込みに失敗しました。";
                _logger.Error(ex, errMsg);
                _dialogService.AlertMessage($"{errMsg}\n\n{ex.Message}");
                System.Windows.Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// メニュー切り替え
        /// </summary>
        /// <param name="viewName"></param>
        private void MenuChanged(string viewName)
            => _regionManager.RequestNavigate("ContentBody", viewName);     // Viewを切替える

        /// <summary>
        /// リクエストメソッド変更
        /// </summary>
        /// <param name="method"></param>
        private void MethodSelectionChanged(string method)
        {
            if (method == null)
                return;
            AutoDiveEnabled.Value = method.Equals("GET", StringComparison.OrdinalIgnoreCase);   // GET以外はオフ
        }

        /// <summary>
        /// 検索開始
        /// </summary>
        /// <returns></returns>
        private void StartSearch()
        {
            // ぐるぐるを表示する
            SearchProgressBar.Value = Visibility.Visible;

            var search = new Search
            {
                Keyword = Keyword.Value,                // URI or SearchWord
                Method = HttpMethod.Value,              // リクエストメソッド
                IsAutoDive = AutoDiveChecked.Value,     // 自動検索
                Username = Username.Value,              // Basic認証:ユーザ名
                Password = Password.Value,              // Basic認証:パスワード
                JsonBody = HttpJsonBody.Value,          // HTTPリクエスト:JSONボディ
            };

            // HTTP:リクエスト情報の設定
            foreach (var param in HttpHeaders)
            {
                if (param.Enabled.Value && 0 < param.Name.Value.Length && 0 < param.Value.Value?.Length)
                    search.Headers.Add(new KeyValue(param.Name.Value, param.Value.Value));
            }
            foreach (var param in HttpParameters)
            {
                if (param.Enabled.Value && 0 < param.Name.Value.Length && 0 < param.Value.Value?.Length)
                    search.Parameters.Add(new KeyValue(param.Name.Value, param.Value.Value));
            }

            // 検索イベント発行
            _eventAggregator
                .GetEvent<SearchEvent<Search>>()
                .Publish(search);
        }

        /// <summary>
        /// 検索終了
        /// </summary>
        /// <param name="keyword"></param>
        private void EndOfSearch(string? keyword)
        {
            // ぐるぐるを隠す
            SearchProgressBar.Value = Visibility.Collapsed;

            // キーワード判定（エラー時は null)
            if (string.IsNullOrEmpty(keyword))
                return;

            // キーワードを履歴に格納する
            if (Keywords.Any(x => x.Equals(keyword, StringComparison.CurrentCultureIgnoreCase)))
            {
                Keywords.Remove(keyword);               // 先頭移動のため削除する
                Keyword.Value = keyword;                // 表示が消えるため、再設定する
            }
            Keywords.Insert(0, keyword);                // 先頭に追加する
            if (100 < Keywords.Count)                   // 100件超えた分を削除する
                Keywords.RemoveAt(Keywords.Count - 1);
        }

        /// <summary>
        /// 自動検索の状態
        /// </summary>
        /// <param name="stat"></param>
        private void AutoDiveStatus(bool stat)
        {
            if (stat)
                return;

            // 自動検索のオフ状態を通知する
            _eventAggregator
                .GetEvent<AutoDiveOffEvent<bool>>()
                .Publish(stat);
        }

        /// <summary>
        /// リクエストヘッダ初期値
        /// </summary>
        private void InitInHeader()
            => HttpHeaders.AddRange(new[]
            {
                new ParameterViewModel(new Parameter { Enabled = true, Name = "User-Agent", Value = $"{_asm.Name}/{_asm.Version}" }),
                new ParameterViewModel(new Parameter { Enabled = true, Name = "Accept", Value = "*/*" }),
                new ParameterViewModel(new Parameter { Name = "If-Match" }),
                new ParameterViewModel(new Parameter { Name = "" }),
            });

        /// <summary>
        /// リクエストヘッダ追加
        /// </summary>
        private void AddInHeader()
            => HttpHeaders.Add(new ParameterViewModel(new Parameter { Enabled = true, Name = "" }));

        /// <summary>
        /// @odata.etag の設定
        /// </summary>
        /// <param name="etag"></param>
        private void SetEtag(string etag)
        {
            var data = HttpHeaders.FirstOrDefault(x => x.Name.Value == "If-Match");
            if (data != null)
                data.Value.Value = etag;
        }

        /// <summary>
        /// リクエストパラメータ初期値
        /// </summary>
        private void InitParameter()
            => HttpParameters.Add(new ParameterViewModel(new Parameter { Name = "" }));

        /// <summary>
        /// リクエストパラメータ追加
        /// </summary>
        private void AddParameter()
            => HttpParameters.Add(new ParameterViewModel(new Parameter { Enabled = true, Name = "" }));

        /// <summary>
        /// Json整形
        /// </summary>
        private void JsonFormat()
        {
            try
            {
                var jsonObject = JsonConvert.DeserializeObject(HttpJsonBody.Value);
                HttpJsonBody.Value = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            }
            catch
            {
                _dialogService.AlertMessage("JSON 文字列ではないようです。");
            }
        }

        /// <summary>
        /// 設定情報を JSON データに変換する
        /// </summary>
        /// <returns></returns>
        private List<Setting> CreateSttings()
        {
            _redfishAdapter.Configure.ProxyPassword = CryptoAes.Encrypt(_redfishAdapter.Configure.ProxyPassword) ?? string.Empty;
            return
                [
                    new() { Id = 1, Json = JsonConvert.SerializeObject(_redfishAdapter.Configure) },
                    new() { Id = 2, Json = JsonConvert.SerializeObject(Keywords.ToList()) }
                ];
        }

        /// <summary>
        /// HTTPリクエスト情報の再設定
        /// </summary>
        private void ReRequest(Search search)
        {
            //// ぐるぐるを表示する
            //SearchProgressBar.Value = Visibility.Visible;

            // メソッド
            HttpMethod.Value = search.Method;

            // URI
            Keyword.Value = search.Keyword;

            // アカウント
            Username.Value = search.Username;
            Password.Value = search.Password;

            // HTTPリクエストヘッダ
            var etag = HttpHeaders.FirstOrDefault(x => Regex.IsMatch(x.Name.Value, "If-Match", RegexOptions.IgnoreCase));
            HttpHeaders.Clear();
            if (etag != null)
                HttpHeaders.Add(etag);
            search.Headers
                .ForEach(x => HttpHeaders.Add(new ParameterViewModel(new Parameter { Enabled = true, Name = x.Key, Value = x.Value })));

            // HTTPリクエストパラメータ
            HttpParameters.Clear();
            search.Parameters
                .ForEach(x => HttpParameters.Add(new ParameterViewModel(new Parameter { Enabled = true, Name = x.Key, Value = x.Value })));

            // HTTPリクエストJsonボディ
            HttpJsonBody.Value = search.JsonBody;
        }
    }
}
