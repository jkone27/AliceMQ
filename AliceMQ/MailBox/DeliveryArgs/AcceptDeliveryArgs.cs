namespace AliceMQ.MailBox.DeliveryArgs
{
    public class AcceptDeliveryArgs : DeliveryArgs
    {
        public bool Multiple { get; }

        public AcceptDeliveryArgs(ulong deliveryTag, bool multiple)
            : base(deliveryTag)
        {
            Multiple = multiple;
        }
    }
}