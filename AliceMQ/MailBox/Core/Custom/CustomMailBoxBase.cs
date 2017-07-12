using System;
using System.Reactive.Linq;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox.Interface;
using AliceMQ.MailBox.Message;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Custom
{
    public abstract class CustomMailBoxBase<T>:
        IMailBox<IMessage>
    {
        protected readonly IMailBox<BasicDeliverEventArgs> MailBox;
        private readonly Func<string, T> _deserializer;

        protected CustomMailBoxBase(IMailBox<BasicDeliverEventArgs> mailbox, 
            Func<string, T> deserializer)
        {
            MailBox = mailbox;
            _deserializer = deserializer;
        }

        private IMessage ConsumeMessage(BasicDeliverEventArgs e)
        {
            try
            {
                var typedMessage = _deserializer(Payload(e));
                if (typedMessage == null)
                    throw new Exception("typedMessage is null.");

                return new Ok<T>(typedMessage, e);
            }
            catch (Exception ex)
            {
                return new Error(e, ex);
            }
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