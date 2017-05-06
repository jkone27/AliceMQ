using AliceMQ.MailBox.Interface;

namespace AliceMQ.MailBox.Message
{
    public abstract class Success<T> : IResult<T>
    {
        protected Success(T data)
        {
            RawData = data;
        }

        public T RawData { get; }
    }
}