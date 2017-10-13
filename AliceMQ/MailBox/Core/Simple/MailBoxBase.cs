using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AliceMQ.MailBox.EndPointArgs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Simple
{
    public interface IMailboxBase : IDisposable, IConnectableObservable<BasicDeliverEventArgs>
    {
        string ConnectionUrl { get; }
        string QueueName { get; }
        string ExchangeName { get; }
        string DeadLetterExchangeName { get; }
        IModel Channel { get; }
    }

    public sealed class MailBoxBase: IMailboxBase
    {
        public string ConnectionUrl { get; }
        public string QueueName => Parameters.Source.QueueArgs.QueueName;

        public string ExchangeName => Parameters.Source.Exchange.ExchangeName;

        public string DeadLetterExchangeName => Parameters.DeadLetterExchangeName;

        private readonly IConnectionFactory _factory;
        private IConnection _connection;
        public IModel Channel { get; private set; }
        private readonly CompositeDisposable _compositeDisposable;


        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            SetupEnvironment();
            _consumer = new EventingBasicConsumer(Channel);

            return Observable
                .FromEventPattern<BasicDeliverEventArgs>(_consumer, nameof(_consumer.Received))
                .Select(e => e.EventArgs)
                .Do(PrivateSequenceAction)
                .Publish()
                .Subscribe(observer);
        }

        public readonly Sink Parameters;
        private readonly bool _autoAck;
        private EventingBasicConsumer _consumer;

        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);

        public bool DeadLettering => !string.IsNullOrWhiteSpace(DeadLetterExchangeName);

        private MailBoxBase(bool autoAck)
        {
            _autoAck = autoAck;
            _compositeDisposable = new CompositeDisposable();
        }

        public MailBoxBase(EndPoint simpleEndpoint,
            Sink sink,
            bool autoAck) : this(autoAck)
        {
            Parameters = sink;
            ConnectionUrl = simpleEndpoint.ConnectionUrl;
            _factory = new ConnectionFactory
            {
                Uri = new Uri(simpleEndpoint.ConnectionUrl),
                AutomaticRecoveryEnabled = simpleEndpoint.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = simpleEndpoint.NetworkRecoveryInterval
            };
        }

        public void PrivateSequenceAction(BasicDeliverEventArgs s)
        {
            //
        }

        public void StartConsumer() =>
            Channel.BasicConsume(Parameters.Source.QueueArgs.QueueName, _autoAck, _consumer);

        public void SetupEnvironment()
        {
            try
            {
                ChannelSetup();

                if (DeadLettering)
                    DeadLetterSetup();

                QueueDeclare();

                if (!DefaultExchange)
                    QueueBind();
            }
            catch (Exception ex)
            {
                throw new MailboxSetupException(ex);
            }
        }

        private void QueueBind()
        {
            Channel.QueueBind(Parameters.Source.QueueArgs.QueueName,
                Parameters.Source.Exchange.ExchangeName,
                Parameters.QueueBind.RoutingKey,
                Parameters.QueueBind.Arguments);
        }

        private void QueueDeclare()
        {
            Channel.QueueDeclare(Parameters.Source.QueueArgs.QueueName,
                Parameters.Source.QueueArgs.Durable,
                Parameters.Source.QueueArgs.Exclusive,
                Parameters.Source.QueueArgs.AutoDelete,
                Parameters.QueueDeclareArguments);
        }

        private void ChannelSetup()
        {
            _connection = _factory.CreateConnection();
            _compositeDisposable.Add(_connection);
            Channel = _connection.CreateModel();
            _compositeDisposable.Add(Channel);
            Channel.BasicQos(0, Parameters.BasicQualityOfService.PrefetchCount, Parameters.BasicQualityOfService.Global);
            //prefetchSize!=0 not implemented - https://www.rabbitmq.com/specification.html
        }

        private void DeadLetterSetup()
        {
            Channel.ExchangeDeclare(DeadLetterExchangeName,
                Parameters.Source.Exchange.ExchangeType,
                Parameters.Source.Exchange.Durable);

            Parameters.QueueDeclareArguments.Add("x-dead-letter-exchange", DeadLetterExchangeName);

            if (!string.IsNullOrWhiteSpace(Parameters.QueueBind.RoutingKey))
                Parameters.QueueDeclareArguments.Add("x-dead-letter-routing-key", Parameters.QueueBind.RoutingKey);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public IDisposable Connect()
        {
            StartConsumer();
            return _compositeDisposable;
        }
    }
}