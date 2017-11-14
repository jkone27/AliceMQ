using AliceMQ.MailBox.Core;

namespace AliceMQ.MailBox.Message
{
    public class Ok<T> : Success<IMailboxContext>
    {
        private readonly bool _multiple;
        private readonly bool _requeue;

        public Ok(T message, IMailboxContext context, bool multiple, bool requeue) : base(context)
        {
            _multiple = multiple;
            _requeue = requeue;
            Message = message;
        }
        public T Message { get; }
        public override void Confirm()
        {
            Context.Channel.BasicAck(Context.EventArgs.DeliveryTag, _multiple);
        }

        public override void Reject()
        {
            Context.Channel.BasicNack(Context.EventArgs.DeliveryTag, _multiple, _requeue);
        }
    }
}