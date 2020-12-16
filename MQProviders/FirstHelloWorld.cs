using MQProviders.Contracts;

namespace MQProviders
{
    public class FirstHelloWorld : IHelloWorld
    {
        public string GetInfo()
        {
            return "Hello world!!!";
        }
    }
}
