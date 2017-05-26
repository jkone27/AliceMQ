using System;
using System.Collections.Generic;
using System.Linq;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Simple
{
    public class MailBox
        : MailBoxBase, IAckableMailbox<BasicDeliverEventArgs>
    {

        private readonly List<ulong> _bagOfAckables;
        private readonly object _lock;


        public MailBox(SimpleEndpointArgs simpleEndpointArgs,
            MailboxArgs mailboxArgs) : base(simpleEndpointArgs, mailboxArgs, false)
        {
            _bagOfAckables = new List<ulong>();
            _lock = new object();
        }

        public MailBox(
            EndpointArgs connParams,
            MailboxArgs mailboxArgs) : base(connParams, mailboxArgs, false)
        {
            _bagOfAckables = new List<ulong>();
            _lock = new object();
        }

        public virtual bool AckRequest(ulong deliveryTag, bool multiple)
        {
            return Confirmation(deliveryTag, d => _channel.BasicAck(d, multiple));
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

        public virtual bool NackRequest(ulong deliveryTag, bool multiple, bool requeue)
        {
            return Confirmation(deliveryTag, d => _channel.BasicNack(d, multiple, requeue));
        }

        protected override void PrivateSequenceAction(BasicDeliverEventArgs s)
        {
            lock (_lock)
                _bagOfAckables.Add(s.DeliveryTag);
        }
    }
}