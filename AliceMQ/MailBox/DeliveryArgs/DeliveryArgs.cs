namespace AliceMQ.MailBox.DeliveryArgs
{
    public abstract class DeliveryArgs
    {
        public ulong DeliveryTag { get; }

        protected DeliveryArgs(ulong deliveryTag)
        {
            DeliveryTag = deliveryTag;
        }
    }
}