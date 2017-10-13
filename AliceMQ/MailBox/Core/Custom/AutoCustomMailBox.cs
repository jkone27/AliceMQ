using System;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Custom
{
    public class AutoCustomMailBox<T> : IAutoMailBox<IMessage>
    {
        private readonly CustomMailBoxBase<T> _customBase;

        public AutoCustomMailBox(IAutoMailBox<BasicDeliverEventArgs> mailbox,
            Func<string,T> deserializer)
        {
            _customBase = new CustomMailBoxBase<T>(mailbox, deserializer);
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