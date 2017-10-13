using System;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Custom
{
    public class CustomMailBox<T> : IAckableMailbox<IMessage>
    {
        private readonly CustomMailBoxBase<T> _customBase;

        public CustomMailBox(EndPoint simpleEndPoint,
            Sink sink,
            Func<string, T> deserializer)
        {
            _customBase = new CustomMailBoxBase<T>(new Simple.MailBox(simpleEndPoint, sink), deserializer);
        }

        protected CustomMailBox(IAckableMailbox<BasicDeliverEventArgs> mailbox,
            Func<string, T> deserializer)
        {
            _customBase = new CustomMailBoxBase<T>(mailbox, deserializer);
        }

        public bool AckRequest(ulong deliveryTag, bool multiple)
        {
            return ((IAckableMailbox<BasicDeliverEventArgs>) _customBase.MailBox).AckRequest(deliveryTag, multiple);
        }

        public bool NackRequest(ulong deliveryTag, bool multiple, bool requeue)
        {
            return ((IAckableMailbox<BasicDeliverEventArgs>)_customBase.MailBox).NackRequest(deliveryTag, multiple, requeue);
        }

        public IDisposable Subscribe(IObserver<IMessage> observer)
        {
            return _customBase.Subscribe(observer);
        }

        public IDisposable Connect()
        {
            return _customBase.Connect();
        }

        public void Dispose()
        {
            _customBase.Dispose();
        }
    }
}