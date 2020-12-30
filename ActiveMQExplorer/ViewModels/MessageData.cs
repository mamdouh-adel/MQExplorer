
namespace ActiveMQExplorer.ViewModels
{
    public class MessageData
    {
        private static long _CurrentId { get; set; }

        public long Id { get; private set; }

        public MessageData()
        {
            Id = ++_CurrentId;
        }

        public string Data { get; set; }

        public static void ResetId()
        {
            _CurrentId = 0;
        }

        public string BriefData
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Data))
                    return string.Empty;

                int briefMax;
                if (Data.Length >= 20)
                    briefMax = 20;
                else
                    briefMax = Data.Length;

                string dots = briefMax >= 20 ? "..." : string.Empty;

                return string.IsNullOrWhiteSpace(Data) ? string.Empty : Data.Substring(0, briefMax) + dots;
            }
        }
    }
}
