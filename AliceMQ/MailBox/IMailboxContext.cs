using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public interface IMailboxContext : IDeliveryContext
    {
        BasicDeliverEventArgs EventArgs { get; }
        IModel Channel { get; }
    }
}