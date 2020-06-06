using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubSubTest
{
    class Program
    {
        //docker run --rm -ti -p 8681:8681 -e PUBSUB_PROJECT1=test-proj,topic1:subscription1 messagebird/gcloud-pubsub-emulator:latest
        const string projectId = "test-proj";
        const string topicId = "topic1";
        const string subscriptionId = "subscription1";

        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8681");
            var emulatorHostAndPort = Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");
            var pubsubProjectId = Environment.GetEnvironmentVariable("PUBSUB_PROJECT_ID");
            //Environment.SetEnvironmentVariable("GCP_SERVICE_ACCOUNT_JSON", "");
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "");

            // First create a topic.
            //PublisherServiceApiClient publisherService = await PublisherServiceApiClient.CreateAsync();

            PublisherServiceApiClient publisherService = new PublisherServiceApiClientBuilder
            {
                Endpoint = emulatorHostAndPort,
                ChannelCredentials = ChannelCredentials.Insecure
            }.Build();


            TopicName topicName = new TopicName(projectId, topicId);

            try
            {
                publisherService.CreateTopic(topicName, CallSettings.FromCancellationToken(default));
            }
            catch
            {
                //already exists
            }

            SubscriberServiceApiClient subscriberService = new SubscriberServiceApiClientBuilder
            {
                Endpoint = emulatorHostAndPort,
                ChannelCredentials = ChannelCredentials.Insecure
            }.Build();


            // Subscribe to the topic.
            //SubscriberServiceApiClient subscriberService = await SubscriberServiceApiClient.CreateAsync();
            
            SubscriptionName subscriptionName = new SubscriptionName(projectId, subscriptionId);

            try
            {
                subscriberService.CreateSubscription(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);
            }
            catch
            {
                //already exists
            }            

            // Publish a message to the topic using PublisherClient.
            PublisherClient publisher = await PublisherClient.CreateAsync(topicName, 
                new PublisherClient.ClientCreationSettings(null, null, ChannelCredentials.Insecure, emulatorHostAndPort));

            // PublishAsync() has various overloads. Here we're using the string overload.
            string messageId = await publisher.PublishAsync("Hello, Pubsub");

            // PublisherClient instance should be shutdown after use.
            // The TimeSpan specifies for how long to attempt to publish locally queued messages.
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));

            // Pull messages from the subscription using SubscriberClient.
            SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName,
                new SubscriberClient.ClientCreationSettings(null, null, ChannelCredentials.Insecure, emulatorHostAndPort));

            List<PubsubMessage> receivedMessages = new List<PubsubMessage>();
            // Start the subscriber listening for messages.

            await subscriber.StartAsync((msg, cancellationToken) =>
            {
                receivedMessages.Add(msg);
                
                Console.WriteLine($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
                
                Console.WriteLine($"Text: '{msg.Data.ToStringUtf8()}'");
                
                // Stop this subscriber after one message is received.
                // This is non-blocking, and the returned Task may be awaited.
                subscriber.StopAsync(TimeSpan.FromSeconds(15));
                
                // Return Reply.Ack to indicate this message has been handled.
                return Task.FromResult(SubscriberClient.Reply.Ack);
            });

            // Tidy up by deleting the subscription and the topic.
            subscriberService.DeleteSubscription(subscriptionName, CallSettings.FromCancellationToken(default));

            publisherService.DeleteTopic(topicName, CallSettings.FromCancellationToken(default));

        }
    }
}
