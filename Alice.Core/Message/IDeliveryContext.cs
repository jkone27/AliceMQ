using System.Text;

namespace Alice.Core.Message
{
    public interface IDeliveryContext
    {
        Encoding Encoding { get; }
        string Payload { get; }
        void Ack(bool multiple);
        void Nack(bool multiple, bool requeue);
    }
}