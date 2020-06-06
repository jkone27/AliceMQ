namespace Alice.Core.Types
{
    public class ConfirmationPolicy
    {
        public bool AutoAck { get; set; }
        public bool Multiple { get; set; }
        public bool Requeue { get; set; }
    }
}