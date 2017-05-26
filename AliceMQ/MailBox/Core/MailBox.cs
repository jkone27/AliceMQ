using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public class MailBox
        : IMailBox<BasicDeliverEventArgs>, IDisposable
    {
        public string ConnectionUrl { get; private set; }
        public string QueueName => Parameters.Source.QueueArgs.QueueName;

        public string ExchangeName => Parameters.Source.ExchangeArgs.ExchangeName;

        public string DeadLetterExchangeName => Parameters.DeadLetterExchangeName;

        private readonly IConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private readonly List<ulong> _bagOfAckables;
        private readonly object _lock;
        private readonly CompositeDisposable _compositeDisposable;


        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return PublicSequence().Subscribe(observer);
        }

        public readonly MailboxArgs Parameters;
        private readonly bool _autoAck;
        private EventingBasicConsumer _consumer;

        public bool IsConfirmable => !_autoAck;

        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);

        public bool DeadLettering => !string.IsNullOrWhiteSpace(DeadLetterExchangeName);

        private MailBox(bool autoAck)
        {
            _autoAck = autoAck;
            _lock = new Object();
            _bagOfAckables = new List<ulong>();
            _compositeDisposable = new CompositeDisposable();
            _started = false;
        }

        public MailBox(SimpleEndpointArgs simpleEndpointArgs,
            MailboxArgs mailboxArgs,
            bool autoAck = true) : this(autoAck)
        {
            Parameters = mailboxArgs;
            ConnectionUrl = simpleEndpointArgs.ConnectionUrl;
            _factory = new ConnectionFactory
            {
                Uri = simpleEndpointArgs.ConnectionUrl,
                AutomaticRecoveryEnabled = simpleEndpointArgs.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = simpleEndpointArgs.NetworkRecoveryInterval
            };
        }

        public MailBox(
            EndpointArgs connParams,
            MailboxArgs mailboxArgs,
            bool autoAck = true) : this(autoAck)
        {
            Parameters = mailboxArgs;
            _factory = new ConnectionFactory
            {
                HostName = connParams.HostName,
                Port = connParams.Port,
                UserName = connParams.UserName,
                Password = connParams.Password,
                VirtualHost = connParams.VirtualHost,
                AutomaticRecoveryEnabled = connParams.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = connParams.NetworkRecoveryInterval
            };
            ConnectionUrl =
                $"amqp://{connParams.UserName}:{connParams.Password}@{connParams.HostName}:{connParams.Port}/{connParams.VirtualHost}";
        }

        private bool _started;

        private IObservable<BasicDeliverEventArgs> PublicSequence()
        {
            if (!_started)
            {
                SetupEnvironment();
                _started = true;
                _uniqueSequence = SourceSequence
                    .Do(s =>
                    {
                        lock (_lock)
                            _bagOfAckables.Add(s.DeliveryTag);
                    })
                    .Publish();
                _uniqueSequence.Connect();
                StartConsumer();
            }

            return _uniqueSequence;
        }

        protected virtual void StartConsumer()
        {
            _channel.BasicConsume(Parameters.Source.QueueArgs.QueueName, _autoAck, _consumer);
        }

        private IConnectableObservable<BasicDeliverEventArgs> _uniqueSequence;
            

        protected virtual IObservable<BasicDeliverEventArgs> SourceSequence =>
        Observable.Defer(() =>
        {
            _consumer = new EventingBasicConsumer(_channel);
            return Observable
                .FromEventPattern<BasicDeliverEventArgs>(_consumer, nameof(_consumer.Received))
                .Select(e => e.EventArgs);
        });
            

        public virtual bool AckRequest(ulong deliveryTag, bool multiple)
        {
            return Confirmation(deliveryTag, d => _channel.BasicAck(d, multiple));
        }

        private bool Confirmation(ulong deliveryTag, Action<ulong> action)
        {
            ThrowIfNonAckable();
            lock (_lock)
                if (IsConfirmable && _bagOfAckables.Any(a => a == deliveryTag))
                {
                    return ConfirmationAction(deliveryTag, action);
                }
            return IsConfirmable;
        }

        private bool ConfirmationAction(ulong deliveryTag, Action<ulong> action)
        {
            action(deliveryTag);
            _bagOfAckables.Remove(deliveryTag);
            return true;
        }

        private void ThrowIfNonAckable()
        {
            if (!IsConfirmable)
                throw new MailboxException("acknowledge not supported");
        }

        public virtual bool NackRequest(ulong deliveryTag, bool multiple, bool requeue)
        {
            return Confirmation(deliveryTag, d => _channel.BasicNack(d, multiple, requeue));
        }

        protected virtual void SetupEnvironment()
        {
            try
            {
                Channel();

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
            _channel.QueueBind(Parameters.Source.QueueArgs.QueueName,
                Parameters.Source.ExchangeArgs.ExchangeName,
                Parameters.QueueBind.RoutingKey,
                Parameters.QueueBind.Arguments);
        }

        private void QueueDeclare()
        {
            _channel.QueueDeclare(Parameters.Source.QueueArgs.QueueName,
                Parameters.Source.QueueArgs.Durable,
                Parameters.Source.QueueArgs.Exclusive,
                Parameters.Source.QueueArgs.AutoDelete,
                Parameters.QueueDeclareArguments);
        }

        private void Channel()
        {
            _connection = _factory.CreateConnection();
            _compositeDisposable.Add(_connection);
            _channel = _connection.CreateModel();
            _compositeDisposable.Add(_channel);
            _channel.BasicQos(0, Parameters.BasicQualityOfService.PrefetchCount, Parameters.BasicQualityOfService.Global);
            //prefetchSize!=0 not implemented - https://www.rabbitmq.com/specification.html
        }

        private void DeadLetterSetup()
        {
            _channel.ExchangeDeclare(DeadLetterExchangeName,
                Parameters.Source.ExchangeArgs.ExchangeType,
                Parameters.Source.ExchangeArgs.Durable);

            Parameters.QueueDeclareArguments.Add("x-dead-letter-exchange", DeadLetterExchangeName);

            if (!string.IsNullOrWhiteSpace(Parameters.QueueBind.RoutingKey))
                Parameters.QueueDeclareArguments.Add("x-dead-letter-routing-key", Parameters.QueueBind.RoutingKey);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}