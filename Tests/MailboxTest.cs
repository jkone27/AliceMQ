using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Alice.ExtensionMethods;
using Alice.MailBox.Core;
using Microsoft.Reactive.Testing;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

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
                .Select(e => new MailboxContext{ Channel = null, EventArgs = e.EventArgs});

            var c = new FakeMailbox(src, autoAck: true);
            c.Subscribe(m => 
                received = true);

            testEvent.Raise(NewArgs());

            Assert.True(received);
        }

        [Fact]
        public void Mailbox_ReceiveOne_OnError()
        {
            var received = false;
            var onError = false;
            var s = new TestScheduler();

            var src = new Subject<IMailboxContext>();
            src.OnError(new Exception("ex"));

            var c = new FakeMailbox(src, autoAck: true);
            c.Subscribe(m =>
                received = true,
                e => onError = true
                );

            s.AdvanceBy(1);
            Assert.False(received);
            Assert.True(onError);
        }

        [Fact]
        public void Mailbox_ReceiveOne_OnComplete()
        {
            var received = false;
            var complete = false;
            var onError = false;

            var s = new TestScheduler();
            var src = new Subject<IMailboxContext>();
            src.OnCompleted();

            var c = new FakeMailbox(src, autoAck: true);
            c.Subscribe(m =>
                received = true,
                _ => {},
                () => complete = true);

            s.AdvanceBy(1);

            Assert.True(complete);
            Assert.False(onError);
            Assert.False(received);
        }

        [Fact]
        public void CustomMailbox_ReceiveOne()
        {
            var received = false;
            var s = new TestScheduler();
            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => new MailboxContext { Channel = null, EventArgs =  NewArgs() });

            var c = new FakeCustomMailbox<string>(new FakeMailbox(src, autoAck: true), str => str);

            c.Subscribe(m => received = true);
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
                .Select(z => new MailboxContext { Channel = null, EventArgs = NewArgs(WrongPayload)});

            var c = new FakeCustomMailbox<TestMessage>(new FakeMailbox(src, autoAck: true), JsonConvert.DeserializeObject<TestMessage>);
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
        public void CustomMailbox_ReceiveOne_andAck()
        {
            var received = false;
            var s = new TestScheduler();

            var channel = new Mock<IModel>();
            channel.Setup(x => x.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => new MailboxContext { Channel = channel.Object, EventArgs = NewArgs()});

            var mb = new FakeMailbox(src);
            var custom = new FakeCustomMailbox<string>(mb, str => str);

            custom.Subscribe(m =>
                { received = true;
                    m.Confirm();
                });

            s.AdvanceBy(1);
            Assert.True(received);
        }

        [Fact]
        public void CustomMailbox_ReceiveMany_AcksThemAll()
        {
            var received = 0;
            var s = new TestScheduler();

            var channel = new Mock<IModel>();
            channel.Setup(x => x.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => new MailboxContext { Channel = channel.Object, EventArgs = NewArgs()});

            var mb = new FakeMailbox(src);
            var custom = new FakeCustomMailbox<string>(mb, str => str);

            custom.Subscribe(m =>
            {
                received++;
                m.Confirm();
            });

            s.AdvanceBy(20);
            Assert.True(received == 20);
        }

        [Fact]
        public void CustomMailbox_ReceiveOne_andNack()
        {
            var received = false;
            var s = new TestScheduler();

            var channel = new Mock<IModel>();
            channel.Setup(x => x.BasicNack(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>()));

            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => new MailboxContext { Channel = channel.Object, EventArgs = NewArgs()});

            var mb = new FakeMailbox(src);
            var typed = new FakeCustomMailbox<string>(mb, str => str);

            typed.Subscribe(m =>
            {
                received = true;
                m.Reject();
            });

            s.AdvanceBy(1);
            Assert.True(received);
        }

        public byte[] WrongPayload => Encoding.UTF8.GetBytes("{ \"code\": \"this is a wrong code, should be int\" }");

        [Theory,
            InlineData(true),
            InlineData(false)]
        public void CustomMailbox_SubscribeOnError_Acks(bool acks)
        {
            var received = false;
            var exception = false;
            var s = new TestScheduler();

            var channel = new Mock<IModel>();
            channel.Setup(x => x.BasicNack(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>()));
            channel.Setup(x => x.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            var src = Observable
                .Interval(TimeSpan.FromTicks(1), s)
                .Select(z => new MailboxContext { Channel = channel.Object, EventArgs = NewArgs(WrongPayload)});

            var mb = new FakeMailbox(src);
            var typed = new FakeCustomMailbox<TestMessage>(mb, JsonConvert.DeserializeObject<TestMessage>);

            typed.Subscribe(m =>
            {
                if (m.IsOk<TestMessage>())
                {
                    received = true;
                    m.Confirm();
                }
                else
                {
                    exception = true;
                    if(acks)
                        m.Confirm();
                    else
                        m.Reject();
                }  
            });

            s.AdvanceBy(1);
            Assert.False(received);
            Assert.True(exception);
        }
    }

    public class TestMessage
    {
        public int Code { get; set; }
    }
}
