using Alice.Core.Types;
using System;

namespace AliceMQ.GooglePubSub
{
    public class GoogleSink : ISink
    {
        public ISource Source { get; }

        public QueueBind QueueBind { get; }

        public ConfirmationPolicy ConfirmationPolicy { get; }

        public string DeadLetterExchangeName { get; }

        public GoogleSink(GoogleSource source, string subscriptionName)
        {
            Source = source;
        }
    }
}
