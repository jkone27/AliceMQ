using System;

namespace AliceMQ.MailBox.Core
{
    public interface ISimpleMailbox : IObservable<IDeliveryContext>
    {
        string ConnectionUrl { get; }
        string QueueName { get; }
        string ExchangeName { get; }
        string DeadLetterExchangeName { get; }
        Sink Sink { get; }
    }
}