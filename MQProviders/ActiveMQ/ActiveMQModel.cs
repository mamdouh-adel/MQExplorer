using MQProviders.Common;

namespace MQProviders.ActiveMQ
{
    public class ActiveMQModel : IMQModel
    {
        public string UserName { get; set; } /* = "admin"; */
        public string Password { get; set; } /*= "admin"; */
        public string Host { get; set; } /*= "localhost"; */
        public int Port { get; set; } = 61616;
        public string Destination { get; set; }
        public string Data { get; set; }
        public string BrokerURI { get => string.Concat("activemq:tcp://", Host, ":", Port, "?transport.useLogging=true"); }
        public long Messages { get; set; } = 10000;
        public int Size { get; set; } = 256;
    }

    public class PublisherMQModel : ActiveMQModel
    {

    }

    public class ListenerMQModel : ActiveMQModel
    {

    }
}
