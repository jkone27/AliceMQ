namespace AliceMQ.Rabbit.MailBox
{
    public interface IMailboxChannel
    {
        MailboxConnection Connect();
    }
}