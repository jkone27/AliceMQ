using AliceMQ.MailBox.Interface;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Custom
{
    public class CustomMailBox<T> : CustomMailBoxBase<T>,
       IAckableMailbox<IMessage>
    {
        public CustomMailBox(IAckableMailbox<BasicDeliverEventArgs> mailbox,
            JsonSerializerSettings jsonSeralizerSettings = null):base(mailbox, jsonSeralizerSettings)
        {
            
        }

        public bool AckRequest(ulong deliveryTag, bool multiple)
        {
            return ((IAckableMailbox<BasicDeliverEventArgs>) _mailBox).AckRequest(deliveryTag, multiple);
        }

        public bool NackRequest(ulong deliveryTag, bool multiple, bool requeue)
        {
            return ((IAckableMailbox<BasicDeliverEventArgs>) _mailBox).NackRequest(deliveryTag, multiple, requeue);
        }
    }
}