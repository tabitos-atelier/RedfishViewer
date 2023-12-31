using NLog;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using System.Web;

namespace RedfishViewer.ViewModels
{
    public class ToolsViewModel : BindableBase, IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        // URLデコード＆エンコード
        public ReactivePropertySlim<string> RequestUri { get; set; }    // HTTPリクエストURI
        public ReactiveCommand UriDecodeCommand { get; }                // デコード
        public ReactiveCommand UriEncodeCommand { get; }                // エンコード

        // URI変換後
        public ReactivePropertySlim<string> ResultUri { get; set; }     // 変換結果

        public ToolsViewModel()
        {
            _logger.Trace($"{this.GetType().Name}.");

            // URI変換前
            RequestUri = new ReactivePropertySlim<string>()
                .AddTo(_disposables);
            UriDecodeCommand = new ReactiveCommand()
                .WithSubscribe(() => UriDecode(RequestUri.Value))
                .AddTo(_disposables);
            UriEncodeCommand = new ReactiveCommand()
                .WithSubscribe(() => UriEncode(RequestUri.Value))
                .AddTo(_disposables);

            // URI変換後
            ResultUri = new ReactivePropertySlim<string>()
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();

        private void UriDecode(string text)
            => ResultUri.Value = HttpUtility.UrlDecode(text);

        private void UriEncode(string text)
            => ResultUri.Value = HttpUtility.UrlEncode(text);
    }
}
