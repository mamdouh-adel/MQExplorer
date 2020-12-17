
namespace ActiveMQExplorer.ViewModels
{
    public class MessageData
    {
        public static long Id { get; set; } = 1;
        public string Data { get; set; }

        public static long NewId()
        {
            return ++Id;
        }
    }
}
