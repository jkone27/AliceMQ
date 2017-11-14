namespace AliceMQ.MailBox
{
    public class BasicQualityOfService
    {
        public BasicQualityOfService(ushort prefetchCount, bool global)
        {
            PrefetchCount = prefetchCount;
            Global = global;
        }

        public ushort PrefetchCount { get; set; }
        public bool Global { get; set; }
    }
}