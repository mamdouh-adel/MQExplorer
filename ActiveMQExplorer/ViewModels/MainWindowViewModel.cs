using ActiveMQExplorer.Common;
using ActiveMQExplorer.Views;
using Caliburn.Micro;
using MQProviders.ActiveMQ;
using MQProviders.Common;
using System.Collections.Generic;
using System.IO;
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
      //  private string _currentPublishQueue;
        private string _currentListenQueue;
        private string _currentSendFromDirQueue;

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
            IsReadyToSendFromDir = true;
            IsSendFromVisible = Visibility.Hidden;

            // set Publisher Mode, add to setting GUI later
            //_mQPublisher.PublisherMode = PublisherMode.ObjectMode;

            MessagesDataList = new List<MessageData>();

            MQModelsHandler.CurrentPublisherMQModel.Host = Properties.Settings.Default.host;
            MQModelsHandler.CurrentPublisherMQModel.Port = Properties.Settings.Default.port;
            MQModelsHandler.CurrentPublisherMQModel.UserName = Properties.Settings.Default.user_name;
            MQModelsHandler.CurrentPublisherMQModel.Password = Properties.Settings.Default.password;
            MQModelsHandler.CurrentPublisherMQModel.PublisherMode = (PublisherMode)Properties.Settings.Default.publisher_mode;

            MQModelsHandler.CurrentListenerMQModel.Host = Properties.Settings.Default.host;
            MQModelsHandler.CurrentListenerMQModel.Port = Properties.Settings.Default.port;
            MQModelsHandler.CurrentListenerMQModel.UserName = Properties.Settings.Default.user_name;
            MQModelsHandler.CurrentListenerMQModel.Password = Properties.Settings.Default.password;

            MQFilesHandler.IsInDumpFilesMode = Properties.Settings.Default.is_in_dump_mode;
            MQFilesHandler.DumpDirectory = Properties.Settings.Default.dump_dir;

            SourceDirectory = Properties.Settings.Default.send_by_dir;
            MQFilesHandler.SourceDirectory = SourceDirectory;

            _mQPublisher.SetPublisherModel(MQModelsHandler.CurrentPublisherMQModel);
            _mQListener.SetListenerModel(MQModelsHandler.CurrentListenerMQModel);

            _log.Debug($"Current Publisher Mode: {_mQPublisher.GetPublisherModel().PublisherMode}");
            
            _log.Debug($"Current Host: {_mQPublisher.GetPublisherModel().Host}");
            _log.Debug($"Current Port: {_mQPublisher.GetPublisherModel().Port}");
            _log.Debug($"Current UserName: {_mQPublisher.GetPublisherModel().UserName}");
            _log.Debug($"Current Password: {_mQPublisher.GetPublisherModel().Password}");
            _log.Debug($"Current Is In Dump Files Mode: {MQFilesHandler.IsInDumpFilesMode}");
            _log.Debug($"Current Dump Directory: {MQFilesHandler.DumpDirectory}");
            _log.Debug($"Current Source Directory: {MQFilesHandler.SourceDirectory}");

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
        

        private string _queueTextPublisher;
        public string QueueTextPublisher
        {
            get => _queueTextPublisher;
            set
            {
                _queueTextPublisher = value;
                if (string.IsNullOrWhiteSpace(_queueTextPublisher))
                    SendStatus = "Please set or choose valid Queue";
                else   
                    SendStatus = string.Empty;

                MQDestination = _queueTextPublisher;
                NotifyOfPropertyChange();
            }
        }

        private string _queueTextListener;
        public string QueueTextListener
        {
            get => _queueTextListener;
            set
            {
                _queueTextListener = value;
                if (string.IsNullOrWhiteSpace(_queueTextListener))
                    SendStatus = "Please set or choose valid Queue";
                else
                    SendStatus = string.Empty;
                if (_currentListenQueue != QueueTextListener)
                {
                    MessageData.ResetId();
                    MessagesDataList = new List<MessageData>();
                }

                _currentListenQueue = QueueTextListener;
                NotifyOfPropertyChange();
            }
        }

        private string _queueTextSendFromDir;
        public string QueueTextSendFromDir
        {
            get => _queueTextSendFromDir;
            set
            {
                _queueTextSendFromDir = value;
                if (string.IsNullOrWhiteSpace(_queueTextSendFromDir))
                    SendStatus = "Please set or choose valid Queue";
                else
                {
                    _currentSendFromDirQueue = QueueTextSendFromDir;
                    IsReadyToSendBySourceDirectory = string.IsNullOrWhiteSpace(_currentSendFromDirQueue) == false && IsSourceDirectoryExist;
                    SendStatus = string.Empty;
                }
                NotifyOfPropertyChange();
            }
        }

        public ICommand Send
        {
            get { return new DelegateCommand(SendMessageToMQ); }
        }

        private void SendMessageToMQ(object sender)
        {
            if (string.IsNullOrWhiteSpace(MQDestination))
            {
                MessageBox.Show("Please set or choose valid Queue", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            SendStatus = string.Empty;

            _log.Debug($"Start send message to {MQDestination}");

            MQModelsHandler.CurrentPublisherMQModel.Destination = MQDestination;
            MQModelsHandler.CurrentPublisherMQModel.Data = MQData;

            _mQPublisher.SetPublisherModel(MQModelsHandler.CurrentPublisherMQModel);

            _log.Debug($"Publisher Mode: {MQModelsHandler.CurrentPublisherMQModel.PublisherMode}");
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
                Thread.Sleep(3000);
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
                bool haveMessages = _mQListener.ReadMessages.TryDequeue(out ActiveMQMessageProxy message);
                if (haveMessages)
                {
                    _log.Debug($"Messages found, Id: {message.MessageId}\n,Data: {message.Content}");

                    if(string.IsNullOrEmpty(message.Content))
                        _log.Error("Null incoming message!");

                    Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind, new ThreadStart(delegate
                    {
                        List<MessageData> newMessageDataList = new List<MessageData>(MessagesDataList)
                        {
                            new MessageData { Data = message.Content }
                        };

                        MessagesDataList = newMessageDataList;
                    }));

                    if (MQFilesHandler.IsInDumpFilesMode)
                    {
                        string result = MQFilesHandler.SaveDumpFile(message.MessageId, message.Content);
                        _log.Debug(result);
                    }
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

        public ICommand OnQueueSelectedChangedPublisher
        {
            get { return new DelegateCommand(OnQueueSelectedChangedPublisherAction); }
        }

        private void OnQueueSelectedChangedPublisherAction(object sender)
        {
           // _currentPublishQueue = QueueTextPublisher;
        }

        public ICommand OnQueueSelectedChangedListener
        {
            get { return new DelegateCommand(OnQueueSelectedChangedListenerAction); }
        }

        private void OnQueueSelectedChangedListenerAction(object sender)
        {
            //if (_currentListenQueue != QueueTextListener)
            //{
            //    MessageData.ResetId();
            //    MessagesDataList = new List<MessageData>();
            //}

            //_currentListenQueue = QueueTextListener;
        }

        public ICommand OnQueueSelectedChangedSendFromDir
        {
            get { return new DelegateCommand(OnQueueSelectedChangedSendFromDirAction); }
        }

        private void OnQueueSelectedChangedSendFromDirAction(object sender)
        {
            //_currentSendFromDirQueue = QueueTextSendFromDir;

            //IsReadyToSendBySourceDirectory = string.IsNullOrWhiteSpace(_currentSendFromDirQueue) == false && IsSourceDirectoryExist;
        }

        private void HandleListenToMQ(object isListenerChecked)
        {
            if ((bool)isListenerChecked)
            {
                if(string.IsNullOrWhiteSpace(QueueTextListener))
                {
                    MessageBox.Show("Please Select Queue First", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }

                _log.Debug($"Trying listen to {QueueTextListener}");

                Task.Factory.StartNew(() => {
                    _isListenerRun = true;
                    IsListenerAvailable = !_isListenerRun;
                    ListenText = "Try to Start";
                });

                Task.Factory.StartNew(() => RetrieveMessagesFromMQ());

                MQModelsHandler.CurrentListenerMQModel.Destination = QueueTextListener;

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

        #region Send from Directory

        private string _sourceDirectory;
        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                _sourceDirectory = value;

                if (string.IsNullOrWhiteSpace(_sourceDirectory))
                    IsSourceDirectoryExist = false;
                else
                    IsSourceDirectoryExist = true;

                NotifyOfPropertyChange();
            }
        }

        private bool _isSourceDirectoryExist;
        public bool IsSourceDirectoryExist
        {
            get => _isSourceDirectoryExist;
            set
            {
                _isSourceDirectoryExist = value;
                NotifyOfPropertyChange();
            }
        }

        private string _sendByDirStatus;
        public string SendByDirStatus
        {
            get => _sendByDirStatus;
            set
            {
                _sendByDirStatus = value;
                NotifyOfPropertyChange();
            }
        }

        private Brush _sendByDirStatusColor;
        public Brush SendByDirStatusColor
        {
            get => _sendByDirStatusColor;
            set
            {
                _sendByDirStatusColor = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _isSendFromVisible;
        public Visibility IsSendFromVisible
        {
            get => _isSendFromVisible;
            set
            {
                _isSendFromVisible = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isReadyToSendBySourceDirectory;
        public bool IsReadyToSendBySourceDirectory
        {
            get => _isReadyToSendBySourceDirectory;
            set
            {
                _isReadyToSendBySourceDirectory = value;
                NotifyOfPropertyChange();
            }
        }
        
        private int _sendFromDirCurrentValue;
        public int SendFromDirCurrentValue
        {
            get => _sendFromDirCurrentValue;
            set
            {
                _sendFromDirCurrentValue = value;
                NotifyOfPropertyChange();
            }
        }

        private int _sendFromDirMaxValue;
        public int SendFromDirMaxValue
        {
            get => _sendFromDirMaxValue;
            set
            {
                _sendFromDirMaxValue = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isReadyToSendFromDir;
        public bool IsReadyToSendFromDir
        {
            get => _isReadyToSendFromDir;
            set
            {
                _isReadyToSendFromDir = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand ChooseSourceDirectory
        {
            get { return new DelegateCommand(ChooseSourceDirectoryAction); }
        }

        private void ChooseSourceDirectoryAction(object sender)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.InitialDirectory = SourceDirectory;
            dialog.Title = "Select Source Directory";
            dialog.Filter = "Directory|*.this.directory";
            dialog.FileName = "select";
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                SourceDirectory = path;

                Properties.Settings.Default.send_by_dir = SourceDirectory;

                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                _log.Debug($"Configuration: Publisher Source Directory: {Properties.Settings.Default.send_by_dir}");

                IsReadyToSendBySourceDirectory = string.IsNullOrWhiteSpace(QueueTextSendFromDir) == false && IsSourceDirectoryExist;
            }
        }

        public ICommand SendByDirectory
        {
            get { return new DelegateCommand(SendByDirectoryAction); }
        }

        private void SendByDirectoryAction(object sender)
        {
            IsReadyToSendBySourceDirectory = false;
            IsReadyToSendFromDir = false;
            IsSendFromVisible = Visibility.Visible;
            SendByDirStatusColor = Brushes.DarkOrange;
            SendByDirStatus = string.Empty;

            if (IsSourceDirectoryExist == false)
            {
                _log.Error("Source directory not found!");
                return;
            }

            var result = ReadAndSendFilesFromDirectory();
            if (result.isSuccess)
            {
                _log.Debug(result.log);
                SendByDirStatusColor = Brushes.Green;
            }           
            else
            {
                _log.Error(result.log);
                SendByDirStatusColor = Brushes.Red;
            }

            SendByDirStatus = result.log;
            IsSendFromVisible = Visibility.Hidden;
            IsReadyToSendBySourceDirectory = true;
            IsReadyToSendFromDir = true;
        }

        private (bool isSuccess, string log, string[] filesContent) ReadAndSendFilesFromDirectory()
        {
            DirectoryInfo srcDirInfo = new DirectoryInfo(SourceDirectory);
            FileInfo[] filesInfo = srcDirInfo.GetFiles("*.txt");
            if (filesInfo == null || filesInfo.Length <= 0)
                return (isSuccess: false, log: $"There is no .txt files in: {SourceDirectory}", filesContent: null);

            SendFromDirMaxValue = filesInfo.Length;
            string[] filesContent = new string[filesInfo.Length];

            for (int i = 0; i < SendFromDirMaxValue; i++)
            {
                var result = MQFilesHandler.ReadFile(filesInfo[i]);
                _log.Debug(result.log);
                SendByDirStatus = result.log;

                if (result.isSuccess)
                {
                    MQModelsHandler.CurrentPublisherMQModel.Destination = _currentSendFromDirQueue;
                    MQModelsHandler.CurrentPublisherMQModel.Data = result.fileContent;
                    _mQPublisher.SetPublisherModel(MQModelsHandler.CurrentPublisherMQModel);

                    string sendResult = _mQPublisher.StartTransaction();
                    SendByDirStatus = sendResult;
                }

                SendFromDirCurrentValue = i;
            }

            return (isSuccess: true, log: "Success", filesContent);
        }

        #endregion
    }
}
