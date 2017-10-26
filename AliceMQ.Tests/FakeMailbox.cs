using System;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailMan;

namespace AliceMQ.Tests
{
    public class FakeCustomMailbox<T> : CustomMailbox<T>
    {
        protected FakeCustomMailbox(EndPoint simpleEndPoint, Sink sink, Func<string, T> deserializer) 
            : base(simpleEndPoint, sink, deserializer)
        {
            throw new NotImplementedException();
        }

        public FakeCustomMailbox(IMailboxBase mailbox, Func<string, T> deserializer) 
            : base(mailbox, deserializer)
        {
        }
    }

    public class FakeMailbox : IMailboxBase
    {
        public IObservable<IMailboxContext> Source;
        private readonly bool _autoAck;

        protected IObservable<IMailboxContext> ConsumerReceivedObservable => Source;

        public FakeMailbox(IObservable<IMailboxContext> source, bool autoAck = false)
        {
            Source = source;
            _autoAck = autoAck;
        }

        public IDisposable Subscribe(IObserver<IMailboxContext> observer)
        {
            return Source.Subscribe(observer);
        }

        public string ConnectionUrl => "";
        public string QueueName => "";
        public string ExchangeName => "";
        public string DeadLetterExchangeName => "";
        public Sink Sink => new Sink(new Source(""), confirmationPolicy: new ConfirmationPolicy
        {
            AutoAck = _autoAck
        });
    }
}