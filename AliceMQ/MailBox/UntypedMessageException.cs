using System;

namespace AliceMQ.MailBox
{
    public class UntypedMessageException : Exception
    {
        public override string Message { get; }
        public UntypedMessageException(string message)
        {
            Message = message;
        }
    }
}