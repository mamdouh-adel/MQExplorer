using Caliburn.Micro;
using MQProviders.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ActiveMQExplorer.ViewModels
{
    public class MainWindowViewModel : Conductor<Screen>.Collection.AllActive
    {
        private IMQModel _mQPubModule;
        private IMQPublisher _mQPublisher;
        private IMQModel _mQLisenModel;
        private IMQListener _mQListener;

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

        private string _queueValue;
        public string QueueValue
        {
            get => _queueValue;
            set
            {
                _queueValue = value;
                if (string.IsNullOrWhiteSpace(_queueValue))
                    SendStatus = "Please set or choose valid Queue";
                else
                {
                    MQDestination = _queueValue;
                    SendStatus = string.Empty;
                }
            }
        }

        public ICommand Send
        {
            get { return new DelegateCommand(SendMessageToMQ); }
        }

        private void SendMessageToMQ()
        {
            _mQPubModule.Destination = MQDestination;
            _mQPubModule.Data = MQData;

            _mQPublisher.SetPublisherModel(_mQPubModule);
            string result = _mQPublisher.StartTransaction();
            if(result == "Success")
            {
                if (Queues.Contains(MQDestination) == false)
                    Queues.Add(MQDestination);
            }

            SendStatus = result;
        }
    }
}
