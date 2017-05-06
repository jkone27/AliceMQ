using System;
using System.Reactive.Linq;
using System.Text;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using RabbitMQ.Client.Events;
using Microsoft.Reactive.Testing;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace AliceMQ.Tests
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

        public BasicDeliverEventArgs NewArgs(byte[] serializedObject)
           =>
           new BasicDeliverEventArgs(
               It.IsAny<string>(),
               It.IsAny<ulong>(),
               It.IsAny<bool>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               new Mock<IBasicProperties>().Object,
               serializedObject);

        [Theory,
            InlineData(true),
            InlineData(false)]
        public void Mailbox_ReceiveOne(bool autoAck)
        {
            var received = false;
            var testEvent = new EventTest();

            var src = Observable
                .FromEventPattern<BasicDeliverEventArgs>(testEvent, nameof(testEvent.FakeEvent))
                .Select(e => e.EventArgs);

            var c = new FakeConsumer(src, autoAck);
            c.Subscribe(m => 
                received = true);

            testEvent.Raise(NewArgs());

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

        [Theory,
            InlineData(true),
            InlineData(false)]
        public void CustomMailbox_ReceiveOne_UnableToDeserialize(bool autoAck)
        {
            int? received = null;
            Exception ex = null;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs(WrongPayload));

            var mb = new FakeConsumer(src, autoAck);
            var c = new CustomMailBox<TestMessage>(mb);
            c.Subscribe(m =>
            {
                if (m.IsOk<TestMessage>())
                    received = m.AsOk<TestMessage>().Message.Code;
                else
                {
                    ex = m.AsError().Ex;
                }
            });

            s.AdvanceBy(1);
            Assert.True(received == null);
            Assert.True(ex != null);
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
            Assert.ThrowsAny<MailboxException>(() => new ConfirmableMailbox(custom));
        }

        [Fact]
        public void ConfirmableMailbox_ReceiveOne_UnableToDeserialize()
        {
            var received = false;
            Exception ex = null;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs(WrongPayload));

            var mb = new FakeConsumer(src, false);
            var c = new CustomMailBox<TestMessage>(mb);
            var conf = new ConfirmableMailbox(c);

            conf.Subscribe(m =>
                {
                    if(m.Content.IsOk<TestMessage>())
                        received = true;
                    else
                    {
                        ex = m.Content.AsError().Ex;
                    }
                });

            s.AdvanceBy(1);
            Assert.False(received);
            Assert.True(ex != null);
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
            var c = new ConfirmableMailbox(custom);

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
            var c = new ConfirmableMailbox(typed);

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

        public byte[] WrongPayload => Encoding.UTF8.GetBytes("{ \"code\": \"this is a wrong code, should be int\" }");

        [Theory,
            InlineData(true),
            InlineData(false)]
        public void ConfirmableMailbox_SubscribeOnError_Acks(bool acks)
        {
            var received = false;
            var exception = false;
            var nackRequested = false;
            var ackRequested = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs(WrongPayload));

            var mb = new FakeConsumer(src, false);
            var typed = new CustomMailBox<TestMessage>(mb);
            var c = new ConfirmableMailbox(typed);

            c.Subscribe(m =>
            {
                m.NacksRequests.Subscribe(next => nackRequested = true);
                m.AcksRequests.Subscribe(next => ackRequested = true);
                if (m.Content.IsOk<TestMessage>())
                {
                    received = true;
                    m.Accept();
                }
                else
                {
                    exception = true;
                    if(acks)
                        m.Accept();
                    else
                        m.Reject();
                }  
            });

            s.AdvanceBy(1);
            Assert.False(received);
            Assert.True(exception);
            Assert.True((acks && ackRequested  && mb.Acks == 1) || (!acks && nackRequested && mb.Nacks == 1));
        }
    }

    public class TestMessage
    {
        public int Code { get; set; }
    }
}
