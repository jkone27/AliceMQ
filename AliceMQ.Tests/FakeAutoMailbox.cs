using System;
using AliceMQ.MailBox.EndPointArgs;
using RabbitMQ.Client.Events;

namespace AliceMQ.Tests
{
    public class FakeAutoMailbox : MailBox.Core.Simple.AutoMailBox
    {

        public IObservable<BasicDeliverEventArgs> Source;

        public FakeAutoMailbox(IObservable<BasicDeliverEventArgs> source)
            : base(new EndpointArgs(), null)
        {
            Source = source;
        }

        protected override IObservable<BasicDeliverEventArgs> ConsumerReceivedObservable => Source;

        protected override void StartConsumer()
        {
            //
        }

        protected override void SetupEnvironment()
        {
            //
        }
    }
}