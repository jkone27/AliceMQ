using System;

namespace Alice.MailBox.Core
{
    public interface IMailboxBase : IObservable<IMailboxContext>
    {
        string ConnectionUrl { get; }
        string QueueName { get; }
        string ExchangeName { get; }
        string DeadLetterExchangeName { get; }
        Sink Sink { get; }
    }
}