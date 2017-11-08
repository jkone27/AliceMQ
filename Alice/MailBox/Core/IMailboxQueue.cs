using RabbitMQ.Client;

namespace Alice.MailBox.Core
{
    public interface IMailboxQueue
    {
        void QueueBind(IModel channel);
        void QueueDeclare(IModel channel);
    }
}