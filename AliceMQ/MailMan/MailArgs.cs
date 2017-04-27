using System;
using System.Collections.Generic;

namespace AliceMQ.MailMan
{
    public class MailArgs
    {
        public string ExchangeName { get; }
        public string QueueName { get; }
        public string ExchangeType { get;  }
        public bool Durable { get; }
        public bool Exclusive { get; }
        public bool AutoDelete { get; }
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

    public class Builder<T> where T : new()
    {
        private IList<Action<T>> _builderActions;

        public T Build()
        {
            var istance = new T();
            _builderActions = new List<Action<T>>();
            foreach(var ba in _builderActions)
                ba(istance);
            return istance;
        }

        public Builder<T> With(Action<T> defineProperty)
        {
            _builderActions.Add(defineProperty);
            return this;
        }
    }
}