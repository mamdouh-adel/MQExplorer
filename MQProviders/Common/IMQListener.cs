﻿using System.Collections.Concurrent;

namespace MQProviders.Common
{
    public interface IMQListener
    {
        string StartListen();
        void StopListen();
        void SetListenerModel(IMQModel ListenerModel);
        IMQModel GetListenerModel();
        ConcurrentQueue<string> ReadMessages { get; }
    }
}
