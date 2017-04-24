using System.Collections.Generic;
using AliceMQ.MailMan;

namespace AliceMQ.MailBox
{
    public class MailboxArgs
    {
        public string RoutingKey { get; private set; }
        public string DeadLetterExchangeName { get; private set; }
        public ushort PrefetchCount { get; private set; }
        public bool Global { get; private set; }
        public IDictionary<string, object> QueueBindArguments { get; set; } //must be settable at runtime
        public IDictionary<string, object> QueueArguments { get; set; } //must be settable at runtime
        public MailArgs MailArgs { get; private set; }

        public MailboxArgs(
            MailArgs mailArgs, 
            string routingKey = "",
            string deadLetterExchangeName = null,
            IDictionary<string, object> queueArguments = null,
            IDictionary<string, object> queueBindArguments = null,
            ushort prefetchCount = 1,
            bool global = false)
        {
            QueueArguments = queueArguments ?? new Dictionary<string, object>();
            QueueBindArguments = queueBindArguments ?? new Dictionary<string, object>();
            MailArgs = mailArgs;
            RoutingKey = routingKey;
            DeadLetterExchangeName = deadLetterExchangeName;
            PrefetchCount = prefetchCount;
            Global = global;
        }

    }
}