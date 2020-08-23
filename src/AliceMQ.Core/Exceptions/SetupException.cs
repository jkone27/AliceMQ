using System;

namespace AliceMQ.Core.Exceptions
{
    public class SetupException : Exception
    {
        public SetupException(string message)
            : base(message) { }

        public SetupException(Exception innerException)
            : base($"{innerException.Message}\r\n{innerException.StackTrace}") { }
    }
}