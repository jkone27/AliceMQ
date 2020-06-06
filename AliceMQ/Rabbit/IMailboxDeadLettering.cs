using RabbitMQ.Client;

namespace AliceMQ.MailBox.Core
{
    public interface IMailboxDeadLettering
    {
        void DeadLetterSetup(IModel channel);
    }
}