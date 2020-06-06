using System.Collections.Generic;
using System.Linq;

namespace AliceMQ.MailMan
{
    public class Source : ISource
    {
        public IExchange Exchange { get; set; }
        public IQueueArgs QueueArgs { get; set; }

        public Source(
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
            Exchange = new Exchange(exchangeName, exchangeType, properties, durableExchange, autoDeleteExchange);
        }
    }
}