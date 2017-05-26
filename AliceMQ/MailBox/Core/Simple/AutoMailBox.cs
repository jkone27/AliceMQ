using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Simple
{
    public class AutoMailBox
        : MailBoxBase, IAutoMailBox<BasicDeliverEventArgs>
    {
        public AutoMailBox(SimpleEndpointArgs simpleEndpointArgs,
            MailboxArgs mailboxArgs) : base(simpleEndpointArgs, mailboxArgs, true)
        {
        }

        public AutoMailBox(
            EndpointArgs connParams,
            MailboxArgs mailboxArgs) : base(connParams, mailboxArgs, true)
        {
        }
    }
}