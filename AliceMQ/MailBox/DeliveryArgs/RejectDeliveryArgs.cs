namespace AliceMQ.MailBox.DeliveryArgs
{
    public class RejectDeliveryArgs : DeliveryArgs
    {
        public bool Multiple { get; }
        public bool Requeue { get; }

        public RejectDeliveryArgs(ulong deliveryTag, bool multiple, bool requeue)
            : base(deliveryTag)
        {
            Requeue = requeue;
            Multiple = multiple;
        }
    }
}