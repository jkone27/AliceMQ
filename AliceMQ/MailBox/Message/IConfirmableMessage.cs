namespace AliceMQ.MailBox.Message
{
    public interface IConfirmableMessage
    {
        void Accept(bool multiple = false);
        void Reject(bool multiple = false, bool requeue = false);
    }
}