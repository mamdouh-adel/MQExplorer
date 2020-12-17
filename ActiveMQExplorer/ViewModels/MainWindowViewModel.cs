using Caliburn.Micro;
using MQProviders.Common;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ActiveMQExplorer.ViewModels
{
    public class MainWindowViewModel : Conductor<Screen>.Collection.AllActive
    {
        private IMQModel _mQPubModule;
        private IMQPublisher _mQPublisher;
        private IMQModel _mQLisenModel;
        private IMQListener _mQListener;

        private volatile bool _stratRetreiveMessages;
        private StringBuilder _messagesBuilder = new StringBuilder();

        private bool _isListenerRun;

        public MainWindowViewModel()
        {

        }

        public MainWindowViewModel(SettingsWindowViewModel settingsWindowViewModel, IMQModel mQModel, IMQPublisher mQPublisher, IMQListener mQListener)
            : this()
        {
            Items.Add(settingsWindowViewModel);

            _mQPubModule = mQModel;
            _mQLisenModel = mQModel;
            _mQPublisher = mQPublisher;
            _mQListener = mQListener;

            Task.Factory.StartNew(() => SetQueueListBox());   
        }

        private void SetQueueListBox()
        {
            Queues = _mQPublisher.GetQueueList().Result;
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
            if(string.IsNullOrWhiteSpace(MQDestination))
            {
                MessageBox.Show("Please set or choose valid Queue", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            _mQPubModule.Destination = MQDestination;
            _mQPubModule.Data = MQData;

            _mQPublisher.SetPublisherModel(_mQPubModule);
            string result = _mQPublisher.StartTransaction();
            if(result == "Success")
            {
                if (Queues.Contains(MQDestination) == false)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind, new ThreadStart(delegate
                    {
                        Queues.Add(MQDestination);
                    }));
                }

                MQData = string.Empty;
            }

            SendStatus = result;
        }

        #region Listener Part

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
                    _messagesBuilder.AppendLine().Append(message);
                    Messages = _messagesBuilder.ToString();
                }

                Thread.Sleep(1000);
            }
        }

        private void HandleListenToMQ(object isListenerChecked)
        {
            Task.Factory.StartNew(() => {
                _isListenerRun = true;
                ListenText = "Try to Start";
            });

            if ((bool)isListenerChecked)
            {
                Task.Factory.StartNew(() => RetrieveMessagesFromMQ());

                _mQLisenModel.Destination = MQDestination;

                _mQListener.SetListenerModel(_mQLisenModel);

                string result = _mQListener.StartListen();
                if(result != "Success")
                {
                    _isListenerRun = false;
                    ListenText = "Fail";
                    MessageBox.Show(result, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }
            else
            {
                _stratRetreiveMessages = false;
                _mQListener.StopListen();

                _isListenerRun = false;
                ListenText = "Stopped";
            }
        }
        #endregion
    }
}
