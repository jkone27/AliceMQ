﻿using AliceMQ.Core.Types;
using System;

namespace AliceMQ.PubSub
{
    public class Source : ISource
    {
        public IExchange Exchange { get; }
        public IQueueArgs QueueArgs { get; }
        public string ProjectId { get; }

        public Source(string topicName, string projectId, string subscriptionId)
        {
            Exchange = new Exchange(topicName);
            QueueArgs = new QueueArgs(subscriptionId);
            ProjectId = projectId;
        }
    }
}
