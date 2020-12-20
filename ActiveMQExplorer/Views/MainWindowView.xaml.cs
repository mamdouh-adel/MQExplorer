using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ActiveMQExplorer.Views
{
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
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
        }

        private void AppExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
