using AutomationTool.Model;
using AutomationTool.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace AutomationTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{e.Exception.Message}{Environment.NewLine}{e.Exception.StackTrace}"), "Error"));
                e.Handled = true;
            }

            void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            {
                if (e.ExceptionObject is Exception ex)
                {
                    App.Bus.Publish<ShowMessage>(new(string.Format($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"), "Error"));
                }
            }

            void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
            {
                App.Bus.Publish<ShowMessage>(new(string.Format($"{e.Exception.Message}{Environment.NewLine}{e.Exception.StackTrace}"), "Error"));
                e.SetObserved();
            }
        }

        public static MessageBus Bus { get; private set; } = null!;


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Bus = new MessageBus(SynchronizationContext.Current);
        }
    }
}
