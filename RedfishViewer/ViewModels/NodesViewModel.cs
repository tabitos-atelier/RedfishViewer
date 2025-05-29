using AutoMapper;
using NLog;
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
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Unity;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// ノード情報 VM
    /// </summary>
    public class NodesViewModel : BindableBase, IDestructible, INavigationAware
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        private readonly IRegionManager _regionManager;                             // IRegionManager
        private readonly IDialogService _dialogService;                             // IDialogService
        private readonly IEventAggregator _eventAggregator;                         // IEventAggregator
        private readonly IRedfishAdapter _redfishAdapter;                           // Redfish RestAPI
        private readonly IDatabaseAgent _dbAgent;                                   // DBアクセス

        private readonly Mapper _mapper;                                            // ViewMode <-> Modelマッパー

        // プラグイン
        public ReactiveCollection<string> PluginNames { get; }                      // プラグイン
        public ReactiveProperty<string?> PluginName { get; }                        // 選択中のプラグイン
        public ReactiveProperty<bool> PluginEnabled { get; }                        // プラグインの有効・無効

        // アクション１
        public AsyncReactiveCommand Action1Command { get; }                         // アクション1
        public ReactiveProperty<string> Action1Content { get; }                     // アクション1名称
        public ReactiveProperty<string> Action1ToolTip { get; }                     // アクション1ヒント
        public ReactiveProperty<bool> Action1Enabled { get; }                       // アクション1有効・無効

        // アクション２
        public AsyncReactiveCommand Action2Command { get; }                         // アクション2
        public ReactiveProperty<string> Action2Content { get; }                     // アクション2名称
        public ReactiveProperty<string> Action2ToolTip { get; }                     // アクション2ヒント
        public ReactiveProperty<bool> Action2Enabled { get; }                       // アクション2有効・無効

        // View表示後
        public ReactiveCommandSlim<RoutedEventArgs> LoadedCommand { get; }          // 表示後(ナンバリングのため)

        // ノード情報
        public ReactiveCollection<NodeViewModel> Nodes { get; set; }                // ノード情報
        public ReactivePropertySlim<int> NodeIndex { get; set; }                    // 選択中の行番号
        public ReactivePropertySlim<NodeViewModel> NodeItem { get; set; }           // 選択中のアイテム
        public ReactiveCommandSlim<DataGridRowEventArgs> LoadingRowCommand { get; } // ナンバリング用
        public AsyncReactiveCommand SaveNodesCommand { get; }                       // 編集情報の保存

        // 右クリックメニュー
        public ReactiveCommandSlim SetAccountCommand { get; }                       // アカウント設定
        public ReactiveCommandSlim LoadResultsCommand { get; }                      // レスポンス結果の読み込み
        public AsyncReactiveCommand RemoveResultsCommand { get; }                   // 関連情報の削除

        /// <summary>
        /// NodesViewModel
        /// </summary>
        /// <param name="unityContainer"></param>
        public NodesViewModel(IUnityContainer unityContainer)
        {
            _logger.Trace($"{this.GetType().Name}.");

            _regionManager = unityContainer.Resolve<IRegionManager>();
            _dialogService = unityContainer.Resolve<IDialogService>();
            _eventAggregator = unityContainer.Resolve<IEventAggregator>();
            _redfishAdapter = unityContainer.Resolve<IRedfishAdapter>();
            _dbAgent = unityContainer.Resolve<IDatabaseAgent>();

            // NodeViewModel を Node に変換する(読み込み時は不要)
            var config = new MapperConfiguration(cfg => cfg.CreateMap<NodeViewModel, Node>()
                .ForMember(x => x.RootUri, option => option.MapFrom(x => x.RootUri.Value))
                .ForMember(x => x.Username, option => option.MapFrom(x => x.Username.Value))
                .ForMember(x => x.Password, option => option.MapFrom(x => x.Password.Value))
                .ForMember(x => x.Created, option => option.MapFrom(x => x.Created.Value))
                .ForMember(x => x.Updated, option => option.MapFrom(x => x.Updated.Value))
                .ForMember(x => x.Plugin, option => option.MapFrom(x => x.Plugin.Value))
                .ForMember(x => x.Title, option => option.MapFrom(x => x.Title.Value))
                .ForMember(x => x.Summary, option => option.MapFrom(x => x.Summary.Value))
                .ForMember(x => x.Note, option => option.MapFrom(x => x.Note.Value)));
            _mapper = new Mapper(config);

            // プラグイン
            PluginNames = new ReactiveCollection<string>()
                .AddTo(_disposables);
            PluginNames.AddRange(_redfishAdapter.Plugins.Select(x => x.Key));
            PluginName = new ReactiveProperty<string?>()
                .AddTo(_disposables);
            PluginName
                .ObserveProperty(x => x.Value)
                .Subscribe(SelectionChanged)
                .AddTo(_disposables);
            PluginEnabled = new ReactiveProperty<bool>(false)
                .AddTo(_disposables);

            // アクション1
            Action1Command = new AsyncReactiveCommand()
                .WithSubscribe(Action1Async)
                .AddTo(_disposables);
            Action1Content = new ReactiveProperty<string>("Act1")
                .AddTo(_disposables);
            Action1ToolTip = new ReactiveProperty<string>()
                .AddTo(_disposables);
            Action1Enabled = new ReactiveProperty<bool>(false)
                .AddTo(_disposables);

            // アクション2
            Action2Command = new AsyncReactiveCommand()
                .WithSubscribe(Action2Async)
                .AddTo(_disposables);
            Action2Content = new ReactiveProperty<string>("Act2")
                .AddTo(_disposables);
            Action2ToolTip = new ReactiveProperty<string>()
                .AddTo(_disposables);
            Action2Enabled = new ReactiveProperty<bool>(false)
                .AddTo(_disposables);

            // 画面表示後の開始処理
            LoadedCommand = new ReactiveCommandSlim<RoutedEventArgs>()
                .WithSubscribe(_ => Loaded())
                .AddTo(_disposables);

            // 表示中のノード情報
            Nodes = new ReactiveCollection<NodeViewModel>()
                .AddTo(_disposables);
            // 選択中の行番号
            NodeIndex = new ReactivePropertySlim<int>()
                .AddTo(_disposables);
            // 選択中のアイテム
            NodeItem = new ReactivePropertySlim<NodeViewModel>()
                .AddTo(_disposables);
            NodeItem
                .ObserveProperty(x => x.Value)
                .Subscribe(SelectionChanged)
                .AddTo(_disposables);

            // ナンバリング
            LoadingRowCommand = new ReactiveCommandSlim<DataGridRowEventArgs>()
                .WithSubscribe(e => e.Row.Header = $"{e.Row.GetIndex() + 1}")
                .AddTo(_disposables);

            // データベースに保存する
            SaveNodesCommand = new AsyncReactiveCommand()
                .WithSubscribe(SaveNodesAsync)
                .AddTo(_disposables);

            // 右メニュー:アカウント情報を反映する
            SetAccountCommand = new ReactiveCommandSlim()
                .WithSubscribe(SetAccount)
                .AddTo(_disposables);

            // 右メニュー:データベースからレスポンス情報を読み込んで、画面を遷移する
            LoadResultsCommand = new ReactiveCommandSlim()
                .WithSubscribe(LoadSpecificResults)
                .AddTo(_disposables);

            // 右メニュー:データベースからノード情報とレスポンス情報を削除する
            RemoveResultsCommand = new AsyncReactiveCommand()
                .WithSubscribe(RemoveSpecificResultsAsync)
                .AddTo(_disposables);

            // ノード情報更新のイベント検知
            _eventAggregator
                .GetEvent<SaveNodeEvent<Node>>()
                .Subscribe(SaveNode, ThreadOption.UIThread)
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();

        /// <summary>
        /// 選択行の変更イベント
        /// </summary>
        /// <param name="name"></param>
        private void SelectionChanged(string? name)
        {
            if (name == null)
                return;

            // プラグイン名称
            var node = NodeItem.Value;
            node.Plugin.Value = name;

            // プラグイン設定
            var obj = _redfishAdapter.Plugins.FirstOrDefault(x => x.Key == name);
            var plugin = obj.Value;
            if (plugin == null)
            {
                Action1Content.Value = string.Empty;
                Action1ToolTip.Value = string.Empty;
                Action1Enabled.Value = false;
                Action2Content.Value = string.Empty;
                Action2ToolTip.Value = string.Empty;
                Action2Enabled.Value = false;
            }
            else
            {
                Action1Content.Value = plugin.Action1Name;
                Action1ToolTip.Value = plugin.Action1Hint;
                Action1Enabled.Value = !string.IsNullOrEmpty(plugin.Action1Name);
                Action2Content.Value = plugin.Action2Name;
                Action2ToolTip.Value = plugin.Action2Hint;
                Action2Enabled.Value = !string.IsNullOrEmpty(plugin.Action2Name);
            }
        }

        /// <summary>
        /// アクション１
        /// </summary>
        /// <returns></returns>
        private async Task Action1Async()
        {
            var name = PluginName.Value;
            if (name == null || name == "None")
                return;

            var obj = _redfishAdapter.Plugins.FirstOrDefault(x => x.Key == name);
            var plugin = obj.Value;
            if (plugin == null)
                return;

            var node = NodeItem.Value;
            await plugin.Action1ExecuteAsync(node.RootUri.Value, node.Username.Value, node.DecryptPassword.Value);
        }

        /// <summary>
        /// アクション２
        /// </summary>
        /// <returns></returns>
        private async Task Action2Async()
        {
            var name = PluginName.Value;
            if (name == null || name == "None")
                return;

            var obj = _redfishAdapter.Plugins.FirstOrDefault(x => x.Key == name);
            var plugin = obj.Value;
            if (plugin == null)
                return;

            var node = NodeItem.Value;
            await plugin.Action2ExecuteAsync(node.RootUri.Value, node.Username.Value, node.DecryptPassword.Value);
        }

        /// <summary>
        /// View 表示後
        /// </summary>
        private void Loaded()
        {
            Nodes.Clear();
            _dbAgent.GetNodes()?
                .ToList()
                .ForEach(x => Nodes.Add(new NodeViewModel(x)));
        }

        /// <summary>
        /// 選択行の変更イベント
        /// </summary>
        /// <param name="model"></param>
        private void SelectionChanged(NodeViewModel model)
        {
            // プラグイン ComboBox の有効・無効
            PluginEnabled.Value = model != null;

            // 行を選択しているかを確認する
            var name = model?.Plugin.Value;
            if (name == null)
                return;

            // プラグイン情報取得
            var obj = _redfishAdapter.Plugins.FirstOrDefault(x => x.Key == name);
            if (obj.Key == null)
                return;

            // プラグイン設定
            PluginName.Value = name;
            SelectionChanged(name);
        }

        /// <summary>
        /// 全てのノード情報を保存する
        /// </summary>
        /// <returns></returns>
        private async Task SaveNodesAsync()
        {
            _logger.Trace("SaveNodesAsync.");

            // 編集内容をコミットする
            ((IEditableCollectionView)CollectionViewSource.GetDefaultView(Nodes)).CommitEdit();

            // ノード情報をデータベースに保存する
            var nodes = _mapper.Map<List<Node>>(Nodes);
            foreach (var node in nodes)
            {
                _logger.Debug($"{node.Title},{node.RootUri},{node.Username},{node.Plugin},{node.Note}");
                var data = _dbAgent.GetNode(node.RootUri);
                if (data == null)
                    continue;
                data.Updated = DateTime.Now;
                data.Plugin = node.Plugin;
                data.Title = node.Title;
                data.Summary= node.Summary;
                data.Note = node.Note;
                await _dbAgent.UpdateNodeAsync(data);
            }
            await _dbAgent.SaveDatabaseAsync();
        }

        /// <summary>
        /// ノード情報の更新
        /// </summary>
        private void SaveNode(Node node)
        {
            var data = Nodes.FirstOrDefault(x => x.RootUri.Value == node.RootUri);
            if (data != null)
            {
                data.Username.Value = node.Username;
                data.Password.Value = node.Password;
                data.Commit();
            }
            else
                Nodes.Add(new NodeViewModel(node));
        }

        /// <summary>
        /// アカウントの設定
        /// </summary>
        private void SetAccount()
        {
            var item = NodeItem.Value;
            _eventAggregator
                .GetEvent<SetAccountEvent<KeyValuePair<string?, string?>>>()
                .Publish(new KeyValuePair<string?, string?>(item.Username.Value, item.DecryptPassword.Value));
        }

        /// <summary>
        /// 特定ホストの読み込み
        /// </summary>
        private void LoadSpecificResults()
        {
            _eventAggregator
                .GetEvent<LoadSpecificResultsEvent<string>>()
                .Publish(NodeItem.Value.RootUri.Value);
            _regionManager.RequestNavigate("ContentBody", nameof(Responses));   // Viewを切替える
        }

        /// <summary>
        /// 指定したホストに関するノード情報の削除
        /// </summary>
        /// <returns></returns>
        private async Task RemoveSpecificResultsAsync()
        {
            var rootUri = NodeItem.Value.RootUri.Value;
            var msg = new StringBuilder();
            msg.AppendLine("ノード情報をデータベースから削除しますか？");
            msg.AppendLine("削除を実行すると、選択中のノード情報に関して、これまでに保存した全ての情報が削除されます。\n");
            msg.AppendLine(rootUri);
            var result = _dialogService.YesNoMessage(msg.ToString(), "Delete");
            if (result != ButtonResult.Yes)
                return;

            _dbAgent.RemoveNode(rootUri);                   // 指定ホストのレコードを削除
            _dbAgent.RemoveSameRootUriResults(rootUri);     // 指定ホストを含むレコードを削除
            await _dbAgent.SaveDatabaseAsync();             // 削除の確定
            Nodes.RemoveAt(NodeIndex.Value);                // Viewからも削除
        }

        /// <summary>
        /// この画面が表示される前
        /// </summary>
        /// <param name="navigationContext"></param>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
            => false;   // 毎回新規

        /// <summary>
        /// 別画面への移動直前
        /// </summary>
        /// <param name="navigationContext"></param>
        public async void OnNavigatedFrom(NavigationContext navigationContext)
            => await SaveNodesAsync();
    }
}
