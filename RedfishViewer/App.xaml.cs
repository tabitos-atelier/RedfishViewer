using Prism.Ioc;
using System.Windows;

namespace RedfishViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<Views.MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Views.Reqests, ViewModels.ReqestsViewModel>();
            containerRegistry.RegisterForNavigation<Views.Responses, ViewModels.ResponsesViewModel>();
            containerRegistry.RegisterForNavigation<Views.Nodes, ViewModels.NodesViewModel>();
            containerRegistry.RegisterForNavigation<Views.HttpErrors, ViewModels.HttpErrorsViewModel>();
            containerRegistry.RegisterForNavigation<Views.Configure, ViewModels.ConfigureViewModel>();
            containerRegistry.RegisterForNavigation<Views.Tools, ViewModels.ToolsViewModel>();
            containerRegistry.RegisterDialog<Views.MessageBox, ViewModels.MessageBoxViewModel>();
            containerRegistry.RegisterSingleton<Services.IRedfishAdapter, Services.RedfishRestSharp>();
            containerRegistry.RegisterSingleton<Services.IDatabaseAgent, Services.DatabaseContext>();
        }
    }
}
