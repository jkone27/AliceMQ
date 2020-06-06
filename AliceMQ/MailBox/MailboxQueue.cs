using System.Collections.Generic;
using AliceMQ.MailMan;
using RabbitMQ.Client;

namespace AliceMQ.MailBox.Core
{
    public sealed class MailboxQueue : IMailboxQueue
    {
        private readonly Source _source;
        private readonly QueueBind _queueBind;
        private readonly IDictionary<string, object> _queueDeclareArguments;

        public MailboxQueue(Source source, QueueBind queueBind, IDictionary<string,object> queueDeclareArguments)
        {
            _source = source;
            _queueBind = queueBind;
            _queueDeclareArguments = queueDeclareArguments;
        }
        public void QueueBind(IModel channel)
        {
            channel.QueueBind(_source.QueueArgs.QueueName,
                _source.Exchange.ExchangeName,
                _queueBind.RoutingKey,
                _queueBind.Arguments);
        }

        public void QueueDeclare(IModel channel)
        {
            channel.QueueDeclare(_source.QueueArgs.QueueName,
                _source.QueueArgs.Durable,
                _source.QueueArgs.Exclusive,
                _source.QueueArgs.AutoDelete,
                _queueDeclareArguments);
        }

    }
}