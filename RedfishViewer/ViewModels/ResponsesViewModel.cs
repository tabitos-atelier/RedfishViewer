using Newtonsoft.Json;
using NLog;
using Notification.Wpf;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Navigation.Regions;
using Prism.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedfishViewer.Events;
using RedfishViewer.Models;
using RedfishViewer.Services;
using RedfishViewer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Unity;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// HTTPレスポンス VM
    /// </summary>
    public class ResponsesViewModel : BindableBase, IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly NotificationManager _notificationManager = new();
        private readonly CompositeDisposable _disposables = [];

        private static readonly char[] separator = ['\n', '\r'];

        private readonly IRegionManager _regionManager;                                 // IRegionManager
        private readonly IDialogService _dialogService;                                 // IDialogService
        private readonly IEventAggregator _eventAggregator;                             // IEventAggregator
        private readonly IRedfishAdapter _redfishAdapter;                               // Redfish RestAPI
        private readonly IDatabaseAgent _dbAgent;                                       // DB アクセス

        private string? _searchWord;                                                    // 検索ワード
        private readonly HashSet<string> _branches = [];                                // 自動検索のURL一覧
        private readonly ObservableCollection<Result> _results = [];                    // レスポンス情報
        private ObservableCollection<Result>? _backupResults;                           // 検索用バックアップ
        private bool _autoDiveStatus = false;                                           // 自動検索スイッチ
        private bool _autoDiveMessageFlag = false;                                      // 自動検索中断メッセージ

        // HTTPレスポンス:結果
        public ReadOnlyReactiveCollection<ResultViewModel> Results { get; }             // レスポンス情報
        public ReactivePropertySlim<ResultViewModel> ResultItem { get; set; }           // 選択中のアイテム
        public ReactivePropertySlim<int> ResultIndex { get; }                           // 選択中の行番号
        public ReactiveCommand<DataGridRowEventArgs> LoadingRowCommand { get; }         // ナンバリング
        public ReactiveCommand ReRequestCommand { get; }                                // 再リクエスト
        public ReactiveCommand ClearResultsCommand { get; }                             // 結果クリア

        // HTTPレスポンス:コンテンツ
        public ReactivePropertySlim<FlowDocument> HttpContent { get; private set; }     // コンテンツ内容

        // HTTPレスポンス:現旧比較
        public ReactivePropertySlim<bool> IsSideBySide { get; private set; }            // 新旧の表示形式
        public ReactivePropertySlim<string> NewText { get; private set; }               // 現行コンテンツ
        public ReactivePropertySlim<string> OldText { get; private set; }               // 過去コンテンツ

        // HTTPレスポンス:ヘッダ
        public ReactiveCollection<KeyValueViewModel> OutHeaders { get; }                // レスポンスヘッダ

        // HTTPレスポンス:結果中のリクエスト情報
        public ReactiveCollection<KeyValueViewModel> InHeaders { get; }                 // リクエストヘッダ
        public ReactiveCollection<KeyValueViewModel> InParameters { get; }              // リクエストパラメータ
        public ReactivePropertySlim<string> InJsonBody { get; }                         // リクエストJSONボディ

        /// <summary>
        /// ResponsesViewModel
        /// </summary>
        /// <param name="unityContainer"></param>
        public ResponsesViewModel(IUnityContainer unityContainer)
        {
            _logger.Trace($"{this.GetType().Name}.");

            _regionManager = unityContainer.Resolve<IRegionManager>();
            _dialogService = unityContainer.Resolve<IDialogService>();
            _eventAggregator = unityContainer.Resolve<IEventAggregator>();
            _redfishAdapter = unityContainer.Resolve<IRedfishAdapter>();
            _dbAgent = unityContainer.Resolve<IDatabaseAgent>();

            // 検索開始
            _eventAggregator
                .GetEvent<SearchEvent<Search>>()
                .Subscribe(async x => await SearchAsync(x))
                .AddTo(_disposables);
            // 自動検索中断
            _eventAggregator
                .GetEvent<AutoDiveOffEvent<bool>>()
                .Subscribe(_ => CancelAutoDive(), ThreadOption.UIThread)
                .AddTo(_disposables);
            // 特定ホストの結果をDBから読み込む
            _eventAggregator
                .GetEvent<LoadSpecificResultsEvent<string>>()
                .Subscribe(LoadSpecificResults, ThreadOption.UIThread)
                .AddTo(_disposables);

            // HTTPレスポンスの結果一覧
            Results = _results
                .ToReadOnlyReactiveCollection(x => new ResultViewModel(x))
                .AddTo(_disposables);
            // 選択中のアイテム
            ResultItem = new ReactivePropertySlim<ResultViewModel>()
                .AddTo(_disposables);
            ResultItem
                .ObserveProperty(x => x.Value)
                .Subscribe(SelectionChanged)
                .AddTo(_disposables);
            // 選択中の行番号
            ResultIndex = new ReactivePropertySlim<int>()
                .AddTo(_disposables);
            // ナンバリング
            LoadingRowCommand = new ReactiveCommand<DataGridRowEventArgs>()
                .WithSubscribe(e => e.Row.Header = $"{e.Row.GetIndex() + 1}")
                .AddTo(_disposables);
            // 再リクエスト
            ReRequestCommand = new ReactiveCommand()
                .WithSubscribe(ReRequest)
                .AddTo(_disposables);
            // 結果クリア
            ClearResultsCommand = new ReactiveCommand()
                .WithSubscribe(ClearResults)
                .AddTo(_disposables);

            // コンテンツ内容
            HttpContent = new ReactivePropertySlim<FlowDocument>()
                .AddTo(_disposables);

            // 新旧比較の表示形式
            IsSideBySide = new ReactivePropertySlim<bool>(true)
                .AddTo(_disposables);
            // 現行コンテンツ
            NewText = new ReactivePropertySlim<string>()
                .AddTo(_disposables);
            // 過去コンテンツ
            OldText = new ReactivePropertySlim<string>()
                .AddTo(_disposables);

            // レスポンスヘッダー
            OutHeaders = new ReactiveCollection<KeyValueViewModel>()
                .AddTo(_disposables);

            // HTTPレスポンス:結果中のリクエスト情報
            InHeaders = new ReactiveCollection<KeyValueViewModel>()
                .AddTo(_disposables);
            InParameters = new ReactiveCollection<KeyValueViewModel>()
                .AddTo(_disposables);
            InJsonBody = new ReactivePropertySlim<string>()
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="search"></param>
        private async Task SearchAsync(Search search)
        {
            // 検索開始
            var keyword = search.Keyword;
            if (Regex.IsMatch(keyword, "^(http|https)://"))
            {
                _searchWord = null;
                _backupResults = null;

                // Redfish(Web)検索
                var response = await _redfishAdapter.RequestAsync(search);
                if (response.HttpError != null)
                {
                    _regionManager.RequestNavigate("ContentBody", nameof(HttpErrors));   // Viewを切替える

                    // エラー処理
                    var errmsg = response.HttpError.Message;
                    if (0 < response.HttpError.StatusCode)
                        errmsg = $"{response.HttpError.StatusCode}:{errmsg}";
                    if (response.RestResponse?.ErrorException != null)
                        errmsg += "\n\n" + response.RestResponse.ErrorException.Message;
                    _dialogService.AlertMessage(errmsg);

                    // 終了処理
                    _eventAggregator
                        .GetEvent<PostSearchEvent<string?>>()
                        .Publish(null);
                    return;
                }

                // 結果取得
                var result = response.Result;
                if (result == null)
                    return;

                keyword = result.Uri;
                if (search.IsAutoDive)
                    _results.Clear();

                // @odata.etag 更新
                var etag = _redfishAdapter.GetOdataEtag(result.Content);
                if (0 < etag.Length)
                    _eventAggregator
                        .GetEvent<SetEtagEvent<string>>()
                        .Publish(etag);

                // View 更新
                var data = _results.FirstOrDefault(x => x.Uri == search.Keyword);
                if (data != null)
                    _results.Remove(data);
                _results.Add(await SaveResultAsync(result));

                // ノード情報を保存する
                if (response.Uri != null)
                {
                    var node = new Node
                    {
                        RootUri = RedfishRestSharp.GenerateRootUri(response.Uri),
                        Username = search.Username,
                        Password = CryptoAes.Encrypt(search.Password),
                        Updated = DateTime.Now,
                    };
                    await SaveNodeAsync(node);
                    _eventAggregator
                        .GetEvent<SaveNodeEvent<Node>>()
                        .Publish(node);
                }

                // 自動検索のスイッチを確認する
                if (search.IsAutoDive)
                {
                    _autoDiveStatus = _autoDiveMessageFlag = true;
                    _branches.Clear();
                    _branches.Add(search.Keyword);
                    foreach (var uri in _redfishAdapter.GetOdataIds(result))
                    {
                        search.Keyword = uri;
                        search.ParentUri = result.Uri;
                        await AutoDivingAsync(search);
                    }
                    _autoDiveStatus = _autoDiveMessageFlag = false;
                    var msg = new NotificationContent { Type = NotificationType.Success, Title = "RedfishViewer 自動取得", Message = "全てのコンテンツを取得しました。" };
                    _notificationManager.Show(msg, "ToastArea", onClose: () => _notificationManager.Show(msg));
                }
            }
            else
            {
                _searchWord = keyword;
                _backupResults ??= new ObservableCollection<Result>(_results);
                _results.Clear();
                _results.AddRange(_backupResults.Where(x => x.Content != null && x.Content.Contains(keyword, StringComparison.CurrentCultureIgnoreCase)));
            }

            // キーワード設定
            _eventAggregator
                .GetEvent<PostSearchEvent<string?>>()
                .Publish(keyword);

            // 最終行をフォーカスする
            await Task.Delay(IRedfishAdapter.DelayedDisplay);       // 反映は遅延が必要
            if (0 < _results.Count && ResultIndex.Value < 0)
                ResultIndex.Value = _results.Count - 1;
        }

        /// <summary>
        /// 自動検索の中止
        /// </summary>
        private void CancelAutoDive()
        {
            // 自動検索の中断を確認する            
            if (_autoDiveStatus && _autoDiveMessageFlag)
            {
                var buttonResult = _dialogService.YesNoMessage("自動検索を中断しますか？", "AutorenewOff");
                if (buttonResult == ButtonResult.Yes)
                    _autoDiveStatus = _autoDiveMessageFlag = false;
                else
                    _eventAggregator
                        .GetEvent<AutoDiveOnEvent<bool>>()
                        .Publish(true);
            }
            else
                _autoDiveStatus = _autoDiveMessageFlag = false;
        }

        /// <summary>
        /// 自動検索
        /// </summary>
        /// <returns></returns>
        private async Task AutoDivingAsync(Search search)
        {
            // 自動検索中断判定
            if (!_autoDiveStatus)
                return;

            // 二重検索しない
            if (_branches.Contains(search.Keyword))
                return;
            _branches.Add(search.Keyword);

            _notificationManager.Show(new NotificationContent { Message = search.Keyword }, "ToastArea");
            var response = await _redfishAdapter.RequestAsync(search);
            if (response.HttpError != null)
            {
                // エラー処理
                var errmsg = new NotificationContent { Type = NotificationType.Error, Message = response.HttpError.Message, };
                _notificationManager.Show(errmsg, "ToastArea", onClose: () => _notificationManager.Show(errmsg));
                return;
            }

            // 結果取得
            var result = response.Result;
            if (result == null)
                return;

            // View 更新
            _results.Add(await SaveResultAsync(result));

            // 再帰自動検索
            foreach (var uri in _redfishAdapter.GetOdataIds(result))
            {
                search.Keyword = uri;
                search.ParentUri = result.Uri;
                await AutoDivingAsync(search);
            }
        }

        /// <summary>
        /// 結果をデータベースに保存する
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task<Result> SaveResultAsync(Result result)
        {
            var record = _dbAgent.GetResult(result.Uri);
            if (record == null)
            {
                await _dbAgent.AddResultAsync(result);
            }
            else
            {
                // 既存のデータとコンテンツが異なる場合、データを更新する。
                var hash1 = string.Join("", SHA256.HashData(Encoding.UTF8.GetBytes(result.Content ?? "")).Select(x => $"{x:x2}"));
                var hash2 = string.Join("", SHA256.HashData(Encoding.UTF8.GetBytes(record.Content ?? "")).Select(x => $"{x:x2}"));
                if (hash1 != hash2)
                {
                    record.LastContent = record.Content;                // 既存を前回エリアへ移動する
                    record.LastUpdated = record.Updated;                // 更新日付も前回エリアへ移動する
                    record.Content = result.Content ?? string.Empty;    // 今回のコンテンツ
                }
                record.Updated = result.Updated;                        // 今回の日付
                await _dbAgent.UpdateResultAsync(record);               // データベースの情報を更新する

                // データベースの前回分を入れる
                result.LastContent = record.LastContent;
                result.LastUpdated = record.LastUpdated;
            }
            await _dbAgent.SaveDatabaseAsync();
            return result;
        }

        /// <summary>
        /// 選択行の変更イベント
        /// </summary>
        private void SelectionChanged(ResultViewModel model)
        {
            if (model == null)
                return;

            // 現旧コンテンツ
            var newText = model.Content.Value;
            if (newText != null && model.IsJsonText.Value)
                newText = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(newText), Formatting.Indented);
            var oldText = model.LastContent.Value;
            if (oldText != null && model.IsJsonText.Value)
                oldText = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(oldText), Formatting.Indented);

            // 現行コンテンツ
            var paragraph = new Paragraph();
            if (_searchWord != null)
            {
                newText ??= string.Empty;
                foreach (var line in newText.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    int i;
                    var pool = line;
                    while (0 <= (i = pool.IndexOf(_searchWord, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        // キーワードより前の文字列を抜き出す。
                        if (1 < i)
                        {
                            paragraph.Inlines.Add(pool[..i]);           // キーワードより前の文字列を格納する
                            pool = pool[i..];                           // 格納分をスキップ
                        }

                        // キーワードをハイライトして格納する。
                        var work = pool[0.._searchWord.Length];         // ヒットしたキーワードを抜き出す
                        paragraph.Inlines.Add(new Run(work)
                        {
                            Background = Brushes.Yellow
                        });

                        // 残りの文字列を格納する
                        pool = pool[work.Length..];                     // キーワードより後ろの文字列に移動する
                    }
                    if (0 < pool.Length) paragraph.Inlines.Add(pool);   // キーワードより後ろの文字列
                    paragraph.Inlines.Add(new LineBreak());             // 改行を追加する(Split してるので改行を追加)
                }
            }
            else
            {
                paragraph.Inlines.Add(new TextBox
                {
                    IsReadOnly = true,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Text = newText,
                    TextWrapping = TextWrapping.NoWrap,
                });
            }
            HttpContent.Value = new FlowDocument(paragraph)
            {
                FontFamily = new FontFamily("Consolas, Meiryo UI"),
                FontWeight = FontWeights.Medium,
                FontSize = 13,
            };

            // 新旧比較
            IsSideBySide.Value = oldText != null;
            NewText.Value = newText ?? "";
            OldText.Value = oldText ?? "";

            // HTTPレスポンスヘッダー
            OutHeaders.Clear();
            JsonConvert.DeserializeObject<ObservableCollection<KeyValue>>(model.OutHeaders.Value)?
                .ToList()
                .ForEach(x => OutHeaders.Add(new KeyValueViewModel(new KeyValue (x.Key, x.Value))));
            
            // HTTPリクエストヘッダー
            InHeaders.Clear();
            if (!string.IsNullOrEmpty(model.InHeaders.Value))
                JsonConvert.DeserializeObject<ObservableCollection<KeyValue>>(model.InHeaders.Value)?
                    .ToList()
                    .ForEach(x => InHeaders.Add(new KeyValueViewModel(new KeyValue(x.Key, x.Value))));

            // HTTPリクエストパラメータ
            InParameters.Clear();
            if (!string.IsNullOrEmpty(model.Parameters.Value))
                JsonConvert.DeserializeObject<ObservableCollection<KeyValue>>(model.Parameters.Value)?
                    .ToList()
                    .ForEach(x => InParameters.Add(new KeyValueViewModel(new KeyValue(x.Key, x.Value))));

            // HTTPリクエストJSONボディ
            try
            {
                var json = model.JsonBody.Value;
                InJsonBody.Value = string.IsNullOrEmpty(json) ?
                    string.Empty :
                    JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);
            }
            catch
            {
                InJsonBody.Value = string.Empty;
            }
        }

        /// <summary>
        /// 再リクエスト
        /// </summary>
        private void ReRequest()
        {
            var item = ResultItem.Value;
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
                Method = item.Method.Value.ToUpper(),                           // リクエストメソッド
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
            //await SearchAsync(search);
        }

        /// <summary>
        /// 結果のクリア
        /// </summary>
        private void ClearResults()
        {
            // HTTPレスポンス
            _results.Clear();                       // 結果リスト
            HttpContent.Value = new FlowDocument(); // HTTPレスポンスコンテンツ
            OutHeaders.Clear();                     // HTTPレスポンスヘッダ

            // 前回比較
            IsSideBySide.Value = false;             // サイドバイサイド・モード
            NewText.Value = string.Empty;           // 新テキスト
            OldText.Value = string.Empty;           // 旧テキスト

            // HTTPリクエスト
            InHeaders.Clear();                      // HTTPリクエストヘッダ
            InParameters.Clear();                   // HTTPリクエストパラメータ
            InJsonBody.Value = string.Empty;        // HTTPリクエストJsonボディ
        }


        /// <summary>
        /// ノード情報を保存する
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private async Task SaveNodeAsync(Node node)
        {
            _logger.Trace("SaveNodeAsync.");

            var data = _dbAgent.GetNode(node.RootUri);
            if (data != null)
            {
                data.Username = node.Username;
                data.Password = node.Password;
                data.Updated = node.Updated;
                await _dbAgent.UpdateNodeAsync(data);   // 更新
            }
            else
            {
                node.Created = DateTime.Now;
                await _dbAgent.AddNodeAsync(node);      // 追加
            }
            await _dbAgent.SaveDatabaseAsync();
        }

        /// <summary>
        /// 特定ホストの読み込み
        /// </summary>
        /// <param name="rootUri"></param>
        /// <returns></returns>
        private void LoadSpecificResults(string rootUri)
        {
            _results.Clear();
            _dbAgent.GetSameRootUriResults(rootUri)?
                .ToList()
                .ForEach(x => _results.Add(x));
        }
    }
}
