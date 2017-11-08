using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Alice.MailBox.EndPointArgs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Alice.MailBox.Core
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

        private class Utility
        {
            public CompositeDisposable CompositeDisposable { get; set; }
            public IModel Channel { get; set; }
        }

        public SimpleMailbox(EndPoint simpleEndpoint,
            Sink sink)
        {
            Sink = sink;
            ConnectionUrl = simpleEndpoint.ConnectionUrl;
            _mailboxChannel = new MailboxChannel(simpleEndpoint, sink.BasicQualityOfService);
            _mailboxQueue = new MailboxQueue(sink.Source, sink.QueueBind, sink.QueueDeclareArguments);
            _deadLettering = new MailboxDeadLettering(sink);
        }

        public IDisposable Subscribe(IObserver<IMailboxContext> observer)
        {
            var utility = SetupEnvironment();
            var consumer = new EventingBasicConsumer(utility.Channel);

            var ob = Observable
                .FromEventPattern<BasicDeliverEventArgs>(consumer, nameof(consumer.Received))
                .Select(e => e.EventArgs)
                .Select(s => new MailboxContext
                {
                    EventArgs = s,
                    Channel = utility.Channel
                });

            var subscription = ob.Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
            utility.CompositeDisposable.Add(subscription);

            StartConsumer(utility.Channel, consumer);

            return utility.CompositeDisposable;
        }

        private void StartConsumer(IModel channel, EventingBasicConsumer consumer) =>
            channel.BasicConsume(Sink.Source.QueueArgs.QueueName, Sink.ConfirmationPolicy.AutoAck, consumer);

        private Utility SetupEnvironment()
        {
            try
            {
                _mailboxChannel.Connect(out var channel, out var compositeDisposable);

                if (DeadLettering)
                    _deadLettering.DeadLetterSetup(channel);

                _mailboxQueue.QueueDeclare(channel);

                if (!DefaultExchange)
                    _mailboxQueue.QueueBind(channel);

                return new Utility
                {
                    Channel = channel,
                    CompositeDisposable = compositeDisposable
                };
            }
            catch (Exception ex)
            {
                throw new MailboxSetupException(ex);
            }
        }
    }
}