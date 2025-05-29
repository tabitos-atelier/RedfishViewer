using MaterialDesignThemes.Wpf;
using NLog;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedfishViewer.Models;
using RedfishViewer.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Unity;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// アプリ構成VM
    /// </summary>
    public class ConfigureViewModel : BindableBase, IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        private readonly IRedfishAdapter _redfishAdapter;
        private readonly Configure _configure;

        // ダークモード
        public ReactivePropertySlim<bool> IsDark { get; set; }

        // 色調整
        public ReactivePropertySlim<bool> IsColorAdjustment { get; set; }

        // プライマリ色
        public ReadOnlyReactiveCollection<string> PrimaryColors { get; set; }
        public ReactivePropertySlim<string> PrimaryColor { get; set; }

        // セカンダリ色
        public ReadOnlyReactiveCollection<string> SecondaryColors { get; set; }
        public ReactivePropertySlim<string> SecondaryColor { get; set; }

        // タイムアウト
        [Required]
        [RegularExpression("(-1|[0-9]+)")]
        [Range(-1, 86400)]
        public ReactivePropertySlim<int> MaxTimeout { get; set; }

        // プロキシ有無
        public ReactivePropertySlim<bool> ProxyEnabled { get; set; }

        // プロキシサーバ＆ポート
        public ReactivePropertySlim<string> ProxyUri { get; set; }

        // プロキシユーザ名
        public ReactivePropertySlim<string> ProxyUsername { get; set; }

        // プロキシパスワード
        public ReactivePropertySlim<string> ProxyPassword { get; set; }

        /// <summary>
        /// ConfigureViewModel
        /// </summary>
        /// <param name="unityContainer"></param>
        public ConfigureViewModel(IUnityContainer unityContainer)
        {
            _logger.Trace($"{this.GetType().Name}.");

            _redfishAdapter = unityContainer.Resolve<IRedfishAdapter>();
            _configure = _redfishAdapter.Configure;

            // ライト or ダーク
            IsDark = new ReactivePropertySlim<bool>(_configure.IsDark)
                .AddTo(_disposables);
            IsDark
                .ObserveProperty(x => x.Value)
                .Subscribe(SetDarkOrLight)
                .AddTo(_disposables);

            // 色調整
            IsColorAdjustment = new ReactivePropertySlim<bool>(_configure.IsColorAdjustment)
                .AddTo(_disposables);
            IsColorAdjustment
                .ObserveProperty(x => x.Value)
                .Subscribe(async x => await SetColoringAsync(x))
                .AddTo(_disposables);

            // プライマリ色
            PrimaryColors = _redfishAdapter.Colors
                .ToReadOnlyReactiveCollection()
                .AddTo(_disposables);
            PrimaryColor = new ReactivePropertySlim<string>(_configure.PrimaryColor)
                .AddTo(_disposables);
            PrimaryColor
                .ObserveProperty(x => x.Value)
                .Subscribe(async _ => await SetColoringAsync(IsColorAdjustment.Value))
                .AddTo(_disposables);

            // セカンダリ色
            SecondaryColors = _redfishAdapter.Colors
                .ToReadOnlyReactiveCollection()
                .AddTo(_disposables);
            SecondaryColor = new ReactivePropertySlim<string>(_configure.SecondaryColor)
                .AddTo(_disposables);
            SecondaryColor
                .ObserveProperty(x => x.Value)
                .Subscribe(async _ => await SetColoringAsync(IsColorAdjustment.Value))
                .AddTo(_disposables);

            // タイムアウト
            MaxTimeout = new ReactivePropertySlim<int>(_configure.MaxTimeout)
                .AddTo(_disposables);
            MaxTimeout
                .ObserveProperty(x => x.Value)
                .Subscribe(SetMaxTimeout)
                .AddTo(_disposables);

            // プロキシ有効・無効
            ProxyEnabled = new ReactivePropertySlim<bool>(_configure.ProxyEnabled)
                .AddTo(_disposables);
            ProxyEnabled
                .ObserveProperty(x => x.Value)
                .Subscribe(x => _configure.ProxyEnabled = x)
                .AddTo(_disposables);

            // プロキシURI
            ProxyUri = new ReactivePropertySlim<string>(_configure.ProxyUri)
                .AddTo(_disposables);
            ProxyUri
                .ObserveProperty(x => x.Value)
                .Subscribe(x => _configure.ProxyUri = x)
                .AddTo(_disposables);

            // プロキシユーザ名
            ProxyUsername = new ReactivePropertySlim<string>(_configure.ProxyUsername)
                .AddTo(_disposables);
            ProxyUsername
                .ObserveProperty(x => x.Value)
                .Subscribe(x => _configure.ProxyUsername = x)
                .AddTo(_disposables);

            // プロキシパスワード
            ProxyPassword = new ReactivePropertySlim<string>(_configure.ProxyPassword)
                .AddTo(_disposables);
            ProxyPassword
                .ObserveProperty(x => x.Value)
                .Subscribe(x => _configure.ProxyPassword = x)
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();

        /// <summary>
        /// ライト or ダーク
        /// </summary>
        /// <param name="isDark"></param>
        private void SetDarkOrLight(bool isDark)
        {
            // ライト・ダークを切り替える
            var mode = isDark ? BaseTheme.Dark : BaseTheme.Light;
            var palette = new PaletteHelper();
            var theme = palette.GetTheme();
            theme.SetBaseTheme(mode);                   // ライト or ダークモード
            palette.SetTheme(theme);

            // ライト・ダークを保持する
            _configure.IsDark = IsDark.Value;
        }

        /// <summary>
        /// 色設定
        /// </summary>
        /// <param name="isColorAdjustment"></param>
        /// <returns></returns>
        private async Task SetColoringAsync(bool isColorAdjustment)
        {
            await Task.Delay(IRedfishAdapter.DelayedDisplay);

            // 指定した色を設定する
            var color1 = _redfishAdapter.Swatches[PrimaryColor.Value].PrimaryHues[5].Color;
            var color2 = _redfishAdapter.Swatches[SecondaryColor.Value].PrimaryHues[5].Color;
            var palette = new PaletteHelper();
            var theme = palette.GetTheme();
            theme.SetPrimaryColor(color1);          // プライマリ色
            theme.SetSecondaryColor(color2);        // セカンダリ色
            theme.ColorAdjustment = !isColorAdjustment ? null :
                new ColorAdjustment
                {
                    DesiredContrastRatio = 4.5f,    // コントラスト比
                    Contrast = Contrast.Medium,     // コントラスト
                    Colors = ColorSelection.All     // 対象範囲
                };
            palette.SetTheme(theme);

            // 色情報を保持する
            _configure.PrimaryColor = PrimaryColor.Value;
            _configure.SecondaryColor = SecondaryColor.Value;
            _configure.IsColorAdjustment = IsColorAdjustment.Value;
        }

        /// <summary>
        /// タイムアウト設定
        /// </summary>
        /// <param name="value"></param>
        private void SetMaxTimeout(int value)
        {
            var timeout = value;
            if (timeout < -1)
            {
                MaxTimeout.Value = timeout = -1;
            }
            _configure.MaxTimeout = timeout;
        }
    }
}
