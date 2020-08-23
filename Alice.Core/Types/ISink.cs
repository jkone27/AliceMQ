namespace AliceMQ.Core.Types
{
    public interface ISink
    {
        ISource Source { get; }
        //QueueBind QueueBind { get; }
        ConfirmationPolicy ConfirmationPolicy { get; }
        string DeadLetterExchangeName { get; }
    }
}