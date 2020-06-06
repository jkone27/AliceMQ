namespace AliceMQ.MailMan
{
    public interface ISource
    {
        IExchange Exchange { get; }
        IQueueArgs QueueArgs { get; }
    }
}