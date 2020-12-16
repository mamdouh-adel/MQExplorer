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
