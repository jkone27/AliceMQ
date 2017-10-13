using System;
using System.Reactive.Linq;
using System.Text;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.Core.Custom;
using RabbitMQ.Client.Events;
using Microsoft.Reactive.Testing;
using Moq;
using Newtonsoft.Json;
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

        [Fact]
        public void Mailbox_ReceiveOne()
        {
            var received = false;
            var testEvent = new EventTest();

            var src = Observable
                .FromEventPattern<BasicDeliverEventArgs>(testEvent, nameof(testEvent.FakeEvent))
                .Select(e => e.EventArgs);

            var c = new FakeAutoMailbox(src);
            c.Subscribe(m => 
                received = true);

            c.Connect();

            testEvent.Raise(NewArgs());

            Assert.True(received);
        }

        [Fact]
        public void CustomMailbox_ReceiveOne()
        {
            var received = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var c = new AutoCustomMailBox<string>(new FakeAutoMailbox(src), str => str);

            c.Subscribe(m => received = true);
            c.Connect();
            s.AdvanceBy(1);
            Assert.True(received);
        }

        [Fact]
        public void CustomMailbox_ReceiveOne_UnableToDeserialize()
        {
            int? received = null;
            Exception ex = null;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs(WrongPayload));

            var mb = new FakeAutoMailbox(src);
            var c = new AutoCustomMailBox<TestMessage>(mb, JsonConvert.DeserializeObject<TestMessage>);
            c.Subscribe(m =>
            {
                if (m.IsOk<TestMessage>())
                    received = m.AsOk<TestMessage>().Message.Code;
                else
                {
                    ex = m.AsError().Ex;
                }
            });
            c.Connect();

            s.AdvanceBy(1);
            Assert.True(received == null);
            Assert.True(ex != null);
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

            var mb = new FakeMailbox(src);
            var c = new FakeCustomMailbox<TestMessage>(mb, JsonConvert.DeserializeObject<TestMessage>);
            var conf = new FakeConfirmableMailbox<TestMessage>(c);

            conf.Subscribe(m =>
                {
                    if(m.IsOk())
                        received = true;
                    else
                    {
                        ex = m.Exception();
                    }
                });
            conf.Connect();

            s.AdvanceBy(1);
            Assert.False(received);
            Assert.True(ex != null);
        }


        [Fact]
        public void ConfirmableMailbox_ReceiveOne_andAck()
        {
            var received = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var mb = new FakeMailbox(src);
            var custom = new FakeCustomMailbox<string>(mb, str => str);
            var c = new FakeConfirmableMailbox<TestMessage>(custom);

            c.Subscribe(m =>
                { received = true;
                    m.Accept();
                });

            c.Connect();
            s.AdvanceBy(1);
            Assert.True(received);
        }

        [Fact]
        public void ConfirmableMailbox_ReceiveMany_AcksThemAll()
        {
            var received = 0;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var mb = new FakeMailbox(src);
            var custom = new FakeCustomMailbox<string>(mb, str => str);
            var c = new FakeConfirmableMailbox<TestMessage>(custom);

            c.Subscribe(m =>
            {
                received++;
                m.Accept();
            });

            c.Connect();
            s.AdvanceBy(20);
            Assert.True(received == 20);
            Assert.True(mb.Acks == 20);
        }

        [Fact]
        public void ConfirmableMailbox_ReceiveOne_andNack()
        {
            var received = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs());

            var mb = new FakeMailbox(src);
            var typed = new FakeCustomMailbox<string>(mb, str => str);
            var c = new FakeConfirmableMailbox<TestMessage>(typed);

            c.Subscribe(m =>
            {
                received = true;
                m.Reject();
            });
            c.Connect();

            s.AdvanceBy(1);
            Assert.True(received);
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
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => NewArgs(WrongPayload));

            var mb = new FakeMailbox(src);
            var typed = new FakeCustomMailbox<TestMessage>(mb, JsonConvert.DeserializeObject<TestMessage>);
            var c = new FakeConfirmableMailbox<TestMessage>(typed);

            c.Subscribe(m =>
            {
                if (m.IsOk())
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
            c.Connect();

            s.AdvanceBy(1);
            Assert.False(received);
            Assert.True(exception);
            Assert.True((acks  && mb.Acks == 1) || (!acks && mb.Nacks == 1));
        }
    }

    public class TestMessage
    {
        public int Code { get; set; }
    }
}
