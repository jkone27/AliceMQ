using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public class MailBox
        : IDisposable, IConfirmable<BasicDeliverEventArgs>
    {
        public string ConnectionUrl { get; private set; }
        public string QueueName => Parameters.MailArgs.QueueName;

        public string ExchangeName => Parameters.MailArgs.ExchangeName;

        public string DeadLetterExchangeName => Parameters.DeadLetterExchangeName;

        private readonly IConnectionFactory _factory;
        private IDisposable _subscription;
        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;
        private readonly List<ulong> _bagOfAckables;
        private readonly object _lock;


        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return _subject.Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
        }

        private readonly ISubject<BasicDeliverEventArgs> _subject;

        public readonly MailboxArgs Parameters;
        private readonly bool _autoAck;

        public bool IsConfirmable => !_autoAck;

        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);

        public bool DeadLettering => !string.IsNullOrWhiteSpace(DeadLetterExchangeName);

        private MailBox(bool autoAck)
        {
            _autoAck = autoAck;
            _subject = new Subject<BasicDeliverEventArgs>();
            _lock = new Object();
            _bagOfAckables = new List<ulong>();
        }

        public MailBox(string connectionUrl,
            MailboxArgs @params,
            int networkRecoveryIntervalMinutes,
            bool autoAck = true): this(autoAck) 
        {
            Parameters = @params;
            ConnectionUrl = connectionUrl;
            _factory = new ConnectionFactory
            {
                Uri = connectionUrl,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromMinutes(networkRecoveryIntervalMinutes)
            };
            Start();
        }

        public MailBox(
            ConnectionFactoryParams connParams,
            MailboxArgs @params,
            bool autoAck = true) : this(autoAck)
        {
            Parameters = @params;
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
            Start();
        }

        protected void Start()
        {
            try
            {
                SetupConsumer();
                SetupConsumerHandler();
            }
            catch (Exception ex)
            {
                throw new MailboxSetupException(ex);
            }
        }

        private IDisposable InnerSubscribe(EventingBasicConsumer consumer)
        {
            return
                Observable
                    .FromEventPattern<BasicDeliverEventArgs>(consumer,nameof(consumer.Received))
                    .Select(e => e.EventArgs)
                    .Buffer(TimeSpan.FromMilliseconds(200))
                    .SelectMany(s => s)
                    .Subscribe(OnNext, OnError, OnCompleted);
        }

        protected void OnCompleted()
        {
            _subject.OnCompleted();
        }

        protected void OnError(Exception e)
        {
            _subject.OnError(e);
        }

        protected void OnNext(BasicDeliverEventArgs a)
        {
            lock(_lock)
                _bagOfAckables.Add(a.DeliveryTag);

            _subject.OnNext(a);
        }

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

        protected virtual void SetupConsumerHandler()
        {
            _consumer = new EventingBasicConsumer(_channel);
            _subscription = InnerSubscribe(_consumer);
            _channel.BasicConsume(QueueName, _autoAck, _consumer);
        }

        protected virtual void SetupConsumer()
        {
            Channel();

            if (DeadLettering)
                DeadLetterSetup();

            QueueDeclare();

            if (!DefaultExchange)
                QueueBind();
        }

        private void QueueBind()
        {
            _channel.QueueBind(QueueName,
                Parameters.MailArgs.ExchangeName,
                Parameters.RoutingKey,
                Parameters.QueueBindArguments);
        }

        private void QueueDeclare()
        {
            _channel.QueueDeclare(QueueName,
                Parameters.MailArgs.Durable,
                Parameters.MailArgs.Exclusive,
                Parameters.MailArgs.AutoDelete,
                Parameters.QueueArguments);
        }

        private void Channel()
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.BasicQos(0, Parameters.PrefetchCount, Parameters.Global);
                //prefetchSize!=0 not implemented - https://www.rabbitmq.com/specification.html
        }

        protected virtual void DeadLetterSetup()
        {
            _channel.ExchangeDeclare(DeadLetterExchangeName, 
                Parameters.MailArgs.ExchangeType, 
                Parameters.MailArgs.Durable);

            Parameters.QueueArguments.Add("x-dead-letter-exchange", DeadLetterExchangeName);

            if (!string.IsNullOrWhiteSpace(Parameters.RoutingKey))
                Parameters.QueueArguments.Add("x-dead-letter-routing-key", Parameters.RoutingKey);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _connection?.Dispose();
            _channel?.Dispose();
        }

    }
}