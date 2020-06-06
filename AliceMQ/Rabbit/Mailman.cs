using System;
using System.Collections.Generic;
using System.Linq;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailMan.Interface;
using AliceMQ.Rabbit;
using RabbitMQ.Client;

namespace AliceMQ.MailMan
{
    public sealed class Mailman : IMailman
    {
        public string ExchangeName => _exchange.ExchangeName;
        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);

        public readonly ConnectionFactory Factory;
		private readonly IExchange _exchange;
        private readonly Func<object, string> _serializer;

        public Mailman(
            EndPoint simpleEndpoint, 
            IExchange exchange, 
            Func<object,string> serializer)
        {
			_exchange = exchange;
            _serializer = serializer;
            Factory = new ConnectionFactory
            {
                Uri = new Uri(simpleEndpoint.ConnectionUrl),
                NetworkRecoveryInterval = simpleEndpoint.NetworkRecoveryInterval,
                AutomaticRecoveryEnabled = simpleEndpoint.AutomaticRecoveryEnabled
            };
        }

        public IBasicProperties SetupChannel(IModel channel)
        {
            try
            {
                if (!DefaultExchange)
                {
                    channel.ExchangeDeclare(
                                          ExchangeName,
                                          _exchange.ExchangeType,
                                          _exchange.Durable,
                                          _exchange.AutoDelete,
                                          _exchange.Properties);
                }
                  
                return channel.CreateBasicProperties();
            }
            catch (Exception ex)
            {
                throw new SetupException(ex);
            }
        }

        private void RabbitSendMessage(IModel channel, string message,
            IBasicProperties props, string routingKey) 
        {
            channel.BasicPublish(ExchangeName,
                routingKey,
                props,
                props.GetEncoding().GetBytes(message));
        }

        private void TryApplyOnNewChannel(Action<IModel> channelAction)
        {
            try
            {
                using (var connection = Factory.CreateConnection())
                using (var channel = connection.CreateModel())
                    channelAction(channel);
            }
            catch (SetupException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SetupException(ex);
            }
        }

        private Func<IModel, IBasicProperties> UpdateProperties(Action<IBasicProperties> propertiesSetter)
        {
            return c => SetupChannel(c).AssignProperties(propertiesSetter);
        }

        private Action<IModel> SendMessageOnChannel<T>(T message, string routingKey, Action<IBasicProperties> propertiesSetter = null)
        {

            return channel => RabbitSendMessage(
                channel, 
                _serializer(message), 
                UpdateProperties(propertiesSetter)(channel), 
                routingKey);
        }

        public void PublishOne<T>(T message, string routingKey, Action<IBasicProperties> applyStaticProperties)
        {
            TryApplyOnNewChannel(SendMessageOnChannel(message,routingKey, applyStaticProperties));
        }

        public void PublishSome<T>(IEnumerable<T> messages, string routingKey, Action<IBasicProperties> applyStaticProperties)
        {
            TryApplyOnNewChannel(SendManyMessagesOnChannel(messages,routingKey, applyStaticProperties));
        }

        private Action<IModel> SendManyMessagesOnChannel<T>(IEnumerable<T> messages, string routingKey, 
            Action<IBasicProperties> applyStaticProperties)
        {
            return messages
                .Select(m => SendMessageOnChannel(
                    m, 
                    routingKey, 
                    applyStaticProperties))
                .Aggregate((previous, next) => previous + next);
        }

        public void PublishOne<T>(T message, string routingKey)
        {
            PublishOne(message, routingKey, null);
        }

        public void PublishSome<T>(IList<T> messages, string routingKey)
        {
            PublishSome(messages, routingKey, null);
        }
    }
}