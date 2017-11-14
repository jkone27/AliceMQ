using System;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailMan;

namespace Tests
{
    public class FakeMailbox<T> : Mailbox<T>
    {
        protected FakeMailbox(EndPoint simpleEndPoint, Sink sink, Func<string, T> deserializer) 
            : base(simpleEndPoint, sink, deserializer)
        {
            throw new NotImplementedException();
        }

        public FakeMailbox(ISimpleMailbox simpleMailbox, Func<string, T> deserializer) 
            : base(simpleMailbox, deserializer)
        {
        }
    }

    public class FakeSimpleMailbox : ISimpleMailbox
    {
        public IObservable<IMailboxContext> Source;
        private readonly bool _autoAck;

        protected IObservable<IMailboxContext> ConsumerReceivedObservable => Source;

        public FakeSimpleMailbox(IObservable<IMailboxContext> source, bool autoAck = false)
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