namespace AliceMQ.MailBox.Core
{
    public interface IDeliveryContext
    {
        System.Text.Encoding Encoding { get; }
        string Payload { get; }
        void Ack(bool multiple);
        void Nack(bool multiple, bool requeue);
    }
}