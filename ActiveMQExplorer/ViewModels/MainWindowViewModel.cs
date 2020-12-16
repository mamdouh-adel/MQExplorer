using ActiveMQExplorer.Views;
using Autofac;
using Caliburn.Micro;
using MQProviders.Contracts;
using System.Windows;
using System.Windows.Input;

namespace ActiveMQExplorer.ViewModels
{
    public class MainWindowViewModel : Conductor<Screen>.Collection.AllActive
    {
        private readonly IHelloWorld _helloWorldService;
        private string _infoText;

        //public MainWindowViewModel(IHelloWorld helloWorldService)
        //{
        //    _helloWorldService = helloWorldService;
        //    InfoText = _helloWorldService.GetInfo();
        //}

        public MainWindowViewModel()
        {

        }

        public MainWindowViewModel(IWindowManager helloWorldService, SettingsWindowViewModel settingsWindowViewModel)
            : this()
        {
            Items.Add(settingsWindowViewModel);

            InfoText = "OK";
        }

        public string InfoText
        {
            get => _infoText;
            set
            {
                _infoText = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand ConvertTextCommand
        {
            get { return new DelegateCommand(Test); }
        }

        private void Test()
        {
        }
    }
}
