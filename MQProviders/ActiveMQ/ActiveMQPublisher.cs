using Apache.NMS;
using MQProviders.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MQProviders.ActiveMQ
{
    public class ActiveMQPublisher : IMQPublisher
    {
        private IMQModel _publisherModel;
        private IConnectionFactory _connectionFactory;

        public ActiveMQPublisher()
        {
            _publisherModel = new PublisherMQModel();
        }

        public string StartTransaction()
        {
            try
            {
                _connectionFactory = new NMSConnectionFactory(_publisherModel?.BrokerURI);
                if (_connectionFactory == null)
                    return "Null ConnectionFactory!";

                using (IConnection connection = _connectionFactory.CreateConnection(_publisherModel.UserName, _publisherModel.Password))
                {
                    if (connection == null)
                        return "Null Connection!";

                    connection.Start();

                    using (ISession session = connection?.CreateSession(AcknowledgementMode.AutoAcknowledge))
                    {
                        if (session == null)
                            return "Null Session!";

                        using (IDestination dest = session.GetQueue(_publisherModel.Destination))
                        {
                            if (dest == null)
                                return "Null Queue!";
                            using (IMessageProducer messageProducer = session.CreateProducer(dest))
                            {
                                if (messageProducer == null)
                                    return "Null Message Producer!";

                                messageProducer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                                messageProducer.Send(session.CreateTextMessage(_publisherModel.Data));
                            }
                        }     
                    }
                }
            }
            catch(Exception ex)
            {
                return "Failed to send: " + ex.Message;
            }

            return "Success";
        }

        public void SetPublisherModel(IMQModel publisherModel) => _publisherModel = publisherModel;
        public IMQModel GetPublisherModel() => _publisherModel;

        public async Task<ISet<string>> GetQueueList()
        {
            string uri = $"http://{_publisherModel.Host}:8161/api/jolokia/read/org.apache.activemq:type=Broker,brokerName={_publisherModel.Host}";
            
            HashSet<string> queueList = new HashSet<string>();

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{_publisherModel.UserName}:{_publisherModel.Password}");
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

                    for (int i=1; i<resultArr2.Length; i++)
                    {
                        string queueTempName = Regex.Replace(resultArr2[i], pattern1, string.Empty);
                        string queueName = Regex.Replace(queueTempName, pattern2, string.Empty);
                        queueList.Add(queueName?.Trim());
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return queueList;
        }

        public string TryConnect()
        {
            try 
            {
                _connectionFactory = new NMSConnectionFactory(_publisherModel?.BrokerURI);

                using (IConnection connection = _connectionFactory.CreateConnection(_publisherModel.UserName, _publisherModel.Password))
                {
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
    }
}
