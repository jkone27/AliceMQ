using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Alice.MailBox.Core
{
    public class MailboxContext : IMailboxContext
    {
        public BasicDeliverEventArgs EventArgs { get; set; }
        public IModel Channel { get; set; }
    }
}