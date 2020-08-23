using AliceMQ.Core.Types;
using System.Collections.Generic;

namespace AliceMQ.Rabbit.MailBox
{
    public class Sink : ISink
    {
        public string DeadLetterExchangeName { get; set; } //must be settable at runtime
        public IDictionary<string, object> QueueDeclareArguments { get; set; } //must be settable at runtime
        public ISource Source { get; }
        public BasicQualityOfService BasicQualityOfService { get; }
        public ConfirmationPolicy ConfirmationPolicy { get; }
        public QueueBind QueueBind { get; }

        public Sink(
            ISource source,
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