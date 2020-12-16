using Apache.NMS;
using MQProviders.Common;
using System.Collections.Concurrent;
using System.Threading;

namespace MQProviders.ActiveMQ
{
    public class ActiveMQListener : IMQListener
    {
        private volatile bool _startListen;
        private IMQModel _listenerModel;
        private readonly IConnectionFactory _connectionFactory;

        public ActiveMQListener()
        {
            _listenerModel = new ActiveMQModel();
            _connectionFactory = new NMSConnectionFactory(_listenerModel?.BrokerURI);
            
            ReadMessages = new ConcurrentQueue<string>();
        }

        public void StartListen()
        {
            using (IConnection connection = _connectionFactory?.CreateConnection(_listenerModel?.UserName, _listenerModel?.Password))
            {
                connection?.Start();
                
                using(ISession session = connection?.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    using(IDestination dest = session?.GetQueue(_listenerModel?.Destination))
                    {
                        using(IMessageConsumer consumer = session?.CreateConsumer(dest))
                        {
                            ListenSession(consumer);
                        }
                    }
                }
            }          
        }

        private void ListenSession(IMessageConsumer consumer)
        {
            _startListen = true;
            _listenerModel.Messages = 0;
            while (_startListen)
            {
                IMessage msg = consumer?.Receive();
                ITextMessage txtMsg = msg as ITextMessage;

                ReadMessages.Enqueue(txtMsg.Text);

                ++_listenerModel.Messages;

                Thread.Sleep(1000);
            }
        }

        public void SetListenerModel(IMQModel listenerModel) => _listenerModel = listenerModel;
        public IMQModel GetListenerModel() => _listenerModel;

        public void StopListen()
        {
            _startListen = false;
        }

        public ConcurrentQueue<string> ReadMessages { get; }
    }
}
