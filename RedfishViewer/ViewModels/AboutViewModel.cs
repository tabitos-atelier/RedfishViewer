// Copyright (c) 2023-2026 Tabito's Works
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using NLog;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using System.Reflection;

namespace RedfishViewer.ViewModels
{
    public class AboutViewModel : BindableBase, IDestructible
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CompositeDisposable _disposables = [];

        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        public string AppVersion { get; } =
            $"Version {_assembly.GetName().Version?.ToString(3) ?? string.Empty}";

        public string AppCopyright { get; } =
            _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;

        public ReactiveCommand OpenGitHubCommand { get; }
        public ReactiveCommand OpenBlogCommand { get; }
        public ReactiveCommand OpenXCommand { get; }

        public AboutViewModel()
        {
            _logger.Trace($"{this.GetType().Name}.");

            OpenGitHubCommand = new ReactiveCommand()
                .WithSubscribe(() => OpenUrl("https://github.com/tabitos-atelier/redfish-viewer"))
                .AddTo(_disposables);
            OpenBlogCommand = new ReactiveCommand()
                .WithSubscribe(() => OpenUrl("https://tabitos-voyage.com/"))
                .AddTo(_disposables);
            OpenXCommand = new ReactiveCommand()
                .WithSubscribe(() => OpenUrl("https://x.com/TabitosPharos"))
                .AddTo(_disposables);
        }

        public void Destroy()
            => _disposables.Dispose();

        private static void OpenUrl(string url)
            => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
