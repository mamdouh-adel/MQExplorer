using System.Collections.Generic;
using System.Threading.Tasks;

namespace MQProviders.Common
{
    public interface IMQPublisher
    {
        string StartTransaction();
        void SetPublisherModel(IMQModel publisherModel);
        IMQModel GetPublisherModel();
        Task<ISet<string>> GetQueueList();
        string TryConnect();
        PublisherMode PublisherMode { get; set; }
    }


    public enum PublisherMode
    {
        ObjectMode = 1,
        TextMode = 2
    }
}