using System;
using System.Reactive.Disposables;
using Alice.Core.Types;
using RabbitMQ.Client;

namespace AliceMQ.Rabbit.MailBox
{
    public sealed class MailboxChannel : IMailboxChannel
    {
        private readonly IConnectionFactory _factory;
        private readonly BasicQualityOfService _basicQualityOfService;
        //factory, endpoint, connect

        public MailboxChannel(EndPoint endPoint, BasicQualityOfService basicQualityOfService)
        {
            _factory = new ConnectionFactory
            {
                Uri = new Uri(endPoint.ConnectionUrl),
                AutomaticRecoveryEnabled = endPoint.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = endPoint.NetworkRecoveryInterval
            };
            _basicQualityOfService = basicQualityOfService;
        }

        public MailboxConnection Connect()
        {
            var compositeDisposable = new CompositeDisposable();
            var connection = _factory.CreateConnection();
            compositeDisposable.Add(connection);
            var channel = ChannelSetup(connection, compositeDisposable);
            return new MailboxConnection(channel, compositeDisposable);
        }

        private IModel ChannelSetup(IConnection connection, CompositeDisposable disposableBag)
        {
            var channel = connection.CreateModel();
            disposableBag.Add(channel);
            channel.BasicQos(0, _basicQualityOfService.PrefetchCount, _basicQualityOfService.Global);
            //prefetchSize!=0 not implemented - https://www.rabbitmq.com/specification.html
            return channel;
        }
    }
}