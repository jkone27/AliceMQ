using AliceMQ.MailBox.Core;

namespace AliceMQ.MailBox.Message
{
    public interface IContext : IMessage
    {
        IDeliveryContext DeliveryContext { get; }
    }
}