using RabbitMQ.Client;

namespace AliceMQ.MailBox.Core
{
    public interface IMailboxDeadLettering
    {
        void DeadLetterSetup(IModel channel);
    }

    public sealed class MailboxDeadLettering : IMailboxDeadLettering
    {
        private readonly Sink _sink;

        public MailboxDeadLettering(Sink sink)
        {
            _sink = sink;
        }

        public void DeadLetterSetup(IModel channel)
        {
            var ex = _sink.Source.Exchange;
            channel.ExchangeDeclare(_sink.DeadLetterExchangeName, ex.ExchangeType, ex.Durable);
            _sink.QueueDeclareArguments.Add("x-dead-letter-exchange", _sink.DeadLetterExchangeName);

            if (!string.IsNullOrWhiteSpace(_sink.QueueBind.RoutingKey))
                _sink.QueueDeclareArguments.Add("x-dead-letter-routing-key", _sink.QueueBind.RoutingKey);
        }
    }
}