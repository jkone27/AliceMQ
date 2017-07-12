using System;
using System.Collections.Generic;
using System.Linq;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailMan.Interface;
using RabbitMQ.Client;

namespace AliceMQ.MailMan
{
    public abstract class RabbitMailmanLogic
    {
        public string ExchangeName => _exchangeArgs.ExchangeName;
        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);

        protected readonly ConnectionFactory Factory;
		private readonly ExchangeArgs _exchangeArgs;
        private readonly Func<object, string> _serializer;

        protected RabbitMailmanLogic(
            EndpointArgs endpointArgs, 
            ExchangeArgs exchangeArgs,
             Func<object, string> serializer)
        {
			_exchangeArgs = exchangeArgs;
            _serializer = serializer;
            Factory = new ConnectionFactory
            {
                HostName = endpointArgs.HostName,
                Port = endpointArgs.Port,
                UserName = endpointArgs.UserName,
                Password = endpointArgs.Password,
                VirtualHost = endpointArgs.VirtualHost,
                NetworkRecoveryInterval = endpointArgs.NetworkRecoveryInterval,
                AutomaticRecoveryEnabled = endpointArgs.AutomaticRecoveryEnabled
            };
        }

        protected RabbitMailmanLogic(
            SimpleEndpointArgs simpleEndpointArgs, 
            ExchangeArgs exchangeArgs, 
            Func<object,string> serializer)
        {
			_exchangeArgs = exchangeArgs;
            _serializer = serializer;
            Factory = new ConnectionFactory
            {
                Uri = simpleEndpointArgs.ConnectionUrl,
                NetworkRecoveryInterval = simpleEndpointArgs.NetworkRecoveryInterval,
                AutomaticRecoveryEnabled = simpleEndpointArgs.AutomaticRecoveryEnabled
            };
        }

        protected IBasicProperties SetupChannel(IModel channel)
        {
            try
            {
                if (!DefaultExchange)
                    channel.ExchangeDeclare(
                        ExchangeName,
                        _exchangeArgs.ExchangeType,
						_exchangeArgs.Durable,
						_exchangeArgs.AutoDelete,
						_exchangeArgs.Properties);

                return channel.CreateBasicProperties();
            }
            catch (Exception ex)
            {
                throw new MailmanSetupException(ex);
            }
        }

        protected void RabbitSendMessage(IModel channel, string message,
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
            catch (MailmanSetupException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MailmanSetupException(ex);
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

        protected void PublishOne<T>(T message, string routingKey, Action<IBasicProperties> applyStaticProperties = null)
        {
            TryApplyOnNewChannel(SendMessageOnChannel(message,routingKey, applyStaticProperties));
        }

        protected void PublishSome<T>(IEnumerable<T> messages, string routingKey, Action<IBasicProperties> applyStaticProperties = null)
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

        protected void CustomPublishSome<T, TP>(IList<IMessageProperty<T, TP>> messagePropertyTuples, string routingKey,
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