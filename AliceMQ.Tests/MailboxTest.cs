using System;
using System.Reactive.Linq;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.Message;
using RabbitMQ.Client.Events;
using Microsoft.Reactive.Testing;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace AliceMQ.Tests
{
    public class MailboxTest
    {
        public BasicDeliverEventArgs NewArgs()
            =>
            new BasicDeliverEventArgs(
                It.IsAny<string>(), 
                It.IsAny<ulong>(), 
                It.IsAny<bool>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(),
                new Mock<IBasicProperties>().Object, 
                new byte[] {0});

        [Theory,
            InlineData(true),
            InlineData(false)]
        public void Mailbox_ReceiveOne(bool autoAck)
        {
            var received = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var c = new FakeConsumer(src, autoAck);
            c.Subscribe(m => received = true);
           
            s.AdvanceBy(1);
            Assert.True(received);
        }

        [Theory,
        InlineData(true),
        InlineData(false)]
        public void CustomMailbox_ReceiveOne(bool autoAck)
        {
            var received = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var mb = new FakeConsumer(src, autoAck);
            var c = new CustomMailBox<string>(mb);
            c.Subscribe(m => received = true);
            
            s.AdvanceBy(1);
            Assert.True(received);
        }

        public class TestClass
        {
            public string Test { get; set; }
        }

        [Theory,
            InlineData(true),
            InlineData(false)]
        public void CustomMailbox_ReceiveOne_UnableToDeserialize(bool autoAck)
        {
            string received = null;
            Exception ex = null;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var mb = new FakeConsumer(src, autoAck);
            var c = new CustomMailBox<TestClass>(mb);
            c.Subscribe(m => received = m.Datum.Test, e => ex = e);

            s.AdvanceBy(1);
            Assert.True(received == null);
            Assert.True(ex != null);
            Assert.ThrowsAny<UntypedMessageException>(() => { throw ex; });
        }

        [Fact]
        public void ConfirmableMailbox_AutoAckTrue_ThrowsException()
        {
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());
            var mb = new FakeConsumer(src, true);
            var custom = new CustomMailBox<string>(mb);
            Assert.ThrowsAny<MailboxException>(() => new ConfirmableMailbox<string>(custom));
        }

        [Fact]
        public void ConfirmableMailbox_ReceiveOne_andAck()
        {
            var received = false;
            bool ackRequested = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var mb = new FakeConsumer(src, false);
            var custom = new CustomMailBox<string>(mb);
            var c = new ConfirmableMailbox<string>(custom);

            c.Subscribe(m =>
                { received = true;
                    m.AcksRequests.Subscribe(next => ackRequested = true);
                    m.Accept();
                });

            s.AdvanceBy(1);
            Assert.True(received);
            Assert.True(ackRequested);
            Assert.True(mb.Acks == 1);
        }

        [Fact]
        public void ConfirmableMailbox_ReceiveOne_andNack()
        {
            var received = false;
            bool nackRequested = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var mb = new FakeConsumer(src, false);
            var typed = new CustomMailBox<string>(mb);
            var c = new ConfirmableMailbox<string>(typed);

            c.Subscribe(m =>
            {
                received = true;
                m.NacksRequests.Subscribe(next => nackRequested = true);
                m.Reject();
            });

            s.AdvanceBy(1);
            Assert.True(received);
            Assert.True(nackRequested);
            Assert.True(mb.Nacks == 1);
        }

        [Fact]
        public void ConfirmableMailbox_ReceiveOne_SubscribeTwo_AckingTwiceRaisesException()
        {
            var received = false;
            bool ackRequested = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(2), s)
                .Select(z => NewArgs());

            var mb = new FakeConsumer(src, false);
            var typed = new CustomMailBox<string>(mb);
            var c = new ConfirmableMailbox<string>(typed);

            c.Subscribe(m =>
            {
                received = true;
                m.AcksRequests.Subscribe(next => ackRequested = true);
                m.Accept();//first observer to ack
            });

            //https://social.msdn.microsoft.com/Forums/en-US/ac721f91-4dbc-40b8-a2b2-19f00998239f/order-of-subscriptions-order-of-observations?forum=rx
            c.Subscribe(m =>
            {
                received = true;
                m.AcksRequests.Subscribe(next => ackRequested = true);
                //no need to delay as order of subscription is same as order of observation.
                Assert.ThrowsAny<AlreadyConfirmedMessageException>(() => m.Accept());
            });

            s.AdvanceBy(2);
            Assert.True(received);
            Assert.True(ackRequested);
            Assert.True(mb.Acks == 1);
        }
    }
}
