using System;

namespace AliceMQ.MailMan
{
    public class MailmanSetupException : Exception
    {
        public MailmanSetupException(string message)
            : base(message) { }

        public MailmanSetupException(Exception innerException)
            : base($"{innerException.Message}\r\n{innerException.StackTrace}") { }
    }
}