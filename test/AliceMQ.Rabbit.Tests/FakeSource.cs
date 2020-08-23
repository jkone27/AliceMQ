using AliceMQ.Core.Types;

namespace Tests
{
    public class FakeSource : ISource
    {
        public IExchange Exchange { get; set; }

        public IQueueArgs QueueArgs { get; set; }
    }

}