using System.Collections.Generic;

namespace AliceMQ.MailMan
{
    public class MailArgs
    {
        public string ExchangeName { get; private set; }
        public string QueueName { get; private set; }
        public string ExchangeType { get; private set; }
        public bool Durable { get; private set; }
        public bool Exclusive { get; private set; }
        public bool AutoDelete { get; private set; }
        public IDictionary<string, object> ExchangeArguments { get; set; } //must be settable at runtime

        public MailArgs(
        string exchangeName,
        string queueName = "",
        string exchangeType = RabbitMQ.Client.ExchangeType.Direct,
        bool durable = false,
        bool exclusive = false,
        bool autoDelete = false,
        IDictionary<string, object> exchangeArguments = null)
        {
            QueueName = queueName;
            ExchangeName = exchangeName;
            ExchangeType = exchangeType;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            ExchangeArguments = exchangeArguments ?? new Dictionary<string, object>();
        }
    }
}