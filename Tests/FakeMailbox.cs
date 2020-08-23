using System;
using AliceMQ.Core;
using AliceMQ.Core.Types;
using AliceMQ.Rabbit.MailBox;

namespace Tests
{
    public class FakeMailbox<T> : Mailbox<T>
    {
        protected FakeMailbox(EndPoint simpleEndPoint, Sink sink, Func<string, T> deserializer) 
            : base(simpleEndPoint, sink, deserializer)
        {
            throw new NotImplementedException();
        }

        public FakeMailbox(ISimpleMailbox simpleMailbox, Func<string, T> deserializer) 
            : base(simpleMailbox, deserializer)
        {
        }
    }
}