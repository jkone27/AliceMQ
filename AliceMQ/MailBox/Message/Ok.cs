using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Message
{
    public class Ok<T> : Success<BasicDeliverEventArgs>, IMessage
    {
        public Ok(T message, BasicDeliverEventArgs data) : base(data)
        {
            Message = message;
        }
        public T Message { get; }
    }
}