namespace Alice.Core.Message
{
    public interface IContext : IMessage
    {
        IDeliveryContext DeliveryContext { get; }
    }
}