using System;
using Alice.MailBox.Core;

namespace Alice.MailBox.Message
{
    public class Error : Failure<IMailboxContext>
    {
        private readonly bool _multiple;
        private readonly bool _requeue;

        public Error(IMailboxContext data, Exception ex, bool multiple, bool requeue) : base(data, ex)
        {
            _multiple = multiple;
            _requeue = requeue;
        }

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