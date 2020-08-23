using System.Text;
using AliceMQ.Core.Message;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.Rabbit.MailBox
{
    public class DeliveryContext : IDeliveryContext
    {
        public BasicDeliverEventArgs EventArgs { get; }
        public IModel Channel { get; }
        public ulong DeliveryTag { get; }

        public DeliveryContext(BasicDeliverEventArgs eventArgs, IModel channel)
        {
            EventArgs = eventArgs;
            Channel = channel;
            DeliveryTag = EventArgs.DeliveryTag;
        }

        public Encoding Encoding => Encoding.GetEncoding(EventArgs.BasicProperties.ContentEncoding ?? "utf-8");

        public string Payload => Encoding.GetString(EventArgs.Body.ToArray());

        public void Ack(bool multiple) => Channel.BasicAck(DeliveryTag, multiple);

        public void Nack(bool multiple, bool requeue) => Channel.BasicNack(DeliveryTag, multiple, requeue);
    }
}