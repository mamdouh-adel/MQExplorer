using System;
using System.ComponentModel;
using System.Windows;

namespace ActiveMQExplorer.Views
{
    public partial class MainWindowView : Window
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindowView()
        {
            InitializeComponent();

            _log.Info("MQExplorer Start...");
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            ScreensManager.SettingsWindow.Show();
            this.Hide();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ScreensManager.IsClosing = true;
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            ScreensManager.SettingsWindow.Close();
            base.OnClosed(e);

            Application.Current.Shutdown();

            _log.Info("MQExplorer Shutdown...");
            _log.Info("--------------------------------");
        }

        private void AppExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();

            //_log.Info("MQExplorer Shutdown...");
            //_log.Info("--------------------------------");
        }
    }
}
