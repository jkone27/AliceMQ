﻿using Alice.Core;
using Alice.Core.Message;
using Alice.Core.Types;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace AliceMQ.GooglePubSub
{
    public class GoogleMailbox : ISimpleMailbox
    {
        public string ConnectionUrl { get; }
        public string QueueName => Sink.Source.QueueArgs.QueueName;
        public string ExchangeName => Sink.Source.Exchange.ExchangeName;
        public string DeadLetterExchangeName => Sink.DeadLetterExchangeName;

        public ISink Sink { get; }

        public string ProjectId { get; }

        public GoogleMailbox(string projectId, string connectionUrl, GoogleSink sink)
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
                subscriberService.CreateSubscription(QueueName, ExchangeName, pushConfig: null, ackDeadlineSeconds: 60);
            }
            catch
            {
                //already exists
            }

            var subscriber = SubscriberClient.CreateAsync(subscriptionName,
                new SubscriberClient.ClientCreationSettings(null, null, ChannelCredentials.Insecure, ConnectionUrl))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            var subscriberTask = subscriber.StartAsync((msg, cancellationToken) =>
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        subscriber.StopAsync(cancellationToken);
                        observer.OnCompleted();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    var context = new GoogleMailboxContext(msg, subscriber);
                    observer.OnNext(context);

                    // Return Reply.Ack to indicate this message has been handled.
                    return Task.FromResult(context.Reply);

                }
                catch(Exception ex)
                {
                    observer.OnError(ex);
                    return Task.FromResult(SubscriberClient.Reply.Nack);
                }
            });

            return null;
        }
    }
}
