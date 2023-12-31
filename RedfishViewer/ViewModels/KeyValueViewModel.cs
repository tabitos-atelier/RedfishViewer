using NLog;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using RedfishViewer.Models;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RedfishViewer.ViewModels
{
    /// <summary>
    /// Key-Value VM
    /// </summary>
    public class KeyValueViewModel : IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        public ReactiveProperty<string> Key { get; }
        public ReactiveProperty<string> Value { get; }

        private readonly ReactiveProperty<bool>? _hasErrors = null;
        public ReactiveProperty<bool> HasErrors =>
            _hasErrors ?? Observable.CombineLatest(
                Key.ObserveHasErrors,
                Value.ObserveHasErrors)
            .Select(x => x.Any(y => y))
            .ToReactiveProperty();

        private Subject<Unit> CommitTrigger { get; } = new();
        private IObservable<Unit> CommitAsObservable =>
            CommitTrigger
            .Where(_ => !HasErrors.Value);

        /// <summary>
        /// KeyValueViewModel
        /// </summary>
        /// <param name="model"></param>
        public KeyValueViewModel(KeyValue model)
        {
            _logger.Trace($"{this.GetType().Name}.");

            // Key
            Key = model.ObserveProperty(x => x.Key)
                .ToReactiveProperty<string>()
                .SetValidateAttribute(() => Key)
                .AddTo(_disposables);
            CommitAsObservable
                .Select(_ => Key.Value)
                .Subscribe(x => model.Key = x)
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
