using System;
using System.Reactive.Subjects;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core.Simple;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.Tests
{
    public class FakeCustomMailbox<T> : MailBox.Core.Custom.CustomMailBox<T>
    {
        public FakeCustomMailbox(EndPoint simpleEndPoint, Sink sink, Func<string, T> deserializer) 
            : base(simpleEndPoint, sink, deserializer)
        {
            throw new NotImplementedException();
        }

        public FakeCustomMailbox(IAckableMailbox<BasicDeliverEventArgs> mailbox, Func<string, T> deserializer) 
            : base(mailbox, deserializer)
        {
        }
    }

    public class FakeConfirmableMailbox<T> : MailBox.Core.ConfirmableMailbox<T>
    {
        public FakeConfirmableMailbox(EndPoint simpleEndPoint, Sink sink, Func<string, T> deserializer) 
            : base(simpleEndPoint, sink, deserializer)
        {
            throw new NotImplementedException();
        }

        public FakeConfirmableMailbox(IAckableMailbox<IMessage> customMailBox) : base(customMailBox)
        {
        }
    }

    public class FakeMailbox : MailBox.Core.Simple.MailBox
    {
        public IObservable<BasicDeliverEventArgs> Source;

        protected IObservable<BasicDeliverEventArgs> ConsumerReceivedObservable => Source;

        protected void StartConsumer()
        {
            //
        }

        protected void SetupEnvironment()
        {
            //
        }

        public FakeMailbox(EndPoint simpleEndpoint, Sink sink) : base(simpleEndpoint, sink)
        {
            throw new NotImplementedException();
        }

        protected FakeMailbox(IMailboxBase mailboxBase) : base(mailboxBase)
        {
        }
    }

    public class FakeBase : IMailboxBase
    {
        private readonly IConnectableObservable<BasicDeliverEventArgs> _fake;

        public FakeBase(IConnectableObservable<BasicDeliverEventArgs> fake)
        {
            _fake = fake;
        }

        public void Dispose()
        {
            //
        }

        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return _fake.Subscribe(observer);
        }

        public IDisposable Connect()
        {
            return _fake.Connect();
        }

        public string ConnectionUrl { get; }
        public string QueueName { get; }
        public string ExchangeName { get; }
        public string DeadLetterExchangeName { get; }
        public IModel Channel { get; }
    }
}