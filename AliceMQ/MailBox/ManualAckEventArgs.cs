using System;

namespace AliceMQ.MailBox
{
    public class ManualAckEventArgs<T> : EventArgs
    {
        public T MessageArgs;

        public ManualAckEventArgs(T messageArgs)
        {
            MessageArgs = messageArgs;
        }
    }
}