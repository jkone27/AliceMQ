namespace Alice.Core.Types
{
    public interface IQueueArgs
    {
        string QueueName { get; }
        bool Durable { get; }
        bool Exclusive { get; }
        bool AutoDelete { get; }
    }
}