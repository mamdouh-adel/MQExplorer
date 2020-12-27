using ActiveMQExplorer.Views;
using Caliburn.Micro;
using MQProviders.ActiveMQ;
using MQProviders.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ActiveMQExplorer.ViewModels
{
    public class MainWindowViewModel : Conductor<Screen>.Collection.AllActive
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMQPublisher _mQPublisher;
        private readonly IMQListener _mQListener;

        private volatile bool _stratRetreiveMessages;

        private bool _isListenerRun;
        private string _currentQueue;

        public MainWindowViewModel()
        {
            Queues = new HashSet<string>();
        }

        public MainWindowViewModel(SettingsWindowViewModel settingsWindowViewModel, IMQPublisher mQPublisher, IMQListener mQListener)
            : this()
        {
            Items.Add(settingsWindowViewModel);

            ScreensManager.MainWindowModel = this;
            ScreensManager.SettingsWindowModel = settingsWindowViewModel;

            _mQPublisher = mQPublisher;
            _mQListener = mQListener;
            IsListenerAvailable = true;

            // set Publisher Mode, add to setting GUI later
            _mQPublisher.PublisherMode = PublisherMode.ObjectMode;

            MessagesDataList = new List<MessageData>();

            MQModelsHandler.CurrentPublisherMQModel.Host = Properties.Settings.Default.host;
            MQModelsHandler.CurrentPublisherMQModel.Port = Properties.Settings.Default.port;
            MQModelsHandler.CurrentPublisherMQModel.UserName = Properties.Settings.Default.user_name;
            MQModelsHandler.CurrentPublisherMQModel.Password = Properties.Settings.Default.password;

            MQModelsHandler.CurrentListenerMQModel.Host = Properties.Settings.Default.host;
            MQModelsHandler.CurrentListenerMQModel.Port = Properties.Settings.Default.port;
            MQModelsHandler.CurrentListenerMQModel.UserName = Properties.Settings.Default.user_name;
            MQModelsHandler.CurrentListenerMQModel.Password = Properties.Settings.Default.password;

            _mQPublisher.SetPublisherModel(MQModelsHandler.CurrentPublisherMQModel);
            _mQListener.SetListenerModel(MQModelsHandler.CurrentListenerMQModel);

            _log.Debug($"Current Host: {_mQPublisher.GetPublisherModel().Host}");
            _log.Debug($"Current Port: {_mQPublisher.GetPublisherModel().Port}");
            _log.Debug($"Current UserName: {_mQPublisher.GetPublisherModel().UserName}");
            _log.Debug($"Current Password: {_mQPublisher.GetPublisherModel().Password}");

            Task.Factory.StartNew(() => SetQueueListBox());
        }

        public void SetQueueListBox()
        {
            Queues = _mQPublisher.GetQueueList().Result;

            if (Queues == null || Queues.Any() == false)
                _log.Error($"Queues List: Null/Empty");
            else
                _log.Debug($"Queues List: {string.Join(", ", Queues?.ToArray())}");
        }

        private ISet<string> _queues;
        public ISet<string> Queues
        {
            get => _queues;
            set
            {
                _queues = value;
                NotifyOfPropertyChange();
            }
        }

        private string _mQData;
        public string MQData
        {
            get => _mQData;
            set
            {
                _mQData = value;
                SendStatus = string.Empty;
                NotifyOfPropertyChange();
            }
        }

        private bool _isReadyToSend = true;
        public bool IsReadyToSend
        {
            get => _isReadyToSend;
            set
            {
                _isReadyToSend = value;
                NotifyOfPropertyChange();
            }
        }

        private string _mQDestination;
        public string MQDestination
        {
            get => _mQDestination;
            set
            {
                _mQDestination = value;
                NotifyOfPropertyChange();
            }
        }

        private string _sendStatus;
        public string SendStatus
        {
            get => _sendStatus;
            set
            {
                _sendStatus = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isListenerAvailable;
        public bool IsListenerAvailable
        {
            get => _isListenerAvailable;
            set
            {
                _isListenerAvailable = value;
                NotifyOfPropertyChange();
            }
        }
        

        private string _queueText;
        public string QueueText
        {
            get => _queueText;
            set
            {
                _queueText = value;
                if (string.IsNullOrWhiteSpace(_queueText))
                    SendStatus = "Please set or choose valid Queue";
                else   
                    SendStatus = string.Empty;

                MQDestination = _queueText;
                NotifyOfPropertyChange();
            }
        }

        public ICommand Send
        {
            get { return new DelegateCommand(SendMessageToMQ); }
        }

        private void SendMessageToMQ(object sender)
        {
            IsReadyToSend = false;

            _log.Debug($"Start send message to {MQDestination}");

            if(string.IsNullOrWhiteSpace(MQDestination))
            {
                MessageBox.Show("Please set or choose valid Queue", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                IsReadyToSend = true;
                return;
            }

            MQModelsHandler.CurrentPublisherMQModel.Destination = MQDestination;
            MQModelsHandler.CurrentPublisherMQModel.Data = MQData;

            _mQPublisher.SetPublisherModel(MQModelsHandler.CurrentPublisherMQModel);

            _log.Debug($"Publisher Info: Host: {MQModelsHandler.CurrentPublisherMQModel.Host}");
            _log.Debug($"Publisher Info: Port: {MQModelsHandler.CurrentPublisherMQModel.Port}");
            _log.Debug($"Publisher Info: Destination: {MQModelsHandler.CurrentPublisherMQModel.Destination}");
            _log.Debug($"Publisher Info: Data: {MQModelsHandler.CurrentPublisherMQModel.Data}");
            _log.Debug($"Publisher Info: UserName: {MQModelsHandler.CurrentPublisherMQModel.UserName}");
            _log.Debug($"Publisher Info: Password: {MQModelsHandler.CurrentPublisherMQModel.Password}");

            IsReadyToSend = false;
            SendStatusColor = Brushes.DarkOrange;
            SendStatus = "Please wait...";

            string result = _mQPublisher.StartTransaction();
            if (result == "Success")
            {
                _log.Debug($"Sent Data: {MQModelsHandler.CurrentPublisherMQModel.Data} successfully to {MQModelsHandler.CurrentPublisherMQModel.Destination}");

                if (Queues.Contains(MQDestination) == false)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind, new ThreadStart(delegate
                    {
                        Queues.Add(MQDestination);
                        _log.Debug($"Adding {MQDestination} to Queues");
                    }));
                }

                MQData = string.Empty;
                SendStatusColor = Brushes.Green;
            }
            else
            {
                SendStatusColor = Brushes.Red;
                _log.Error($"Send Data: {MQModelsHandler.CurrentPublisherMQModel.Data} to {MQModelsHandler.CurrentPublisherMQModel.Destination} failed, reason: {result}");
            }

            IsReadyToSend = true;
            SendStatus = result;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                SendStatus = string.Empty;
            });
        }

        #region Listener Part
        private Brush _sendStatusColor;
        public Brush SendStatusColor
        {
            get => _sendStatusColor;
            set
            {
                _sendStatusColor = value;
                NotifyOfPropertyChange();
            }
        }

        private string _messages;
        public string Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                NotifyOfPropertyChange();
            }
        }

        private List<MessageData> _messagesDataList;
        public List<MessageData> MessagesDataList
        {
            get => _messagesDataList;
            set
            {
                _messagesDataList = value;
                NotifyOfPropertyChange();
            }
        }

        private string _listenText = "Start Listening";
        public string ListenText
        {
            get => _listenText;
            set
            {
                if (_isListenerRun)
                    _listenText = "Stop Listening";
                else
                    _listenText = "Start Listening";

                NotifyOfPropertyChange();
            }
        }


        private MessageData _selectedMessageData;
        public MessageData SelectedMessageData
        {
            get => _selectedMessageData;
            set
            {
                _selectedMessageData = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand ListenHandler
        {
            get { return new DelegateCommand(HandleListenToMQ); }
        }

        private void RetrieveMessagesFromMQ()
        {
            _stratRetreiveMessages = true;
            while (_stratRetreiveMessages)
            {
                bool haveMessages = _mQListener.ReadMessages.TryDequeue(out string message);
                if (haveMessages)
                {
                    _log.Debug($"Messages found, Data: {message}");

                    if(string.IsNullOrEmpty(message))
                        _log.Error("Null incoming message!");

                    Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind, new ThreadStart(delegate
                    {
                        List<MessageData> newMessageDataList = new List<MessageData>(MessagesDataList)
                        {
                            new MessageData { Data = message }
                        };

                        MessagesDataList = newMessageDataList;
                    }));
                }

                Thread.Sleep(100);
            }
        }

        public ICommand MessageDataRowClicked
        {
            get { return new DelegateCommand(MessageDataRowClickedAction); }
        }

        private void MessageDataRowClickedAction(object sender)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind, new ThreadStart(delegate
            {
                MessagePresenter.Show(SelectedMessageData.Data);
            }));
        }

        public ICommand OnQueueSelectedChanged
        {
            get { return new DelegateCommand(OnQueueSelectedChangedAction); }
        }

        private void OnQueueSelectedChangedAction(object sender)
        {
            if (_currentQueue != QueueText)
                MessagesDataList = new List<MessageData>();

            _currentQueue = QueueText;
        }

        private void HandleListenToMQ(object isListenerChecked)
        {
            if ((bool)isListenerChecked)
            {
                _log.Debug($"Trying listen to {MQDestination}");

                Task.Factory.StartNew(() => {
                    _isListenerRun = true;
                    IsListenerAvailable = !_isListenerRun;
                    ListenText = "Try to Start";
                });

                Task.Factory.StartNew(() => RetrieveMessagesFromMQ());

                MQModelsHandler.CurrentListenerMQModel.Destination = MQDestination;

                _mQListener.SetListenerModel(MQModelsHandler.CurrentListenerMQModel);

                string result = _mQListener.StartListen();
                if(result != "Success")
                {
                    _log.Error($"Failed to start listening, reason: {result}");

                    _log.Error($"Listener Info: Host: {MQModelsHandler.CurrentListenerMQModel.Host}");
                    _log.Error($"Listener Info: Port: {MQModelsHandler.CurrentListenerMQModel.Port}");
                    _log.Error($"Listener Info: Destination: {MQModelsHandler.CurrentListenerMQModel.Destination}");
                    _log.Error($"Listener Info: Data: {MQModelsHandler.CurrentListenerMQModel.Data}");
                    _log.Error($"Listener Info: UserName: {MQModelsHandler.CurrentListenerMQModel.UserName}");
                    _log.Error($"Listener Info: Password: {MQModelsHandler.CurrentListenerMQModel.Password}");

                    _isListenerRun = false;
                    IsListenerAvailable = !_isListenerRun;
                    ListenText = "Fail";
                    MessageBox.Show(result, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }
            else
            {
                _log.Debug("Stop listening");

                _stratRetreiveMessages = false;
                _isListenerRun = false;
                IsListenerAvailable = !_isListenerRun;
                _mQListener.StopListen();

                _isListenerRun = false;
                ListenText = "Stopped";
            }
        }
        #endregion
    }
}
