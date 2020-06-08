using Alice.Core.Types;
using System.Collections.Generic;

namespace AliceMQ.GooglePubSub
{
    public class GoogleTopic : IExchange
    {
        public string ExchangeName { get; }

        public string ExchangeType => string.Empty;

        public bool Durable => true;

        public bool AutoDelete => false;

        public IDictionary<string, object> Properties => new Dictionary<string, object>();

        public GoogleTopic(string topicName)
        {
            ExchangeName = topicName;
        }
    }
}
