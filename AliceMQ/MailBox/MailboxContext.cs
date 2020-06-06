using System.Dynamic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public class MailboxContext : IDeliveryContext
    {
        public BasicDeliverEventArgs EventArgs { get; }
        public IModel Channel { get; }
        public ulong DeliveryTag { get; }

        public MailboxContext(BasicDeliverEventArgs eventArgs, IModel channel)
        {
            EventArgs = eventArgs;
            Channel = channel;
            DeliveryTag = EventArgs.DeliveryTag;
        }

        public Encoding Encoding => Encoding.GetEncoding(EventArgs.BasicProperties.ContentEncoding);

        public string Payload => Encoding.GetString(EventArgs.Body.ToArray());

        public void Ack(bool multiple) => Channel.BasicAck(DeliveryTag, multiple);

        public void Nack(bool multiple, bool requeue) => Channel.BasicNack(DeliveryTag, multiple, requeue);
    }
}