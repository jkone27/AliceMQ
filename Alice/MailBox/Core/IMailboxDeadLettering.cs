using RabbitMQ.Client;

namespace Alice.MailBox.Core
{
    public interface IMailboxDeadLettering
    {
        void DeadLetterSetup(IModel channel);
    }
}