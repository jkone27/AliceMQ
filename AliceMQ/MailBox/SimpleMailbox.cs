using System;
using System.Reactive.Linq;
using AliceMQ.Core;
using AliceMQ.Core.Exceptions;
using AliceMQ.Core.Message;
using AliceMQ.Core.Types;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.Rabbit.MailBox
{
    public sealed class SimpleMailbox: ISimpleMailbox
    {
        public ISink Sink { get; }
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

            //consumer.Received += (o, e) => observer.OnNext(new DeliveryContext(e, utility.Channel));
            //consumer.ConsumerCancelled += (o, e) => observer.OnCompleted();
            //consumer.Shutdown += (o, e) => observer.OnError(new Exception(e.ReplyText));
            //consumer.Unregistered += (o, e) => observer.OnCompleted();

            var ob = Observable
                .FromEventPattern<BasicDeliverEventArgs>(consumer, nameof(consumer.Received))
                .Select(e => e.EventArgs)
                .Select(s => new DeliveryContext(s, utility.Channel));

            var subscription = ob.Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
            utility.Disposables.Add(subscription);

            StartConsumer(utility.Channel, consumer);

            return utility.Disposables;
        }

        private void StartConsumer(IModel channel, EventingBasicConsumer consumer) =>
            channel.BasicConsume(Sink.Source.QueueArgs.QueueName, Sink.ConfirmationPolicy.AutoAck, consumer);

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