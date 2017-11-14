using RabbitMQ.Client;

namespace AliceMQ.MailBox.Core
{
    public interface IMailboxQueue
    {
        void QueueBind(IModel channel);
        void QueueDeclare(IModel channel);
    }
}