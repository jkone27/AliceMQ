using System;
using System.Reactive.Disposables;
using Alice.MailBox.EndPointArgs;
using RabbitMQ.Client;

namespace Alice.MailBox.Core
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

        public void Connect(out IModel channel, out CompositeDisposable compositeDisposable)
        {
            compositeDisposable = new CompositeDisposable();
            var connection = _factory.CreateConnection();
            compositeDisposable.Add(connection);
            channel = ChannelSetup(connection, compositeDisposable);
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