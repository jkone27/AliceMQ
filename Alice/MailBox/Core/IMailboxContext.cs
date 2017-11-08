using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Alice.MailBox.Core
{
    public interface IMailboxContext
    {
        BasicDeliverEventArgs EventArgs { get; }
        IModel Channel { get; }
    }
}