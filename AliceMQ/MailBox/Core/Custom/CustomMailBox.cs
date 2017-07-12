using System;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Custom
{
    public class CustomMailBox<T> : CustomMailBoxBase<T>,
       IAckableMailbox<IMessage>
    {
        public CustomMailBox(IAckableMailbox<BasicDeliverEventArgs> mailbox,
            Func<string,T> deserializer):base(mailbox, deserializer) { }

        public bool AckRequest(ulong deliveryTag, bool multiple)
        {
            return ((IAckableMailbox<BasicDeliverEventArgs>) MailBox).AckRequest(deliveryTag, multiple);
        }

        public bool NackRequest(ulong deliveryTag, bool multiple, bool requeue)
        {
            return ((IAckableMailbox<BasicDeliverEventArgs>) MailBox).NackRequest(deliveryTag, multiple, requeue);
        }
    }
}