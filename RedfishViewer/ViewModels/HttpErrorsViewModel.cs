using Newtonsoft.Json;
using NLog;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using RedfishViewer.Events;
using RedfishViewer.Models;
using RedfishViewer.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Unity;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// エラー情報VM
    /// </summary>
    public class HttpErrorsViewModel : BindableBase, IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        private readonly IEventAggregator _eventAggregator;                                 // IEventAggregator
        private readonly IRedfishAdapter _redfishAdapter;                                   // Redfish RestAPI
        private readonly IDatabaseAgent _dbAgent;                                           // DB アクセス

        // エラー情報リスト
        public ReactiveCommandSlim<RoutedEventArgs> LoadedCommand { get; }                  // エラー情報表示後(ナンバリングのため)
        public ReactiveCollection<HttpErrorViewModel> HttpErrors { get; set; }              // HTTPエラー
        public ReactivePropertySlim<int> HttpErrorIndex { get; set; }                       // 選択中のインデックス
        public ReactivePropertySlim<HttpErrorViewModel> HttpErrorItem { get; set; }         // 選択中のアイテム
        public ReactiveCommandSlim<DataGridRowEventArgs> LoadingRowCommand { get; }         // ナンバリング用
        public ReactiveCommand ReRequestCommand { get; }                                    // 再リクエスト
        public ReactiveCommand ClearErrorsCommand { get; }                                  // エラーのクリア

        // HTTPレスポンス
        public ReactiveCollection<KeyValueViewModel> OutHeaders { get; }                    // HTTPレスポンスヘッダ
        public ReactivePropertySlim<string> HttpContent { get; private set; }               // コンテンツ内容

        // HTTPレスポンス:結果中のリクエスト情報
        public ReactiveCollection<KeyValueViewModel> InHeaders { get; }                     // リクエストヘッダ
        public ReactiveCollection<KeyValueViewModel> InParameters { get; }                  // リクエストパラメータ
        public ReactivePropertySlim<string> InJsonBody { get; }                             // リクエストJSONボディ

        /// <summary>
        /// HttpErrorsViewModel
        /// </summary>
        /// <param name="unityContainer"></param>
        public HttpErrorsViewModel(IUnityContainer unityContainer)
        {
            _logger.Trace($"{this.GetType().Name}.");

            _eventAggregator = unityContainer.Resolve<IEventAggregator>();
            _redfishAdapter = unityContainer.Resolve<IRedfishAdapter>();
            _dbAgent = unityContainer.Resolve<IDatabaseAgent>();

            // エラー情報表示後
            LoadedCommand = new ReactiveCommandSlim<RoutedEventArgs>()
                .WithSubscribe(_ => Loaded())
                .AddTo(_disposables);

            // HTTPエラー
            HttpErrors = new ReactiveCollection<HttpErrorViewModel>()
                .AddTo(_disposables);
            // 選択中のインデックス
            HttpErrorIndex = new ReactivePropertySlim<int>()
                .AddTo(_disposables);
            // 選択中のアイテム
            HttpErrorItem = new ReactivePropertySlim<HttpErrorViewModel>()
                .AddTo(_disposables);
            HttpErrorItem
                .ObserveProperty(x => x.Value)
                .Subscribe(SelectionChanged)
                .AddTo(_disposables);
            // ナンバリング
            LoadingRowCommand = new ReactiveCommandSlim<DataGridRowEventArgs>()
                .WithSubscribe(e => e.Row.Header = $"{e.Row.GetIndex() + 1}")
                .AddTo(_disposables);
            // 再リクエスト
            ReRequestCommand = new ReactiveCommand()
                .WithSubscribe(ReRequest)
                .AddTo(_disposables);
            // エラーのクリア
            ClearErrorsCommand = new ReactiveCommand()
                .WithSubscribe(ClearErrors)
                .AddTo(_disposables);
            // エラー検知
            _eventAggregator
                .GetEvent<HttpErrorEvent<HttpError>>()
                .Subscribe(AddError, ThreadOption.UIThread)
                .AddTo(_disposables);

            // HTTPレスポンスヘッダー
            OutHeaders = new ReactiveCollection<KeyValueViewModel>()
                .AddTo(_disposables);

            // コンテンツ内容
            HttpContent = new ReactivePropertySlim<string>()
                .AddTo(_disposables);

            // HTTPレスポンス:結果中のリクエスト情報
            InHeaders = new ReactiveCollection<KeyValueViewModel>()
                .AddTo(_disposables);
            InParameters = new ReactiveCollection<KeyValueViewModel>()
                .AddTo(_disposables);
            InJsonBody = new ReactivePropertySlim<string>()
                .AddTo(_disposables);
        }

        /// <summary>
        /// エラー情報の表示
        /// </summary>
        private void Loaded()
        {
            HttpErrors.Clear();
            _redfishAdapter.HttpErrors
                .ToList()
                .ForEach(x => HttpErrors.Add(new HttpErrorViewModel(x)));
            HttpErrorIndex.Value = 0;
        }

        /// <summary>
        /// エラー詳細の表示
        /// </summary>
        private void SelectionChanged(HttpErrorViewModel item)
        {
            if (item == null)
                return;

            // HTTPレスポンスヘッダー
            OutHeaders.Clear();
            JsonConvert.DeserializeObject<ObservableCollection<KeyValue>>(item.OutHeaders.Value)?
                .ToList()
                .ForEach(x => OutHeaders.Add(new KeyValueViewModel(new KeyValue(x.Key, x.Value))));

            // 現行コンテンツ
            var content = item.Content.Value;
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    content = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented);
                }
                catch
                {
                }
            }
            HttpContent.Value = content;

            // HTTPリクエストヘッダー
            InHeaders.Clear();
            if (!string.IsNullOrEmpty(item.InHeaders.Value))
                JsonConvert.DeserializeObject<ObservableCollection<KeyValue>>(item.InHeaders.Value)?
                    .ToList()
                    .ForEach(x => InHeaders.Add(new KeyValueViewModel(new KeyValue(x.Key, x.Value))));

            // HTTPリクエストパラメータ
            InParameters.Clear();
            if (!string.IsNullOrEmpty(item.Parameters.Value))
                JsonConvert.DeserializeObject<ObservableCollection<KeyValue>>(item.Parameters.Value)?
                    .ToList()
                    .ForEach(x => InParameters.Add(new KeyValueViewModel(new KeyValue(x.Key, x.Value))));

            // HTTPリクエストJSONボディ
            var jsonBody = item.JsonBody.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(jsonBody))
            {
                try
                {
                    jsonBody = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(jsonBody), Formatting.Indented);
                }
                catch
                {
                }
            }
            InJsonBody.Value = jsonBody;
        }

        public void Destroy()
            => _disposables.Dispose();

        /// <summary>
        /// 再リクエスト
        /// </summary>
        private void ReRequest()
        {
            var item = HttpErrorItem.Value;
            if (item == null)
                return;

            var uri = new Uri(item.Uri.Value);

            // アカウント情報の取得
            var node = _dbAgent.GetNode(RedfishRestSharp.GenerateRootUri(uri));
            if (node == null)
                return;

            // 検索条件
            var search = new Search
            {
                Keyword = uri.AbsoluteUri,                                      // URI
                Method = item.Method.Value,                                     // リクエストメソッド
                IsAutoDive = false,                                             // 自動検索しない
                Username = node.Username ?? string.Empty,                       // Basic認証:ユーザ名
                Password = CryptoAes.Decrypt(node.Password) ?? string.Empty,    // Basic認証:パスワード
                Headers = [],                                                   // ヘッダ
                Parameters = [],                                                // パラメータ
                JsonBody = item.JsonBody.Value ?? string.Empty,                 // HTTPリクエスト:JSONボディ
            };

            // ヘッダ
            if (!string.IsNullOrEmpty(item.InHeaders.Value))
                JsonConvert.DeserializeObject<ObservableCollection<KeyValue>>(item.InHeaders.Value)?
                .ToList()
                .ForEach(x =>
                {
                    if (!Regex.IsMatch(x.Key, "If-Match", RegexOptions.IgnoreCase))
                        search.Headers.Add(x);
                });

            // パラメータ
            if (!string.IsNullOrEmpty(item.Parameters.Value))
                JsonConvert.DeserializeObject<ObservableCollection<KeyValue>>(item.Parameters.Value)?
                .ToList()
                .ForEach(x => search.Parameters.Add(x));

            // HTTPリクエストのViewの情報を書き換える
            _eventAggregator
                .GetEvent<ReRequestEvent<Search>>()
                .Publish(search);

            //// 過去の設定で、再度HTTPリクエストを実行する
            //_eventAggregator
            //    .GetEvent<SearchEvent<Search>>()
            //    .Publish(search);
        }

        /// <summary>
        /// エラークリア
        /// </summary>
        private void ClearErrors()
        {
            _redfishAdapter.HttpErrors.Clear();
            HttpErrors.Clear();
            OutHeaders.Clear();
            HttpContent.Value = string.Empty;
            InHeaders.Clear();
            InParameters.Clear();
            InJsonBody.Value = string.Empty;
        }

        /// <summary>
        /// エラー追加
        /// </summary>
        /// <param name="item"></param>
        private void AddError(HttpError item)
        {
            var data = HttpErrors.FirstOrDefault(x => x.Created.Value == item.Created);
            if (data == null)
                HttpErrors.Add(new HttpErrorViewModel(item));
        }
    }
}
