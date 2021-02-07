using Alice.Core.Types;
using System.Collections.Generic;

namespace AliceMQ.PubSub
{
    public class Exchange : IExchange
    {
        public string ExchangeName { get; } // topic name

        public string ExchangeType => string.Empty;

        public bool Durable => true;

        public bool AutoDelete => false;

        public IDictionary<string, object> Properties => new Dictionary<string, object>();

        public Exchange(string topicName)
        {
            ExchangeName = topicName;
        }
    }
}
