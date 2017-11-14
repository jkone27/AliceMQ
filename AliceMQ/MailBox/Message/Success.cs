namespace AliceMQ.MailBox.Message
{
    public abstract class Success<T> : IMessage
    {
        protected Success(T context)
        {
            Context = context;
        }

        protected T Context { get; }
        public abstract void Confirm();
        public abstract void Reject();
    }
}