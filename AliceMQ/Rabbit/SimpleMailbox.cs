using System;
using System.Reactive.Linq;
using AliceMQ.MailBox.EndPointArgs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public sealed class SimpleMailbox: ISimpleMailbox
    {
        public Sink Sink { get; }
        public string ConnectionUrl { get; }
        public string QueueName => Sink.Source.QueueArgs.QueueName;
        public string ExchangeName => Sink.Source.Exchange.ExchangeName;
        public string DeadLetterExchangeName => Sink.DeadLetterExchangeName;
        private readonly IMailboxChannel _mailboxChannel;
        private readonly IMailboxQueue _mailboxQueue;
        private readonly IMailboxDeadLettering _deadLettering;
        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);
        public bool DeadLettering => !string.IsNullOrWhiteSpace(DeadLetterExchangeName);

        public SimpleMailbox(EndPoint simpleEndpoint,
            Sink sink)
        {
            Sink = sink;
            ConnectionUrl = simpleEndpoint.ConnectionUrl;
            _mailboxChannel = new MailboxChannel(simpleEndpoint, sink.BasicQualityOfService);
            _mailboxQueue = new MailboxQueue(sink.Source, sink.QueueBind, sink.QueueDeclareArguments);
            _deadLettering = new MailboxDeadLettering(sink);
        }

        public IDisposable Subscribe(IObserver<IDeliveryContext> observer)
        {
            var utility = SetupEnvironment();
            var consumer = new EventingBasicConsumer(utility.Channel);

            var ob = Observable
                .FromEventPattern<BasicDeliverEventArgs>(consumer, nameof(consumer.Received))
                .Select(e => e.EventArgs)
                .Select(s => new MailboxContext(s, utility.Channel));

            var subscription = ob.Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
            utility.Disposables.Add(subscription);

            StartConsumer(utility.Channel, consumer);

            return utility.Disposables;
        }

        private void StartConsumer(IModel channel, EventingBasicConsumer consumer) =>
            channel.BasicConsume(Sink.Source.QueueArgs.QueueName, 
                Sink.ConfirmationPolicy.AutoAck, consumer);

        private MailboxConnection SetupEnvironment()
        {
            try
            {
                var connection = _mailboxChannel.Connect();

                if (DeadLettering)
                {
                    _deadLettering.DeadLetterSetup(connection.Channel);
                }  

                _mailboxQueue.QueueDeclare(connection.Channel);

                if (!DefaultExchange)
                {
                    _mailboxQueue.QueueBind(connection.Channel);
                }
                    
                return connection;
            }
            catch (Exception ex)
            {
                throw new MailboxSetupException(ex);
            }
        }
    }
}