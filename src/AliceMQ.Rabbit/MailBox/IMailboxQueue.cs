using RabbitMQ.Client;

namespace AliceMQ.Rabbit.MailBox
{
    public interface IMailboxQueue
    {
        void QueueBind(IModel channel);
        void QueueDeclare(IModel channel);
    }
}