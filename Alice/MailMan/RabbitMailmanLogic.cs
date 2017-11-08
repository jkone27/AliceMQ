using System;
using System.Collections.Generic;
using System.Linq;
using Alice.ExtensionMethods;
using Alice.MailBox.EndPointArgs;
using Alice.MailMan.Interface;
using RabbitMQ.Client;

namespace Alice.MailMan
{
    public sealed class RabbitMailmanLogic
    {
        public string ExchangeName => _exchange.ExchangeName;
        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);

        public readonly ConnectionFactory Factory;
		private readonly IExchange _exchange;
        private readonly Func<object, string> _serializer;

        public RabbitMailmanLogic(
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
                    channel.ExchangeDeclare(
                        ExchangeName,
                        _exchange.ExchangeType,
						_exchange.Durable,
						_exchange.AutoDelete,
						_exchange.Properties);

                return channel.CreateBasicProperties();
            }
            catch (Exception ex)
            {
                throw new SetupException(ex);
            }
        }

        public void RabbitSendMessage(IModel channel, string message,
            IBasicProperties props, string routingKey, Action<string, Exception> onExceptionAction = null) 
        {
            try
            {
                channel.BasicPublish(ExchangeName,
                    routingKey,
                    props,
                    props.GetEncoding().GetBytes(message));
            }
            catch (Exception e)
            {
                (onExceptionAction ?? ((m, ex) => { }))(message, e);
            }
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
            var serializedMsg = _serializer(message);
            return channel => RabbitSendMessage(channel, serializedMsg, UpdateProperties(propertiesSetter)(channel), routingKey);
        }

        public void PublishOne<T>(T message, string routingKey, Action<IBasicProperties> applyStaticProperties = null)
        {
            TryApplyOnNewChannel(SendMessageOnChannel(message,routingKey, applyStaticProperties));
        }

        public void PublishSome<T>(IEnumerable<T> messages, string routingKey, Action<IBasicProperties> applyStaticProperties = null)
        {
            TryApplyOnNewChannel(SendManyMessagesOnChannel(messages,routingKey, applyStaticProperties));
        }

        private Action<IModel> SendManyMessagesOnChannel<T>(IEnumerable<T> messages, string routingKey, 
            Action<IBasicProperties> applyStaticProperties = null)
        {
            return messages
                .Select(m => SendMessageOnChannel(m, routingKey, applyStaticProperties))
                .Aggregate((previous, next) => previous + next);
        }

        public void CustomPublishSome<T, TP>(IList<IMessageProperty<T, TP>> messagePropertyTuples, string routingKey,
            Action<TP, IBasicProperties> applyDynamicProperties)
        {
            PublishSome(messagePropertyTuples.Select(mp => mp.Message), routingKey, 
                CollectPropertiesCustomizationOnChannel(messagePropertyTuples, applyDynamicProperties));
        }

        private static Action<IBasicProperties> CollectPropertiesCustomizationOnChannel<T, TP>(
            IEnumerable<IMessageProperty<T, TP>> messagePropertyTuples, 
            Action<TP, IBasicProperties> applyDynamicProperties)
        {
            return messagePropertyTuples
                .Select(mp => (Action<IBasicProperties>)(p => applyDynamicProperties(mp.Property, p)))
                .Aggregate((previous, next) => previous + next);
        }
    }
}