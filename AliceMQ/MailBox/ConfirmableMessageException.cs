using System;
using AliceMQ.MailBox.Message;

namespace AliceMQ.MailBox
{
    public class ConfirmableMessageException<T> : Exception
    {
        public ConfirmableMessage<T> ConfirmableMessage { get; }

        public ConfirmableMessageException(ConfirmableMessage<T> confirmableMessage, string message) : base(message)
        {
            ConfirmableMessage = confirmableMessage;
        }
    }
}