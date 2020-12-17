using System.Windows;
using System.Windows.Media.Imaging;


namespace ActiveMQExplorer.Views
{
    public partial class MessagePresenter : Window
    {
        public MessagePresenter()
        {
            InitializeComponent();
        }

        public static bool? Show(string message, BitmapImage image = null)
        {
            MessagePresenter msgBox = new MessagePresenter();
            msgBox.Message.Text = message;
            msgBox.Image.Source = image;

            return msgBox.ShowDialog();
        }
    }
}
