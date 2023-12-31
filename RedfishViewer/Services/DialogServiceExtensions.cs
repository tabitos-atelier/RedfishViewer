using Prism.Services.Dialogs;
using System.Windows;

namespace RedfishViewer.Services
{
    /// <summary>
    /// IDialogServiceの拡張メソッド
    /// </summary>
    public static class DialogServiceExtensions
    {
        /// <summary>
        /// エラーメッセージ表示
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="message"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ButtonResult AlertMessage(this IDialogService dialogService, string message, string icon = "Alert")
        {
            var parameters = new DialogParameters
            {
                {"Icon", icon },
                { "Message", message },
            };
            var result = ButtonResult.Cancel;
            dialogService.ShowDialog("MessageBox", parameters, x => result = x.Result);
            return result;
        }

        public static ButtonResult YesNoMessage(this IDialogService dialogService, string message, string icon = "ToggleSwitch")
        {
            var parameters = new DialogParameters
            {
                {"Icon", icon },
                { "Message", message },
                { "Buttons", MessageBoxButton.YesNo }
            };
            var result = ButtonResult.Cancel;
            dialogService.ShowDialog("MessageBox", parameters, x => result = x.Result);
            return result;
        }
    }
}
