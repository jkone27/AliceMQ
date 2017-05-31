using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AliceMQ.MailBox.EndPointArgs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Simple
{
    public abstract class MailBoxBase: IDisposable
    {
        public string ConnectionUrl { get; private set; }
        public string QueueName => Parameters.Source.QueueArgs.QueueName;

        public string ExchangeName => Parameters.Source.ExchangeArgs.ExchangeName;

        public string DeadLetterExchangeName => Parameters.DeadLetterExchangeName;

        private readonly IConnectionFactory _factory;
        private IConnection _connection;
        protected IModel Channel;
        private readonly CompositeDisposable _compositeDisposable;


        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return PrivateSequence().Subscribe(observer);
        }

        public readonly MailboxArgs Parameters;
        private readonly bool _autoAck;
        private EventingBasicConsumer _consumer;
        private IConnectableObservable<BasicDeliverEventArgs> _privateConnectableSequence;

        public bool DefaultExchange => string.IsNullOrWhiteSpace(ExchangeName);

        public bool DeadLettering => !string.IsNullOrWhiteSpace(DeadLetterExchangeName);

        private MailBoxBase(bool autoAck)
        {
            _autoAck = autoAck;
            _compositeDisposable = new CompositeDisposable();
            _privateConnectableSequence = null;
        }

        protected MailBoxBase(SimpleEndpointArgs simpleEndpointArgs,
            MailboxArgs mailboxArgs,
            bool autoAck) : this(autoAck)
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

        protected MailBoxBase(
            EndpointArgs connParams,
            MailboxArgs mailboxArgs,
            bool autoAck) : this(autoAck)
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


        private IConnectableObservable<BasicDeliverEventArgs> PrivateSequence()
        {
            if (_privateConnectableSequence == null)
            {
                SetupEnvironment();
                _consumer = new EventingBasicConsumer(Channel);

                _privateConnectableSequence =
                    ConsumerReceivedObservable
                        .Do(PrivateSequenceAction)
                        .Publish();
            }
            return _privateConnectableSequence;
        }

        protected virtual void PrivateSequenceAction(BasicDeliverEventArgs s)
        {
            //
        }

        protected virtual void StartConsumer() =>
            Channel.BasicConsume(Parameters.Source.QueueArgs.QueueName, _autoAck, _consumer);

        protected virtual IObservable<BasicDeliverEventArgs> ConsumerReceivedObservable =>
            Observable.FromEventPattern<BasicDeliverEventArgs>(_consumer, nameof(_consumer.Received)).Select(e => e.EventArgs);
            

        protected virtual void SetupEnvironment()
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
                Parameters.Source.ExchangeArgs.ExchangeName,
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

        public IDisposable Connect()
        {
            var connection = _privateConnectableSequence.Connect();
            StartConsumer();
            return connection;
        }
    }
}