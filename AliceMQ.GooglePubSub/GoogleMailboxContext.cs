﻿using Alice.Core.Message;
using Google.Cloud.PubSub.V1;
using System.Text;

namespace AliceMQ.GooglePubSub
{
    public class GoogleMailboxContext : IDeliveryContext
    {
        private readonly PubsubMessage message;
        private readonly SubscriberClient subscriberClient;

        public Encoding Encoding => Encoding.UTF8;
        public string Payload => message.Data.ToStringUtf8();

        public SubscriberClient.Reply Reply { get; private set; }

        public GoogleMailboxContext(PubsubMessage message, SubscriberClient subscriberClient)
        {
            this.message = message;
            this.subscriberClient = subscriberClient;
        }

        public void Ack(bool multiple)
        {
            Reply = SubscriberClient.Reply.Ack;
        }

        public void Nack(bool multiple, bool requeue)
        {
            Reply = SubscriberClient.Reply.Nack;
        }
    }
}
