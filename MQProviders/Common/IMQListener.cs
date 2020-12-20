using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MQProviders.Common
{
    public interface IMQListener
    {
        string StartListen();
        void StopListen();
        void SetListenerModel(IMQModel ListenerModel);
        IMQModel GetListenerModel();
        ConcurrentQueue<string> ReadMessages { get; }
        string TryConnect();
        Task<ISet<string>> GetQueueList();
    }
}
