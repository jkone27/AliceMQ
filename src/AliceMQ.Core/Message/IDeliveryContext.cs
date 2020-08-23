using System.Text;

namespace AliceMQ.Core.Message
{
    public interface IDeliveryContext
    {
        Encoding Encoding { get; }
        string Payload { get; }
        void Ack(bool multiple);
        void Nack(bool multiple, bool requeue);
    }
}