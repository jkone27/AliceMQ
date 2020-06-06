namespace AliceMQ.MailBox.Core
{
    public interface IMailboxChannel
    {
        MailboxConnection Connect();
    }
}