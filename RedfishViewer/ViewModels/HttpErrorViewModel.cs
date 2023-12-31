using NLog;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using RedfishViewer.Models;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using Reactive.Bindings.Extensions;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// HTTPエラー単体VM
    /// </summary>
    public class HttpErrorViewModel : IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        public ReactiveProperty<DateTime> Created { get; }          // エラー発生日時
        public ReactiveProperty<string> Message { get; }            // エラーメッセージ
        public ReactiveProperty<bool> ProxyEnabled { get; }         // プロキシ有無
        public ReactiveProperty<string> Method { get; }             // HTTPリクエスト:メソッド
        public ReactiveProperty<string> Uri { get; }                // HTTPリクエスト:URI
        public ReactiveProperty<string?> InHeaders { get; }         // HTTPリクエスト:ヘッダ
        public ReactiveProperty<string?> Parameters { get; }        // HTTPリクエスト:パラメータ
        public ReactiveProperty<string?> JsonBody { get; }          // HTTPリクエスト:Jsonボディ
        public ReactiveProperty<int> StatusCode { get; }            // HTTPレスポンス:ステータスコード
        public ReactiveProperty<string> OutHeaders { get; }         // HTTPレスポンス:ヘッダ
        public ReactiveProperty<bool> IsJsonText { get; }           // コンテンツはJsonか？
        public ReactiveProperty<string> Content { get; }            // HTTPレスポンス:コンテンツ

        private readonly ReactiveProperty<bool>? _hasErrors = null;
        public ReactiveProperty<bool> HasErrors =>
            _hasErrors ?? Observable.CombineLatest(
                Created.ObserveHasErrors,
                Message.ObserveHasErrors,
                ProxyEnabled.ObserveHasErrors,
                Method.ObserveHasErrors,
                Uri.ObserveHasErrors,
                InHeaders.ObserveHasErrors,
                Parameters.ObserveHasErrors,
                JsonBody.ObserveHasErrors,
                StatusCode.ObserveHasErrors,
                OutHeaders.ObserveHasErrors,
                IsJsonText.ObserveHasErrors,
                Content.ObserveHasErrors)
            .Select(x => x.Any(y => y))
            .ToReactiveProperty();

        private Subject<Unit> CommitTrigger { get; } = new();
        private IObservable<Unit> CommitAsObservable =>
            CommitTrigger
            .Where(_ => !HasErrors.Value);

        /// <summary>
        /// HttpErrorViewModel
        /// </summary>
        /// <param name="model"></param>
        public HttpErrorViewModel(HttpError model)
        {
            _logger.Trace($"{this.GetType().Name}.");

            // 発生日時
            Created = model.ObserveProperty(x => x.Created)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Created)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Created.Value)
                .Subscribe(x => model.Created = x)
                .AddTo(_disposables);

            // Error Message
            Message = model.ObserveProperty(x => x.Message)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Message)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Message.Value)
                .Subscribe(x => model.Message = x)
                .AddTo(_disposables);

            // プロキシの有無
            ProxyEnabled = model.ObserveProperty(x => x.ProxyEnabled)
                .ToReactiveProperty()
                .SetValidateAttribute(() => ProxyEnabled)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => ProxyEnabled.Value)
                .Subscribe(x => model.ProxyEnabled = x)
                .AddTo(_disposables);

            // Method
            Method = model.ObserveProperty(x => x.Method)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Method)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Method.Value)
                .Subscribe(x => model.Method = x)
                .AddTo(_disposables);

            // URI
            Uri = model.ObserveProperty(x => x.Uri)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Uri)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Uri.Value)
                .Subscribe(x => model.Uri = x)
                .AddTo(_disposables);

            // InHeaders
            InHeaders = model.ObserveProperty(x => x.InHeaders)
                .ToReactiveProperty<string?>()
                .SetValidateAttribute(() => InHeaders)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => InHeaders.Value)
                .Subscribe(x => model.InHeaders = x)
                .AddTo(_disposables);

            // Parameters
            Parameters = model.ObserveProperty(x => x.Parameters)
                .ToReactiveProperty<string?>()
                .SetValidateAttribute(() => Parameters)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Parameters.Value)
                .Subscribe(x => model.Parameters = x)
                .AddTo(_disposables);

            // JsonBody
            JsonBody = model.ObserveProperty(x => x.JsonBody)
                .ToReactiveProperty<string?>()
                .SetValidateAttribute(() => JsonBody)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => JsonBody.Value)
                .Subscribe(x => model.JsonBody = x)
                .AddTo(_disposables);

            // StatusCode
            StatusCode = model.ObserveProperty(x => x.StatusCode)
                .ToReactiveProperty()
                .SetValidateAttribute(() => StatusCode)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => StatusCode.Value)
                .Subscribe(x => model.StatusCode = x)
                .AddTo(_disposables);

            // HTTPレスポンス:ヘッダー
            OutHeaders = model.ObserveProperty(x => x.OutHeaders)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => OutHeaders)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => OutHeaders.Value)
                .Subscribe(x => model.OutHeaders = x)
                .AddTo(_disposables);

            // Is json text
            IsJsonText = model.ObserveProperty(x => x.IsJsonText)
                .ToReactiveProperty()
                .SetValidateAttribute(() => IsJsonText)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => IsJsonText.Value)
                .Subscribe(x => model.IsJsonText = x)
                .AddTo(_disposables);

            // HTTPレスポンス:コンテンツ
            Content = model.ObserveProperty(x => x.Content)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Content)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Content.Value)
                .Subscribe(x => model.Content = x)
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();
    }
}
