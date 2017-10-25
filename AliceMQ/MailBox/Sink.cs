using System.Collections.Generic;
using AliceMQ.MailMan;

namespace AliceMQ.MailBox
{
    public class Sink
    {
        public string DeadLetterExchangeName { get; set; } //must be settable at runtime
        public IDictionary<string, object> QueueDeclareArguments { get; set; } //must be settable at runtime
        public Source Source { get; set; }
        public BasicQualityOfService BasicQualityOfService { get; }
        public bool AutoAck { get; }

        public QueueBind QueueBind { get; }

        public Sink(
            Source source, 
            string routingKey = "",
            string deadLetterExchangeName = null,
            IDictionary<string, object> queueDeclareArguments = null,
            IDictionary<string, object> queueBindArguments = null,
            ushort prefetchCount = 1,
            bool global = false,
            bool autoAck = false)
        {
            QueueDeclareArguments = queueDeclareArguments ?? new Dictionary<string, object>();
            QueueBind = new QueueBind(queueBindArguments ?? new Dictionary<string, object>(), routingKey);
            Source = source;
            DeadLetterExchangeName = deadLetterExchangeName;
            BasicQualityOfService = new BasicQualityOfService(prefetchCount, global);
            AutoAck = autoAck;
        }

        
    }
}