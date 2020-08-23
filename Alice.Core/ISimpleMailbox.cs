using AliceMQ.Core.Message;
using AliceMQ.Core.Types;
using System;

namespace AliceMQ.Core
{
    public interface ISimpleMailbox : IObservable<IDeliveryContext>
    {
        string ConnectionUrl { get; }
        string QueueName { get; }
        string ExchangeName { get; }
        string DeadLetterExchangeName { get; }
        ISink Sink { get; }
    }
}