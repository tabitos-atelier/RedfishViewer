using NLog;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedfishViewer.Models;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// HTTPリクエスト:ヘッダ＆パラメータ単体 VM
    /// </summary>
    public class ParameterViewModel : IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        public ReactiveProperty<bool> Enabled {  get; }             // 有効・無効チェックボックス
        public ReactiveProperty<string> Name { get; }               // パラメータ名
        public ReactiveProperty<string> Value { get; }              // パラメータ値

        private readonly ReactiveProperty<bool>? _hasErrors = null;

        public ReactiveProperty<bool> HasErrors =>
            _hasErrors ?? Observable.CombineLatest(
                Enabled.ObserveHasErrors,
                Name.ObserveHasErrors,
                Value.ObserveHasErrors)
            .Select(x => x.Any(y => y))
            .ToReactiveProperty();

        private Subject<Unit> CommitTrigger { get; } = new();
        private IObservable<Unit> CommitAsObservable =>
            CommitTrigger
            .Where(_ => !HasErrors.Value);

        /// <summary>
        /// ParameterViewModel
        /// </summary>
        /// <param name="model"></param>
        public ParameterViewModel(Parameter model)
        {
            _logger.Trace($"{this.GetType().Name}.");

            // Enabled
            Enabled = model.ObserveProperty(x => x.Enabled)
                .ToReactiveProperty()
                .SetValidateAttribute(() => Enabled)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Enabled.Value)
                .Subscribe(x => model.Enabled = x)
                .AddTo(_disposables);

            // Name
            Name = model.ObserveProperty(x => x.Name)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Name)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Name.Value)
                .Subscribe(x => model.Name = x)
                .AddTo(_disposables);

            // Value
            Value = model.ObserveProperty(x => x.Value)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Value)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Value.Value)
                .Subscribe(x => model.Value = x)
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
