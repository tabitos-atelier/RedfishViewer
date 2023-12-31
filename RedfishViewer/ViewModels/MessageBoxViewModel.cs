using NLog;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using System;
using System.Windows;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// MessageBox VM
    /// </summary>
    public class MessageBoxViewModel : BindableBase, IDestructible, IDialogAware
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        public string Title => "RedfishViwerメッセージ";
        public event Action<IDialogResult>? RequestClose;                       // 閉じるとき呼び出す

        public ReactivePropertySlim<string> IconKind { get; set; }              // アイコン名称
        public ReactivePropertySlim<string> Message { get; set; }               // メッセージ本文
        public ReactivePropertySlim<Visibility> OkVisibility { get; set; }      // OK
        public ReactivePropertySlim<Visibility> YesVisibility { get; set; }     // はい
        public ReactivePropertySlim<Visibility> NoVisibility { get; set; }      // いいえ
        public ReactivePropertySlim<Visibility> CancelVisibility { get; set; }  // キャンセル

        public ReactiveCommandSlim OkCommand { get; }                           // OK
        public ReactiveCommandSlim YesCommand { get; }                          // はい
        public ReactiveCommandSlim NoCommand { get; }                           // いいえ
        public ReactiveCommandSlim CancelCommand { get; }                       // キャンセル

        /// <summary>
        /// MessageBoxViewModel
        /// </summary>
        public MessageBoxViewModel()
        {
            _logger.Trace($"{this.GetType().Name}.");

            // アイコン名称
            IconKind = new ReactivePropertySlim<string>("Alert")
                .AddTo(_disposables);

            // メッセージ本文
            Message = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(_disposables);

            // OKボタン表示・非常時
            OkVisibility = new ReactivePropertySlim<Visibility>(Visibility.Visible)
                .AddTo(_disposables);

            // はいボタン表示・非常時
            YesVisibility = new ReactivePropertySlim<Visibility>(Visibility.Collapsed)
                .AddTo(_disposables);

            // いいえボタン表示・非常時
            NoVisibility = new ReactivePropertySlim<Visibility>(Visibility.Collapsed)
                .AddTo(_disposables);

            // キャンセルボタン表示・非常時
            CancelVisibility = new ReactivePropertySlim<Visibility>(Visibility.Collapsed)
                .AddTo(_disposables);

            // OKボタン
            OkCommand = new ReactiveCommandSlim()
                .WithSubscribe(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)))
                .AddTo(_disposables);

            // はいボタン
            YesCommand = new ReactiveCommandSlim()
                .WithSubscribe(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Yes)))
                .AddTo(_disposables);

            // いいえボタン
            NoCommand = new ReactiveCommandSlim()
                .WithSubscribe(() => RequestClose?.Invoke(new DialogResult(ButtonResult.No)))
                .AddTo(_disposables);

            // キャンセルボタン
            CancelCommand = new ReactiveCommandSlim()
                .WithSubscribe(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)))
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();

        /// <summary>
        /// ダイアログがClose可能かを取得します。
        /// </summary>
        /// <returns></returns>
        public bool CanCloseDialog()
            => true;

        /// <summary>
        /// ダイアログClose時のイベントハンドラ。
        /// </summary>
        public void OnDialogClosed()
        {
        }

        /// <summary>
        /// ダイアログOpen時のイベントハンドラ。
        /// </summary>
        /// <param name="parameters"></param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            IconKind.Value = parameters.GetValue<string>("Icon");
            Message.Value = parameters.GetValue<string>("Message");
            var buttons = parameters.GetValue<MessageBoxButton>("Buttons");
            switch (buttons)
            {
                case MessageBoxButton.OKCancel:
                    CancelVisibility.Value = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    OkVisibility.Value = Visibility.Collapsed;
                    YesVisibility.Value = Visibility.Visible;
                    NoVisibility.Value = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    OkVisibility.Value = Visibility.Collapsed;
                    YesVisibility.Value = Visibility.Visible;
                    NoVisibility.Value = Visibility.Visible;
                    CancelVisibility.Value = Visibility.Visible;
                    break;
            }
        }
    }
}
