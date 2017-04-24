using System;
using System.Collections.Generic;
using System.Linq;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailMan.Interface;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace AliceMQ.MailMan
{
    public abstract class RabbitMailmanLogic
    {
        public string ExchangeName => _mailArgs.ExchangeName;

        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);

        protected readonly ConnectionFactory Factory;

        private readonly MailArgs _mailArgs;
        private readonly Formatting _formatting;
        private readonly JsonSerializerSettings _serializerSettings;

        protected RabbitMailmanLogic(MailArgs mailArgs,
            string hostName = "localhost",
            int port = 5672,
            string userName = ConnectionFactory.DefaultUser,
            string password = ConnectionFactory.DefaultPass,
            string virtualHost = ConnectionFactory.DefaultVHost,
            Formatting formatting = Formatting.None,
            JsonSerializerSettings serializerSettings = null)
        {
            _mailArgs = mailArgs;
            _formatting = formatting;
            _serializerSettings = serializerSettings;
            Factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                VirtualHost = virtualHost
            };
        }

        protected RabbitMailmanLogic(string connectionUrl, MailArgs mailArgs, 
            Formatting formatting = Formatting.None,
            JsonSerializerSettings serializerSettings = null)
            : this(mailArgs)
        {
            _formatting = formatting;
            _serializerSettings = serializerSettings;
            Factory = new ConnectionFactory
            {
                Uri = connectionUrl
            };
        }

        protected IBasicProperties SetupChannel(IModel channel)
        {
            try
            {
                if (!DefaultExchange)
                    channel.ExchangeDeclare(
                        ExchangeName,
                        _mailArgs.ExchangeType,
                        _mailArgs.Durable,
                        _mailArgs.AutoDelete,
                        _mailArgs.ExchangeArguments);

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

        private string SerializeMessage<T>(T message)
        {
            return message as String ?? JsonConvert.SerializeObject(message, typeof(T), _formatting, _serializerSettings);
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
            var serializedMsg = SerializeMessage(message);
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