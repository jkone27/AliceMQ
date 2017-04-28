using System;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox
{
    public class CustomMailboxException : Exception
    {
        public override string Message { get; }
        public BasicDeliverEventArgs Metadata { get; }
        public CustomMailboxException(string message, BasicDeliverEventArgs metadata)
        {
            Message = message;
            Metadata = metadata;
        }
    }
}