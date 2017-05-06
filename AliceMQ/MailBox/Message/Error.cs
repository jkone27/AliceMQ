using System;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Message
{
    public class Error : Failure<BasicDeliverEventArgs>, IMessage
    {
        public Error(BasicDeliverEventArgs data, Exception ex) : base(data, ex)
        {
        }
    }
}