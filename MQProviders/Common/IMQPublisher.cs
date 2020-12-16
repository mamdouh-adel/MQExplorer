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
    }
}