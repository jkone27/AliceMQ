namespace AliceMQ.Core.Message
{

    public sealed class Context : IContext
    {
        private readonly bool _multiple;
        private readonly bool _requeue;

        public Context(IDeliveryContext deliveryContext, bool multiple, bool requeue)
        {
            DeliveryContext = deliveryContext;
            _multiple = multiple;
            _requeue = requeue;
        }

        public IDeliveryContext DeliveryContext { get; }

        public void Confirm()
        {
            DeliveryContext.Ack( _multiple);
        }

        public void Reject()
        {
            DeliveryContext.Nack(_multiple, _requeue);
        }
    }
}