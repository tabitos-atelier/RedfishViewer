using NLog;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedfishViewer.Models;
using RedfishViewer.Services;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// ノード単体 VM
    /// </summary>
    public class NodeViewModel : IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        public ReactiveProperty<string> RootUri { get; }                // ルートURI
        public ReactiveProperty<string?> Username { get; }              // ユーザ名
        public ReactiveProperty<string?> Password { get; }              // パスワード(暗号化)
        public ReactiveProperty<string?> DecryptPassword { get; }       // 複合化パスワード(表示用)
        public ReactiveProperty<DateTime> Created {  get; }             // 作成日
        public ReactiveProperty<DateTime> Updated { get; }              // 更新日
        public ReactiveProperty<string> Plugin { get; }                 // プラグイン
        public ReactiveProperty<string?> Title { get; }                 // タイトル(編集可能)
        public ReactiveProperty<string?> Summary { get; }               // 概要(編集可能)
        public ReactiveProperty<string?> Note { get; }                  // 備考(編集可能)
        
        private readonly ReactiveProperty<bool>? _hasErrors = null;
        public ReactiveProperty<bool> HasErrors =>
            _hasErrors ?? Observable.CombineLatest(
                RootUri.ObserveHasErrors,
                Username.ObserveHasErrors,
                Password.ObserveHasErrors,
                Created.ObserveHasErrors,
                Updated.ObserveHasErrors,
                Plugin.ObserveHasErrors,
                Title.ObserveHasErrors,
                Summary.ObserveHasErrors,
                Note.ObserveHasErrors)
            .Select(x => x.Any(y => y))
            .ToReactiveProperty();
        private Subject<Unit> CommitTrigger { get; } = new();
        private IObservable<Unit> CommitAsObservable =>
            CommitTrigger
            .Where(_ => !HasErrors.Value);

        /// <summary>
        /// NodeViewModel
        /// </summary>
        /// <param name="model"></param>
        public NodeViewModel(Node model)
        {
            _logger.Trace($"{this.GetType().Name}.");

            // Host
            RootUri = model.ObserveProperty(x => x.RootUri)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => RootUri)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => RootUri.Value)
                .Subscribe(x => RootUri.Value = x)
                .AddTo(_disposables);

            // Username
            Username = model.ObserveProperty(x => x.Username)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Username)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Username.Value)
                .Subscribe(x => Username.Value = x)
                .AddTo(_disposables);

            // Password
            Password = model.ObserveProperty(x => x.Password)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Password)
                .AddTo(_disposables);
            // 複合化パスワード
            DecryptPassword = new ReactiveProperty<string?>(CryptoAes.Decrypt(model.Password))
                .AddTo(_disposables);
            // パスワードが変更されたら、複合化パスワードも変更する
            Password
                .ObserveProperty(x => x.Value)
                .Subscribe(x => DecryptPassword.Value = CryptoAes.Decrypt(x))
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Password.Value)
                .Subscribe(x => Password.Value = x)
                .AddTo(_disposables);

            // 作成日
            Created = model.ObserveProperty(x => x.Created)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Created)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Created.Value)
                .Subscribe(x => Created.Value = x)
                .AddTo(_disposables);

            // 更新日
            Updated = model.ObserveProperty(x => x.Updated)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Updated)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Updated.Value)
                .Subscribe(x => Updated.Value = x)
                .AddTo(_disposables);

            // Plugin
            Plugin = model.ObserveProperty(x => x.Plugin)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Plugin)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Plugin.Value)
                .Subscribe(x => Plugin.Value = x)
                .AddTo(_disposables);

            // Title
            Title = model.ObserveProperty(x => x.Title)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Title)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Title.Value)
                .Subscribe(x => Title.Value = x)
                .AddTo(_disposables);

            // Component
            Summary= model.ObserveProperty(x => x.Summary)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Summary)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Summary.Value)
                .Subscribe(x => Summary.Value = x)
                .AddTo(_disposables);

            // Note
            Note = model.ObserveProperty(x => x.Note)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Note)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Note.Value)
                .Subscribe(x => Note.Value = x)
                .AddTo(_disposables);
        }

        public void Commit()
        {
            // https://blog.okazuki.jp/entry/2015/11/21/235455
            CommitTrigger.OnNext(Unit.Default);
        }

        public void Destroy()
            => _disposables.Dispose();
    }
}
