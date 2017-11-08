using System;

namespace Alice.MailBox
{
    public class MailboxSetupException : MailboxException
    {
        public MailboxSetupException(string message)
            : base(message) { }

        public MailboxSetupException(Exception innerException)
            : base(innerException) { }
    }

}