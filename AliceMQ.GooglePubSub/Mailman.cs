using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AliceMQ.GooglePubSub
{
    public class Mailman
    {
        private readonly string endpoint;
        private readonly string projectId;
        private readonly string topicId;
        private readonly Func<object, string> serializer;

        public Mailman(string endpoint, string projectId, string topicId, Func<object,string> serializer)
        {
            this.endpoint = endpoint;
            this.projectId = projectId;
            this.topicId = topicId;
            this.serializer = serializer;
        }

        public async Task<string> PublishOneAsync<T>(T message, string withSubscriptionId = null, CancellationToken cancellationToken = default)
        {
            //PublisherServiceApiClient publisherService = await PublisherServiceApiClient.CreateAsync
            PublisherServiceApiClient publisherService = new PublisherServiceApiClientBuilder
            {
                Endpoint = endpoint,
                ChannelCredentials = ChannelCredentials.Insecure
            }.Build();

            TopicName topicName = new TopicName(projectId, topicId);

            try
            {
                publisherService.CreateTopic(topicName, CallSettings.FromCancellationToken(cancellationToken));
            }
            catch
            {
                //already exists
            }

            if (!string.IsNullOrWhiteSpace(withSubscriptionId))
            {
                //SubscriberServiceApiClient subscriberService = await SubscriberServiceApiClient.CreateAsync();
                SubscriberServiceApiClient subscriberService = new SubscriberServiceApiClientBuilder
                {
                    Endpoint = endpoint,
                    ChannelCredentials = ChannelCredentials.Insecure
                }.Build();

                SubscriptionName subscriptionName = new SubscriptionName(projectId, withSubscriptionId);

                try
                {
                    subscriberService.CreateSubscription(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);
                }
                catch
                {
                    //already exists
                }
            }

            return await PublishAsync(topicName, endpoint, message);
        }

        private async Task<string> PublishAsync<T>(TopicName topicName, string emulatorHostAndPort, T message)
        {

            PublisherClient publisher = await PublisherClient.CreateAsync(topicName,
               new PublisherClient.ClientCreationSettings(null, null, ChannelCredentials.Insecure, emulatorHostAndPort));

            // PublishAsync() has various overloads. Here we're using the string overload.
            string messageId = await publisher.PublishAsync(serializer(message), Encoding.UTF8);

            // PublisherClient instance should be shutdown after use.
            // The TimeSpan specifies for how long to attempt to publish locally queued messages.
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15)); //?

            return messageId;
        }
    }
}
