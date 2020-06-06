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
        public ConfirmationPolicy ConfirmationPolicy { get; }
        public QueueBind QueueBind { get; }

        public Sink(
            Source source, 
            string routingKey = "",
            string deadLetterExchangeName = null,
            IDictionary<string, object> queueDeclareArguments = null,
            IDictionary<string, object> queueBindArguments = null,
            ushort prefetchCount = 1,
            bool global = false,
            ConfirmationPolicy confirmationPolicy = null)
        {
            QueueDeclareArguments = queueDeclareArguments ?? new Dictionary<string, object>();
            QueueBind = new QueueBind(queueBindArguments ?? new Dictionary<string, object>(), routingKey);
            Source = source;
            DeadLetterExchangeName = deadLetterExchangeName;
            BasicQualityOfService = new BasicQualityOfService(prefetchCount, global);
            ConfirmationPolicy = confirmationPolicy ?? new ConfirmationPolicy();
        }

        
    }
}