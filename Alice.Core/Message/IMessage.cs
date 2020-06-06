namespace Alice.Core.Message
{
    public interface IMessage
    {
        void Confirm();
        void Reject();
    }
}