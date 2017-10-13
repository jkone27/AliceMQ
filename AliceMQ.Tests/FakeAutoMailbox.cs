using System;
using AliceMQ.MailBox.EndPointArgs;
using RabbitMQ.Client.Events;

namespace AliceMQ.Tests
{
    public class FakeAutoMailbox : MailBox.Core.Simple.AutoMailBox
    {

        public IObservable<BasicDeliverEventArgs> Source;

        public FakeAutoMailbox(IObservable<BasicDeliverEventArgs> source)
            : base(new EndPoint(), null)
        {
            Source = source;
        }

        protected IObservable<BasicDeliverEventArgs> ConsumerReceivedObservable => Source;

        protected void StartConsumer()
        {
            //
        }

        protected void SetupEnvironment()
        {
            //
        }
    }
}