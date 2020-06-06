using Alice.Core.Message;
using Alice.Core.Types;
using System;

namespace Alice.Core
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