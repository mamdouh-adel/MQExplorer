using System;

namespace MQProviders.Common
{
    public interface IMQModel
    {
        string UserName { get; set; }
        string Password { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        string Destination { get; set; }
        string Data { get; set; }
        string BrokerURI { get; } 
        long Messages { get; set; }
        int Size { get; set; }
        PublisherMode PublisherMode { get; set; }
    }
}
