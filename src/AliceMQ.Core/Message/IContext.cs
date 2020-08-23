namespace AliceMQ.Core.Message
{
    public interface IContext : IMessage
    {
        IDeliveryContext DeliveryContext { get; }
    }
}