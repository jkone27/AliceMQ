using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Interface
{

    public interface IMessage : IResult<BasicDeliverEventArgs>
    {
    }
}