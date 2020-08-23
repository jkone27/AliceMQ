using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using AliceMQ.Core.Message;
using AliceMQ.Rabbit.MailBox;
using Microsoft.Reactive.Testing;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace Tests
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
                .Select(e => new DeliveryContext(e.EventArgs, null));

            var c = new FakeSimpleMailbox(src, autoAck: true);
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

            var src = new Subject<IDeliveryContext>();
            src.OnError(new Exception("ex"));

            var c = new FakeSimpleMailbox(src, autoAck: true);
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
            var src = new Subject<IDeliveryContext>();
            src.OnCompleted();

            var c = new FakeSimpleMailbox(src, autoAck: true);
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
                .Select(z => new DeliveryContext(NewArgs(), null));

            var c = new FakeMailbox<string>(new FakeSimpleMailbox(src, autoAck: true), str => str);

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
                .Select(z => new DeliveryContext(NewArgs(WrongPayload), null));

            var c = new FakeMailbox<TestMessage>(new FakeSimpleMailbox(src, autoAck: true), JsonConvert.DeserializeObject<TestMessage>);
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
                .Select(z => new DeliveryContext(NewArgs(), channel.Object));

            var mb = new FakeSimpleMailbox(src);
            var custom = new FakeMailbox<string>(mb, str => str);

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
                .Select(z => new DeliveryContext(NewArgs(), channel.Object));

            var mb = new FakeSimpleMailbox(src);
            var custom = new FakeMailbox<string>(mb, str => str);

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
                .Select(z => new DeliveryContext(NewArgs(), channel.Object));

            var mb = new FakeSimpleMailbox(src);
            var typed = new FakeMailbox<string>(mb, str => str);

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
                .Select(z => new DeliveryContext(NewArgs(WrongPayload), channel.Object));

            var mb = new FakeSimpleMailbox(src);
            var typed = new FakeMailbox<TestMessage>(mb, JsonConvert.DeserializeObject<TestMessage>);

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
}
