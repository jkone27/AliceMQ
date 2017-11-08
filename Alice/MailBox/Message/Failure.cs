using System;
using Alice.MailBox.Interface;

namespace Alice.MailBox.Message
{
    public abstract class Failure<T> : IMessage
    {
        protected Failure(T data, Exception ex)
        {
            Context = data;
            Ex = ex;
        }

        public Exception Ex { get; }
        protected T Context { get; }
        public abstract void Confirm();
        public abstract void Reject();
    }
}