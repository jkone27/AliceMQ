using System;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.EndPointArgs;

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