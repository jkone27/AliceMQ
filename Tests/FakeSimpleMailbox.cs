using System;
using Alice.Core;
using Alice.Core.Message;
using Alice.Core.Types;
using AliceMQ.Rabbit.MailBox;

namespace Tests
{
    public class FakeSimpleMailbox : ISimpleMailbox
    {
        public IObservable<IDeliveryContext> Source;
        private readonly bool _autoAck;

        protected IObservable<IDeliveryContext> ConsumerReceivedObservable => Source;

        public FakeSimpleMailbox(IObservable<IDeliveryContext> source, bool autoAck = false)
        {
            Source = source;
            _autoAck = autoAck;
        }

        public IDisposable Subscribe(IObserver<IDeliveryContext> observer)
        {
            return Source.Subscribe(observer);
        }

        public string ConnectionUrl => "";
        public string QueueName => "";
        public string ExchangeName => "";
        public string DeadLetterExchangeName => "";
        public ISink Sink => new Sink(new FakeSource(), confirmationPolicy: new ConfirmationPolicy
        {
            AutoAck = _autoAck
        });
    }

}