using AliceMQ.Core.Types;
using System;

namespace AliceMQ.GooglePubSub
{
    public class Sink : ISink
    {
        public ISource Source { get; }

        public QueueBind QueueBind { get; }

        public ConfirmationPolicy ConfirmationPolicy { get; }

        public string DeadLetterExchangeName { get; }

        public Sink(Source source, string subscriptionName)
        {
            Source = source;
        }
    }
}
