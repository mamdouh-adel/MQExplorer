using Caliburn.Micro;
using MQProviders.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ActiveMQExplorer.ViewModels
{
    public class SettingsWindowViewModel : Conductor<Screen>.Collection.AllActive
    {
        private IMQModel _mQPubModule;
        private IMQPublisher _mQPublisher;
        private IMQModel _mQLisenModel;
        private IMQListener _mQListener;

        private volatile bool _stratRetreiveMessages;

        private StringBuilder _messagesBuilder = new StringBuilder();

        private string _testText = "Test Text...";
        public string TestText
        {
            get => _testText;
            set
            {
                _testText = value;
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

        public SettingsWindowViewModel()
        {
            var view = ViewLocator.LocateForModel(this, null, null);

            ViewModelBinder.Bind(this, view, null);

            IActivate activator = this;
            if (activator != null)
                activator.Activate();
        }

        public SettingsWindowViewModel(IMQModel mQModel, IMQPublisher mQPublisher, IMQListener mQListener)
            : this()
        {
            _mQPubModule = mQModel;
            _mQLisenModel = mQModel;
            _mQPublisher = mQPublisher;
            _mQListener = mQListener;
        }

        public ICommand Send
        {
            get { return new DelegateCommand(SendMessageToMQ); }
        }

        private void SendMessageToMQ(object sender)
        {
            _mQPubModule.Destination = MQDestination;
            _mQPubModule.Data = MQData;

            _mQPublisher.SetPublisherModel(_mQPubModule);
            _mQPublisher.StartTransaction();
        }

        public ICommand StartListen
        {
            get { return new DelegateCommand(StartListenToMQ); }
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

        private void StartListenToMQ(object sender)
        {
            Task.Factory.StartNew(() => RetrieveMessagesFromMQ());

            _mQLisenModel.Destination = MQDestination;

            _mQListener.SetListenerModel(_mQLisenModel);
            _mQListener.StartListen();
        }

        public ICommand StopListen
        {
            get { return new DelegateCommand(StopListenToMQ); }
        }

        private void StopListenToMQ(object sender)
        {
            _stratRetreiveMessages = false;
            _mQListener.StopListen();
        }

        public ICommand Test
        {
            get { return new DelegateCommand(TestAction); }
        }
        private void TestAction(object sender)
        {
            _mQPublisher.TryConnect();
        }
    }
}
