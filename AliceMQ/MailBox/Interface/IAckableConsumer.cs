namespace AliceMQ.MailBox.Interface
{
    public interface IAckableConsumer
    {
        bool AckRequest(ulong deliveryTag, bool multiple);
        bool NackRequest(ulong deliveryTag, bool multiple, bool requeue);
    }
}