using System.Collections.Generic;
using AliceMQ.MailMan;

namespace AliceMQ.MailBox
{
    public class MailboxArgs
    {
        public string DeadLetterExchangeName { get; set; } //must be settable at runtime
        public IDictionary<string, object> QueueDeclareArguments { get; set; } //must be settable at runtime
        public SourceArgs Source { get; set; }
        public BasicQualityOfService BasicQualityOfService { get; }

        public QueueBind QueueBind { get; }

        public MailboxArgs(
            SourceArgs sourceArgs, 
            string routingKey = "",
            string deadLetterExchangeName = null,
            IDictionary<string, object> queueDeclareArguments = null,
            IDictionary<string, object> queueBindArguments = null,
            ushort prefetchCount = 1,
            bool global = false)
        {
            QueueDeclareArguments = queueDeclareArguments ?? new Dictionary<string, object>();
            QueueBind = new QueueBind(queueBindArguments ?? new Dictionary<string, object>(), routingKey);
            Source = sourceArgs;
            DeadLetterExchangeName = deadLetterExchangeName;
            BasicQualityOfService = new BasicQualityOfService(prefetchCount, global);
        }

        
    }
}