using Apache.NMS;
using MQProviders.Common;

namespace MQProviders.ActiveMQ
{
    public class ActiveMQPublisher : IMQPublisher
    {
        private IMQModel _publisherModel;
        private readonly IConnectionFactory _connectionFactory;

        public ActiveMQPublisher()
        {
            _publisherModel = new ActiveMQModel();
            _publisherModel.BrokerURI = string.Concat("activemq:tcp://", _publisherModel.Host, ":", _publisherModel.Port);
            _connectionFactory = new NMSConnectionFactory(_publisherModel?.BrokerURI);
        }

        public void StartTransaction()
        {
            using(IConnection connection = _connectionFactory?.CreateConnection())
            {
                connection?.Start();

                using(ISession session = connection?.CreateSession(AcknowledgementMode.AutoAcknowledge))
                {
                    IDestination dest = session?.GetQueue(_publisherModel?.Destination);

                    using(IMessageProducer messageProducer = session?.CreateProducer(dest))
                    {
                        messageProducer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                        messageProducer?.Send(session?.CreateTextMessage(_publisherModel.Data));
                    }
                }
            } 
        }

        public void SetPublisherModel(IMQModel publisherModel) => _publisherModel = publisherModel;
        public IMQModel GetPublisherModel() => _publisherModel;
    }
}
