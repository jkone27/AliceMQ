using System;
using RabbitMQ.Client.Events;

namespace Tests
{
    public class EventTest
    {
        public event EventHandler<BasicDeliverEventArgs> FakeEvent;

        public void Raise(BasicDeliverEventArgs e)
        {
            var invoked = FakeEvent;
            invoked?.Invoke(this, e);
        }
    }
}
