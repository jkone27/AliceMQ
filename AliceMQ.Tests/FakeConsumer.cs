using System;
using AliceMQ.MailBox.EndPointArgs;
using RabbitMQ.Client.Events;

namespace AliceMQ.Tests
{
    public class FakeConsumer : MailBox.Core.MailBox
    {
        public int Acks { get; private set; }
        public int Nacks { get; private set; }

        public IObservable<BasicDeliverEventArgs> Source;

        public FakeConsumer(IObservable<BasicDeliverEventArgs> source, bool autoAck) 
            : base(new EndpointArgs(), null, autoAck)
        {
            Source = source;
            Acks = 0;
            Nacks = 0;
        }

        public override bool AckRequest(ulong deliveryTag, bool multiple)
        {
            Acks++;
            return true;
        }

        public override bool NackRequest(ulong deliveryTag, bool multiple, bool requeue)
        {
            Nacks++;
            return true;
        }

        protected override IObservable<BasicDeliverEventArgs> SourceSequence => Source;

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