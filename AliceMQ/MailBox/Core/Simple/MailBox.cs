using System;
using System.Collections.Generic;
using System.Linq;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Simple
{
    public class MailBox
        : IAckableMailbox<BasicDeliverEventArgs>
    {
        private readonly List<ulong> _bagOfAckables;
        private readonly object _lock;
        private readonly IMailboxBase _baseMailBox;

        public MailBox(EndPoint simpleEndpoint,
            Sink sink)
        {
            _baseMailBox = new MailBoxBase(simpleEndpoint, sink, false);
            _bagOfAckables = new List<ulong>();
            _lock = new object();
        }

        protected MailBox(IMailboxBase mailboxBase)
        {
            _baseMailBox = mailboxBase;
            _bagOfAckables = new List<ulong>();
            _lock = new object();
        }

        public bool AckRequest(ulong deliveryTag, bool multiple)
        {
            return Confirmation(deliveryTag, d => _baseMailBox.Channel.BasicAck(d, multiple));
        }

        private bool Confirmation(ulong deliveryTag, Action<ulong> action)
        {
            lock (_lock)
                if(_bagOfAckables.Any(a => a == deliveryTag))
                {
                    return ConfirmationAction(deliveryTag, action);
                }
            return false;
        }

        private bool ConfirmationAction(ulong deliveryTag, Action<ulong> action)
        {
            action(deliveryTag);
            _bagOfAckables.Remove(deliveryTag);
            return true;
        }

        public bool NackRequest(ulong deliveryTag, bool multiple, bool requeue)
        {
            return Confirmation(deliveryTag, d => _baseMailBox.Channel.BasicNack(d, multiple, requeue));
        }

        protected void PrivateSequenceAction(BasicDeliverEventArgs s)
        {
            lock (_lock)
                _bagOfAckables.Add(s.DeliveryTag);
        }

        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return _baseMailBox.Subscribe(observer);
        }

        public IDisposable Connect()
        {
            return _baseMailBox.Connect();
        }

        public void Dispose()
        {
            _baseMailBox.Dispose();
        }
    }
}