using System.Collections.Generic;

namespace AliceMQ.MailMan
{
    public class SourceArgs
    {
        public ExchangeArgs ExchangeArgs { get; }
        public QueueArgs QueueArgs { get; }

        public SourceArgs(
        string exchangeName,
        string queueName = "",
        string exchangeType = RabbitMQ.Client.ExchangeType.Direct,
        bool durableQueue = false,
        bool durableExchange = false,
        bool exclusive = false,
        bool autoDeleteQueue = false,
		bool autoDeleteExchange = false,
        IDictionary<string, object> properties = null)
        {
			QueueArgs = new QueueArgs(queueName, durableQueue, exclusive, autoDeleteQueue);
            ExchangeArgs = new ExchangeArgs(exchangeName, exchangeType, properties, durableExchange, autoDeleteExchange);
        }
    }
}