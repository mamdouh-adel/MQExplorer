
namespace MQProviders.ActiveMQ
{
    public class ActiveMQMessageProxy
    {
        public string MessageId { get; set; }
        public string Content { get; set; }

        public ActiveMQMessageProxy()
        {

        }

        public ActiveMQMessageProxy(string id, string content)
        {
            MessageId = id;
            Content = content;
        }
    }
}
