using System;

namespace Alice.Core.Types
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