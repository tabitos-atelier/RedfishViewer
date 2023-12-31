using NLog;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using RedfishViewer.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// Main VM
    /// </summary>
    public class MainWindowViewModel : BindableBase, IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        private readonly IRedfishAdapter _redfishAdapter;                       // Redfish RestAPI
        private readonly IDatabaseAgent _dbAgent;                               // DBアクセス

        public ReactivePropertySlim<string> Title { get; }                      // タイトル
        public AsyncReactiveCommand<CancelEventArgs> ClosingCommand { get; }    // アプリ終了

        /// <summary>
        /// MainWindowViewModel
        /// </summary>
        /// <param name="unityContainer"></param>
        public MainWindowViewModel(IUnityContainer unityContainer)
        {
            _logger.Trace($"{this.GetType().Name}.");

            unityContainer.Resolve<IRegionManager>()
                .RegisterViewWithRegion("ContentRegion", typeof(Views.Reqests));

            _redfishAdapter = unityContainer.Resolve< IRedfishAdapter>();
            _dbAgent = unityContainer.Resolve<IDatabaseAgent>();

            // タイトル
            Title = new ReactivePropertySlim<string>("Redfish Viwer")
                .AddTo(_disposables);

            // アプリケーション終了
            ClosingCommand = new AsyncReactiveCommand<CancelEventArgs>()
                .WithSubscribe(async _ => await ClosingAsync())
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();

        /// <summary>
        /// アプリ終了
        /// </summary>
        /// <returns></returns>
        private async Task ClosingAsync()
        {
            try
            {
                // 設定情報を保存する
                if (_redfishAdapter.GetSettings != null)
                {
                    _redfishAdapter.GetSettings()
                        .ForEach(item =>
                        {
                            _logger.Debug($"{item.Id}, {item.Json}");
                            var data = _dbAgent.GetSetting(item.Id);
                            if (data == null)
                            {
                                _dbAgent.AddSetting(item);
                            }
                            else
                            {
                                data.Json = item.Json;
                                _dbAgent.UpdateSetting(data);
                            }
                        });
                }

                // DB保存(諸々の変更情報を保存する)
                await _dbAgent.SaveDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "終了処理に失敗しました。");
            }
            _logger.Info("Finished.");
            Application.Current.Shutdown();     // アプリケーション終了
        }
    }
}
