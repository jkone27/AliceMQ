using System.Collections.Generic;

namespace Alice.MailBox
{
    public class QueueBind
    {
        public QueueBind(IDictionary<string, object> arguments, string routingKey)
        {
            Arguments = arguments;
            RoutingKey = routingKey;
        }

        public string RoutingKey { get; }
        public IDictionary<string, object> Arguments { get; }
    }
}