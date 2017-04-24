namespace AliceMQ.MailBox.DeliveryArgs
{
    public abstract class DeliveryArgs
    {
        public ulong DeliveryTag { get; private set; }

        protected DeliveryArgs(ulong deliveryTag)
        {
            DeliveryTag = deliveryTag;
        }
    }
}