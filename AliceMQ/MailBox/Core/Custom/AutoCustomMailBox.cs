using System;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Custom
{
    public class AutoCustomMailBox<T> : CustomMailBoxBase<T>,
        IAutoMailBox<IMessage>
    {
        public AutoCustomMailBox(IAutoMailBox<BasicDeliverEventArgs> mailbox,
            Func<string,T> deserializer) : base(mailbox, deserializer)
        {

        }
    }
}