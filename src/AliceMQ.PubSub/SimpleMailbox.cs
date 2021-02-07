using AliceMQ.Core;
using AliceMQ.Core.Message;
using AliceMQ.Core.Types;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using System;

namespace AliceMQ.PubSub
{
    public class SimpleMailbox : ISimpleMailbox
    {
        public string ConnectionUrl { get; }
        public string QueueName => Sink.Source.QueueArgs.QueueName;
        public string ExchangeName => Sink.Source.Exchange.ExchangeName;
        public string DeadLetterExchangeName => Sink.DeadLetterExchangeName;

        public ISink Sink { get; }

        public string ProjectId { get; }

        public SimpleMailbox(string projectId, string connectionUrl, Sink sink)
        {
            ProjectId = projectId;
            ConnectionUrl = connectionUrl;
            Sink = sink;
        }


        // note: do not call dispose on this for now, it's a "fake" disposable
        public IDisposable Subscribe(IObserver<IDeliveryContext> observer)
        {
            SubscriberServiceApiClient subscriberService = new SubscriberServiceApiClientBuilder
            {
                Endpoint = ConnectionUrl,
                ChannelCredentials = ChannelCredentials.Insecure
            }.Build();

            SubscriptionName subscriptionName = new SubscriptionName(ProjectId, QueueName);

            try
            {
                var subscription = subscriberService.CreateSubscription(QueueName, ExchangeName, pushConfig: null, ackDeadlineSeconds: 60);
            }
            catch
            {
                //already exists
            }

            var subscriber = SubscriberClient.CreateAsync(subscriptionName,
                new SubscriberClient.ClientCreationSettings(null, null, ChannelCredentials.Insecure, ConnectionUrl))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            var subscriberTask = subscriber.StartAsync(async (msg, cancellationToken) =>
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await subscriber.StopAsync(cancellationToken);
                        observer.OnCompleted();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    var context = new DeliveryContext(msg, subscriber);

                    //TODO: mutation not working
                    observer.OnNext(context);

                    // Return Reply.Ack to indicate this message has been handled.
                    return context.Reply;

                }
                catch(Exception ex)
                {
                    observer.OnError(ex);
                    return SubscriberClient.Reply.Nack;
                }
            });

            return null;
        }
    }
}
