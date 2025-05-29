using NLog;
using Prism.Dialogs;
using RedfishViewer.Services;
using System;
using System.Threading.Tasks;

namespace RedfishViewer.Plugins
{
    public class DefaultPlugin : BasePlugin
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public override string PluginName => "Default";             // 標準
        public override string Action1Name => "ブラウザ";           // ブラウザ起動
        public override string Action2Name => string.Empty;         // なし
        public override string Action1Hint => "Uriをブラウザで開く";
        public override string Action2Hint => string.Empty;

        /// <summary>
        /// DefaultPlugin
        /// </summary>
        /// <param name="dialogService"></param>
        public DefaultPlugin(IDialogService dialogService) : base(dialogService)
        {
            _logger.Trace($"{this.GetType().Name}.");
        }

        /// <summary>
        /// ブラウザ起動
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override async Task Action1ExecuteAsync(string uri, string? username, string? password)
        {
            if (string.IsNullOrEmpty(uri))
                return;

            try
            {
                _logger.Info($"Open in browser: {uri}");
                await Task.Run(() => OpenBrowser(uri));
            }
            catch (Exception ex)
            {
                var errmsg = $"ブラウザの起動に失敗しました。\n\n{ex.Message}";
                _logger.Error(ex, errmsg);
                _dialogService.AlertMessage(errmsg);
            }
        }

        /// <summary>
        /// なし
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override async Task Action2ExecuteAsync(string uri, string? username, string? password)
            => await Task.Run(() => { });
    }
}
