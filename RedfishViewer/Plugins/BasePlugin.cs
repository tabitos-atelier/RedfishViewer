using NLog;
using Prism.Services.Dialogs;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RedfishViewer.Plugins
{
    /// <summary>
    /// BasePlugin
    /// </summary>
    /// <param name="dialogService"></param>
    public abstract class BasePlugin(IDialogService dialogService)
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected readonly IDialogService _dialogService = dialogService;
        public abstract string PluginName { get; }
        public abstract string Action1Name { get; }
        public abstract string Action2Name { get; }
        public abstract string Action1Hint { get; }
        public abstract string Action2Hint { get; }
        public abstract Task Action1ExecuteAsync(string uri, string? username, string? password);
        public abstract Task Action2ExecuteAsync(string uri, string? username, string? password);

        /// <summary>
        /// ブラウザで URI を開く
        /// </summary>
        /// <param name="arg"></param>
        public static void OpenBrowser(string uri)
        {
            _logger.Info($"Open: {uri}");
            Process.Start(new ProcessStartInfo(uri)
            {
                UseShellExecute = true,
                Verb = "open",
            });
        }
    }
}
