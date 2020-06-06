namespace Alice.Core.Types
{
    public interface ISource
    {
        IExchange Exchange { get; }
        IQueueArgs QueueArgs { get; }
    }
}