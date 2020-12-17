using Caliburn.Micro;
using MQProviders.Common;
using System.Windows.Input;


namespace ActiveMQExplorer.ViewModels
{
    public class SettingsWindowViewModel : Conductor<Screen>.Collection.AllActive
    {
        private IMQModel _mQPubModel;
        private IMQPublisher _mQPublisher;
        private IMQModel _mQLisenModel;

        private bool _isReadyToTryConnect;
        public bool IsReadyToTryConnect
        {
            get => _isReadyToTryConnect;
            set
            {
                _isReadyToTryConnect = value;
                NotifyOfPropertyChange();
            }
        }

        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                IsReadyToTryConnect = CheckConnectionParameters();
                NotifyOfPropertyChange();
            }
        }

        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                _port = value;
                IsReadyToTryConnect = CheckConnectionParameters();
                NotifyOfPropertyChange();
            }
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                IsReadyToTryConnect = CheckConnectionParameters();
                NotifyOfPropertyChange();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                IsReadyToTryConnect = CheckConnectionParameters();
                NotifyOfPropertyChange();
            }
        }

        private string _connectStatus;
        public string ConnectStatus
        {
            get => _connectStatus;
            set
            {
                _connectStatus = value;
                NotifyOfPropertyChange();
            }
        }

        public SettingsWindowViewModel()
        {
            var view = ViewLocator.LocateForModel(this, null, null);

            ViewModelBinder.Bind(this, view, null);

            IActivate activator = this;
            if (activator != null)
                activator.Activate();
        }

        public SettingsWindowViewModel(IMQModel mQModel, IMQPublisher mQPublisher)
            : this()
        {
            _mQPubModel = mQModel;
            _mQLisenModel = mQModel;
            _mQPublisher = mQPublisher;
        }

        public ICommand Connect
        {
            get { return new DelegateCommand(TryConnect); }
        }

        private void TryConnect(object sender)
        {
            _mQLisenModel.Host = Host;
            _mQLisenModel.Port = Port;
            _mQLisenModel.UserName = UserName;
            _mQLisenModel.Password = Password;
            _mQLisenModel.Destination = "MTS";

            _mQPubModel.Host = Host;
            _mQPubModel.Port = Port;
            _mQPubModel.UserName = UserName;
            _mQPubModel.Password = Password;
            _mQPubModel.Destination = "MTS";

            _mQPublisher.SetPublisherModel(_mQPubModel);       
            string result = _mQPublisher.TryConnect();

            ConnectStatus = result;
            //if(result != "Success")
            //{

            //}
        }

        private bool CheckConnectionParameters()
        {
            if (string.IsNullOrWhiteSpace(Host))
                return false;

            if (Port <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(UserName))
                return false;

            if (string.IsNullOrWhiteSpace(Password))
                return false;

            return true;
        }
    }
}
