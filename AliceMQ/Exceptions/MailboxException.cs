using System;

namespace AliceMQ.MailBox
{
    public class MailboxException : Exception
    {
        public MailboxException(string message)
            : base(message) { }

        public MailboxException(Exception innerException)
            : base($"{innerException.Message}\r\n{innerException.StackTrace}") { }
    }
}