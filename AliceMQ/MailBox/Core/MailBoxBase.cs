using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using AliceMQ.MailBox.Message;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public class CustomMailbox<T> : IObservable<IMessage>
    {
        private readonly Func<string, T> _serializer;
        private readonly IMailboxBase _baseMailbox;

        public CustomMailbox(EndPoint endPoint, Sink sink, Func<string, T> serializer)
        {
            _serializer = serializer;
            _baseMailbox = new MailBoxBase(endPoint, sink);
        }

        protected CustomMailbox(IMailboxBase mailboxBase, Func<string, T> serializer)
        {
            _baseMailbox = mailboxBase;
            _serializer = serializer;
        }

        public IDisposable Subscribe(IObserver<IMessage> observer)
        {
            return _baseMailbox.Select<IMailboxContext, IMessage>(s =>
            {
                try
                {
                    var decodedString = Payload(s.EventArgs);
                    return new Ok<T>(_serializer(decodedString), s, _baseMailbox.Sink.ConfirmationPolicy.Multiple, _baseMailbox.Sink.ConfirmationPolicy.Requeue);
                }
                catch (Exception ex)
                {
                    return new Error(s, ex, _baseMailbox.Sink.ConfirmationPolicy.Multiple, _baseMailbox.Sink.ConfirmationPolicy.Requeue);
                }
            })
            .Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
        }

        private static string Payload(BasicDeliverEventArgs e)
        {
            return e.BasicProperties.GetEncoding().GetString(e.Body);
        }
    }

    public sealed class MailBoxBase: IMailboxBase
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

        public MailBoxBase(EndPoint simpleEndpoint,
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