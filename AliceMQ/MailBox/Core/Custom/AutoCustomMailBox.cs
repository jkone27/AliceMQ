using AliceMQ.MailBox.Interface;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Custom
{
    public class AutoCustomMailBox<T> : CustomMailBoxBase<T>,
        IAutoMailBox<IMessage>
    {
        public AutoCustomMailBox(IAutoMailBox<BasicDeliverEventArgs> mailbox,
            JsonSerializerSettings jsonSeralizerSettings = null) : base(mailbox, jsonSeralizerSettings)
        {

        }
    }
}