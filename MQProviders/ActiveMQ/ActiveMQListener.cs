using Apache.NMS;
using MQProviders.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MQProviders.ActiveMQ
{
    public class ActiveMQListener : IMQListener
    {
        private volatile bool _startListen;
        private IMQModel _listenerModel;
        private IConnectionFactory _connectionFactory;

        public ActiveMQListener()
        {
            _listenerModel = new ActiveMQModel();
      //      _connectionFactory = new NMSConnectionFactory(_listenerModel?.BrokerURI);
            
            ReadMessages = new ConcurrentQueue<string>();
        }

        public string StartListen()
        {
            try
            {
                _connectionFactory = new NMSConnectionFactory(_listenerModel?.BrokerURI);
                using (IConnection connection = _connectionFactory.CreateConnection(_listenerModel.UserName, _listenerModel.Password))
                {
                    connection.Start();

                    using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                    {
                        using (IDestination dest = session.GetQueue(_listenerModel.Destination))
                        {
                            using (IMessageConsumer consumer = session.CreateConsumer(dest))
                            {
                                ListenSession(consumer);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Failed to send: " + ex.Message;
            }

            return "Success";          
        }

        private void ListenSession(IMessageConsumer consumer)
        {
            _startListen = true;
            _listenerModel.Messages = 0;
            while (_startListen)
            {
                IMessage msg = consumer?.Receive();
                ITextMessage txtMsg = msg as ITextMessage;

                ReadMessages.Enqueue(txtMsg.Text);

                ++_listenerModel.Messages;

                Thread.Sleep(100);
            }
        }

        public void SetListenerModel(IMQModel listenerModel) => _listenerModel = listenerModel;
        public IMQModel GetListenerModel() => _listenerModel;

        public void StopListen()
        {
            _startListen = false;
        }

        public ConcurrentQueue<string> ReadMessages { get; }

        public string TryConnect()
        {
            try
            {
                _connectionFactory = new NMSConnectionFactory(_listenerModel?.BrokerURI);
                using (IConnection connection = _connectionFactory.CreateConnection(_listenerModel.UserName, _listenerModel.Password))
                {
                    connection.Start();

                    using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                return "Failed to connect: " + ex.Message;
            }

            return "Success";
        }

        public async Task<ISet<string>> GetQueueList()
        {
            string uri = $"http://{_listenerModel.Host}:8161/api/jolokia/read/org.apache.activemq:type=Broker,brokerName={_listenerModel.Host}";

            HashSet<string> queueList = new HashSet<string>();

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{_listenerModel.UserName}:{_listenerModel.Password}");
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    string result = await httpClient.GetStringAsync(uri);
                    if (string.IsNullOrWhiteSpace(result))
                        return queueList;

                    string[] splitter1 = new string[] { "BrokerId" };
                    string[] splitter2 = new string[] { "{\"objectName\":\"org.apache.activemq:brokerName=" };

                    string[] resultArr1 = result.Split(splitter1, StringSplitOptions.RemoveEmptyEntries);
                    string[] resultArr2 = resultArr1[1].Split(splitter2, StringSplitOptions.RemoveEmptyEntries);

                    if (resultArr2.Length <= 2)
                        return queueList;

                    const string pattern1 = @"^.*destinationName=";
                    const string pattern2 = @",destinationType=.*";

                    for (int i = 1; i < resultArr2.Length; i++)
                    {
                        string queueTempName = Regex.Replace(resultArr2[i], pattern1, string.Empty);
                        string queueName = Regex.Replace(queueTempName, pattern2, string.Empty);
                        queueList.Add(queueName?.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return queueList;
        }
    }
}
