namespace AliceMQ.MailBox.Message
{
    public interface IMessage
    {
        void Confirm();
        void Reject();
    }
}