using System.ComponentModel;
using System.Windows;

namespace ActiveMQExplorer.Views
{
    public partial class SettingsWindowView : Window
    {
        public SettingsWindowView()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;

            if (ScreensManager.IsClosing == false)
                ScreensManager.MainWindow.Show();
        }
    }
}
