using System;
using AliceMQ.MailBox.Interface;

namespace AliceMQ.MailBox.Message
{
    public abstract class Failure<T> : IResult<T>
    {
        protected Failure(T data, Exception ex)
        {
            RawData = data;
            Ex = ex;
        }

        public Exception Ex { get; }
        public T RawData { get; }
    }
}