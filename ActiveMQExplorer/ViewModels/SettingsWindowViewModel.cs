using Caliburn.Micro;
using MQProviders.ActiveMQ;
using MQProviders.Common;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace ActiveMQExplorer.ViewModels
{
    public class SettingsWindowViewModel : Conductor<Screen>.Collection.AllActive
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private PublisherMQModel _mQPubModel;
    //    private ListenerMQModel _mQLisenModel;
        private IMQPublisher _mQPublisher;
        private IMQListener _mQListener;

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

        private Brush _connectStatusColor;
        public Brush ConnectStatusColor
        {
            get => _connectStatusColor;
            set
            {
                _connectStatusColor = value;
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

        public SettingsWindowViewModel(IMQPublisher mQPublisher, IMQListener mQListener)
            : this()
        {
            _mQPubModel = MQModelsHandler.CurrentPublisherMQModel;
            _mQPublisher = mQPublisher;
            _mQListener = mQListener;

            Host = Properties.Settings.Default.host;
            Port = Properties.Settings.Default.port;
            UserName = Properties.Settings.Default.user_name;
            Password = Properties.Settings.Default.password;
        }

        public ICommand Connect
        {
            get { return new DelegateCommand(TryConnect); }
        }

        private void TryConnect(object sender)
        {
            PublisherMQModel testPublisherModel = new PublisherMQModel
            {
                UserName = UserName,
                Password = Password,
                Host = Host,
                Port = Port
            };

            _mQPublisher.SetPublisherModel(testPublisherModel);

            ListenerMQModel testListenerMQModel = new ListenerMQModel
            {
                UserName = UserName,
                Password = Password,
                Host = Host,
                Port = Port
            };

            _mQListener.SetListenerModel(testListenerMQModel);
            
            string result = _mQPublisher.TryConnect();
            if (result == "Success")
            {
                _log.Debug("Connection successfully");

                ConnectStatusColor = Brushes.Green;

                MQModelsHandler.CurrentPublisherMQModel = testPublisherModel;
                MQModelsHandler.CurrentListenerMQModel = testListenerMQModel;
                ScreensManager.MainWindowModel.Queues = _mQListener.GetQueueList().Result;

                if(ScreensManager.MainWindowModel.Queues == null || ScreensManager.MainWindowModel.Queues.Any() == false)
                    _log.Error($"Queues List: Null/Empty");
                else
                    _log.Debug($"Queues List: {string.Join(", ", ScreensManager.MainWindowModel.Queues?.ToArray())}");

                Properties.Settings.Default.host = MQModelsHandler.CurrentPublisherMQModel.Host;
                Properties.Settings.Default.port = MQModelsHandler.CurrentPublisherMQModel.Port;
                Properties.Settings.Default.user_name = MQModelsHandler.CurrentPublisherMQModel.UserName;
                Properties.Settings.Default.password = MQModelsHandler.CurrentPublisherMQModel.Password;

                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                _log.Debug("Connection configuration was saved");

                _log.Debug($"Configuration: Host: {Properties.Settings.Default.host}");
                _log.Debug($"Configuration: Port: {Properties.Settings.Default.port}");
                _log.Debug($"Configuration: UserName: {Properties.Settings.Default.user_name}");
                _log.Debug($"Configuration: Password: {Properties.Settings.Default.password}");
            }             
            else
            {
                ConnectStatusColor = Brushes.Red;
                _log.Debug("Connection failed!");

                _log.Error($"Configuration: Host: {Properties.Settings.Default.host}");
                _log.Error($"Configuration: Port: {Properties.Settings.Default.port}");
                _log.Error($"Configuration: UserName: {Properties.Settings.Default.user_name}");
                _log.Error($"Configuration: Password: {Properties.Settings.Default.password}");
            }

            ConnectStatus = result;
        }

        public ICommand ListenerConnect
        {
            get { return new DelegateCommand(TryListenerConnect); }
        }

        private void TryListenerConnect(object sender)
        {
            ListenerMQModel testListenerMQModel = new ListenerMQModel
            {
                UserName = UserName,
                Password = Password,
                Host = Host,
                Port = Port
            };

            _mQListener.SetListenerModel(testListenerMQModel);

            string result = _mQListener.TryConnect();
            if (result == "Success")
            {
                ConnectStatusColor = Brushes.Green;
                MQModelsHandler.CurrentListenerMQModel = testListenerMQModel;
                ScreensManager.MainWindowModel.Queues = _mQListener.GetQueueList().Result;

                Properties.Settings.Default.host = MQModelsHandler.CurrentListenerMQModel.Host;
                Properties.Settings.Default.port = MQModelsHandler.CurrentListenerMQModel.Port;
                Properties.Settings.Default.user_name = MQModelsHandler.CurrentListenerMQModel.UserName;
                Properties.Settings.Default.password = MQModelsHandler.CurrentListenerMQModel.Password;

                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
            }
            else
                ConnectStatusColor = Brushes.Red;

            ConnectStatus = result;
        }

        private bool CheckConnectionParameters()
        {
            //if (string.IsNullOrWhiteSpace(Host))
            //    return false;

            //if (Port <= 0)
            //    return false;

            //if (string.IsNullOrWhiteSpace(UserName))
            //    return false;

            //if (string.IsNullOrWhiteSpace(Password))
            //    return false;

            return true;
        }
    }
}
