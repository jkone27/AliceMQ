using Alice.Core.Types;
using System;

namespace AliceMQ.GooglePubSub
{
    public class GoogleSource : ISource
    {
        public IExchange Exchange { get; }
        public IQueueArgs QueueArgs { get; }
        public string ProjectId { get; }

        public GoogleSource(string topicName, string projectId, string subscriptionId)
        {
            Exchange = new GoogleTopic(topicName);
            QueueArgs = new QueueArgs(subscriptionId);
            ProjectId = projectId;
        }
    }
}
