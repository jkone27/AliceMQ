using System;
using System.Reactive.Linq;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox.Interface;
using AliceMQ.MailBox.Message;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Custom
{
    public abstract class CustomMailBoxBase<T>:
        IMailBox<IMessage>, IDisposable
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        protected readonly IMailBox<BasicDeliverEventArgs> MailBox;

        public bool IgnoreMissingJsonFields => _jsonSerializerSettings.MissingMemberHandling == MissingMemberHandling.Ignore;

        protected CustomMailBoxBase(IMailBox<BasicDeliverEventArgs> mailbox, 
            JsonSerializerSettings jsonSeralizerSettings = null)
        {
            MailBox = mailbox;
            _jsonSerializerSettings = jsonSeralizerSettings;
        }

        private IMessage ConsumeMessage(BasicDeliverEventArgs e)
        {
            try
            {
                var typedMessage = TypedMessage(Payload(e));

                if (typedMessage == null)
                    throw new Exception("typedMessage is null.");

                return new Ok<T>(typedMessage, e);
            }
            catch (Exception ex)
            {
                return new Error(e, ex);
            }
        }

        private T TypedMessage(string payload)
        {
            return typeof(T) == typeof(string)
                ? (T) (object) payload
                : JsonConvert.DeserializeObject<T>(payload, _jsonSerializerSettings);
        }

        private static string Payload(BasicDeliverEventArgs e)
        {
            return e.BasicProperties.GetEncoding().GetString(e.Body);
        }

        public IDisposable Subscribe(IObserver<IMessage> observer) =>
            MailBox.Select(ConsumeMessage).AsObservable().Subscribe(observer);

        public IDisposable Connect() => MailBox.Connect();
        public void Dispose()
        {
            MailBox.Dispose();
        }
    }
}