using NLog;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedfishViewer.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// HTTPレスポンス結果 VM
    /// </summary>
    public class ResultViewModel : IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];
        private readonly Uri _uri;

        [Key]
        public ReactiveProperty<string> Uri { get; }            // Uri
        public ReactivePropertySlim<string> Host { get; }       // ホスト名
        public ReactivePropertySlim<string> Path { get; }       // Uri.Path
        [Required]
        public ReactiveProperty<string> Method { get; }         // HTTPリクエスト:メソッド
        public ReactiveProperty<string?> InHeaders { get; }     // HTTPリクエスト:ヘッダ
        public ReactiveProperty<string?> Parameters { get; }    // HTTPリクエスト:パラメータ
        public ReactiveProperty<string?> JsonBody{ get; }       // HTTPリクエスト:Jsonボディ
        [Required]
        public ReactiveProperty<int> StatusCode { get; }        // HTTPレスポンス:ステータスコード
        [Required]
        public ReactiveProperty<string> OutHeaders { get; }     // HTTPレスポンス:ヘッダ
        public ReactiveProperty<bool> IsJsonText { get; }       // HTTPレスポンス:コンテンツはJsonか？
        public ReactiveProperty<DateTime> Updated { get; }      // 更新日時
        public ReactiveProperty<string> Content { get; }        // HTTPレスポンス:コンテンツ
        public ReactiveProperty<DateTime?> LastUpdated { get; } // 前回の更新日時
        public ReactiveProperty<string?> LastContent { get; }   // 前回のコンテンツ


        private readonly ReactiveProperty<bool>? _hasErrors = null;
        public ReactiveProperty<bool> HasErrors =>
            _hasErrors ?? Observable.CombineLatest(
                Uri.ObserveHasErrors,
                Method.ObserveHasErrors,
                InHeaders.ObserveHasErrors,
                Parameters.ObserveHasErrors,
                JsonBody.ObserveHasErrors,
                StatusCode.ObserveHasErrors,
                OutHeaders.ObserveHasErrors,
                IsJsonText.ObserveHasErrors,
                Updated.ObserveHasErrors,
                Content.ObserveHasErrors,
                LastUpdated.ObserveHasErrors,
                LastContent.ObserveHasErrors)
            .Select(x => x.Any(y => y))
            .ToReactiveProperty();
        private Subject<Unit> CommitTrigger { get; } = new();
        private IObservable<Unit> CommitAsObservable =>
            CommitTrigger
            .Where(_ => !HasErrors.Value);

        /// <summary>
        /// ResultViewModel
        /// </summary>
        /// <param name="model"></param>
        public ResultViewModel(Result model)
        {
            _logger.Trace($"{this.GetType().Name}.");

            // Request URI
            Uri = model.ObserveProperty(x => x.Uri)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Uri)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Uri.Value)
                .Subscribe(x => model.Uri = x)
                .AddTo(_disposables);
            _uri = new Uri(model.Uri);
            Host = new ReactivePropertySlim<string>(_uri.Host);
            Path = new ReactivePropertySlim<string>(_uri.LocalPath + _uri.Fragment);

            // Request Method
            Method = model.ObserveProperty(x => x.Method)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Method)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Method.Value)
                .Subscribe(x => model.Method = x)
                .AddTo(_disposables);

            // Request Headers
            InHeaders = model.ObserveProperty(x => x.InHeaders)
                .ToReactiveProperty()
                .SetValidateAttribute(() => InHeaders)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => InHeaders.Value)
                .Subscribe(x => model.InHeaders = x)
                .AddTo(_disposables);

            // Request Parameters
            Parameters = model.ObserveProperty(x => x.Parameters)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Parameters)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Parameters.Value)
                .Subscribe(x => model.Parameters = x)
                .AddTo(_disposables);

            // Json Body
            JsonBody = model.ObserveProperty(x => x.JsonBody)
                .ToReactiveProperty()
                .SetValidateAttribute(() => JsonBody)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => JsonBody.Value)
                .Subscribe(x => model.JsonBody = x)
                .AddTo(_disposables);

            // HTTP Response Status Code
            StatusCode = model.ObserveProperty(x => x.StatusCode)
                .ToReactiveProperty()
                .SetValidateAttribute(() => StatusCode)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => StatusCode.Value)
                .Subscribe(x => model.StatusCode = x)
                .AddTo(_disposables);

            // HTTP Response Headers
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

            // Response Updated
            Updated = model.ObserveProperty(x => x.Updated)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Updated)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Updated.Value)
                .Subscribe(x => model.Updated = x)
                .AddTo(_disposables);

            // Response Content
            Content = model.ObserveProperty(x => x.Content)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Content)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Content.Value)
                .Subscribe(x => model.Content = x)
                .AddTo(_disposables);

            // Response Last Updated
            LastUpdated = model.ObserveProperty(x => x.LastUpdated)
                .ToReactiveProperty()
                .SetValidateAttribute(() => LastUpdated)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => LastUpdated.Value)
                .Subscribe(x => model.LastUpdated = x)
                .AddTo(_disposables);

            // Response Last Content
            LastContent = model.ObserveProperty(x => x.LastContent)
                .ToReactiveProperty()
                .SetValidateAttribute(() => LastContent)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => LastContent.Value)
                .Subscribe(x => model.LastContent = x)
                .AddTo(_disposables);
        }

        public void Commit()
        {
            _logger.Trace("Commit.");
            // https://blog.okazuki.jp/entry/2015/11/21/235455
            CommitTrigger.OnNext(Unit.Default);
        }

        public void Destroy()
            => _disposables.Dispose();
    }
}
