using System.Windows;

namespace ActiveMQExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Bootstrapper _bootstrapper;
        public App()
        {
            _bootstrapper = new Bootstrapper();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _bootstrapper.Initialize();
            base.OnStartup(e);
        }
    }
}
