namespace Alice.MailBox.Interface
{
    public interface IMessage
    {
        void Confirm();
        void Reject();
    }
}