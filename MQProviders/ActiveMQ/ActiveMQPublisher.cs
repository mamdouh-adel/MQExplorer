﻿using Apache.NMS;
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
        private readonly IConnectionFactory _connectionFactory;

        public ActiveMQPublisher()
        {
            _publisherModel = new ActiveMQModel();
            _connectionFactory = new NMSConnectionFactory(_publisherModel?.BrokerURI);
        }

        public string StartTransaction()
        {
            try
            {
                using (IConnection connection = _connectionFactory.CreateConnection(_publisherModel.UserName, _publisherModel.Password))
                {
                    connection.Start();

                    using (ISession session = connection?.CreateSession(AcknowledgementMode.AutoAcknowledge))
                    {
                        IDestination dest = session.GetQueue(_publisherModel.Destination);

                        using (IMessageProducer messageProducer = session.CreateProducer(dest))
                        {
                            messageProducer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                            messageProducer.Send(session?.CreateTextMessage(_publisherModel.Data));
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

                    for (int i=2; i<resultArr2.Length; i++)
                    {
                        string queueTempName = Regex.Replace(resultArr2[i], pattern1, string.Empty);
                        string queueName = Regex.Replace(queueTempName, pattern2, string.Empty);
                        queueList.Add(queueName?.Trim());
                    }
                }
            }
            catch
            {

            }

            return queueList;
        }

        public string TryConnect()
        {
            try 
            {
                using (IConnection connection = _connectionFactory.CreateConnection(_publisherModel.UserName, _publisherModel.Password))
                {
                  //  var test = connection.

                    using (ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge))
                    {
                        IDestination dest = session.GetQueue(_publisherModel.Destination);

                        IMessageProducer messageProducer = session.CreateProducer(dest);
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
