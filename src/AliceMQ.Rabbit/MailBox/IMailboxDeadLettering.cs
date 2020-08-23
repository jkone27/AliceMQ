using RabbitMQ.Client;

namespace AliceMQ.Rabbit.MailBox
{
    public interface IMailboxDeadLettering
    {
        void DeadLetterSetup(IModel channel);
    }
}