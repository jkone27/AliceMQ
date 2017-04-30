using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Message
{
    public class Message<T>
    {
        public Message(T datum, BasicDeliverEventArgs deliveryArgs)
        {
            Datum = datum;
            DeliveryArgs = deliveryArgs;
        }

        public T Datum { get; private set; }
        public BasicDeliverEventArgs DeliveryArgs { get; private set; }
    }
}