namespace AliceMQ.Core.Message
{
    public interface IMessage
    {
        void Confirm();
        void Reject();
    }
}