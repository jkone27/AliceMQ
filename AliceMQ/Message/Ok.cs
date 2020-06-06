using AliceMQ.MailBox.Core;

namespace AliceMQ.MailBox.Message
{
    public class Ok<T> : IContext
    {
        public Ok(T message, IContext context)
        {
            Message = message;
            _context = context;
        }
        public T Message { get; }

        private readonly IContext _context;

        public IDeliveryContext DeliveryContext => _context.DeliveryContext;

        public void Confirm()
        {
            _context.Confirm();
        }

        public void Reject()
        {
            _context.Reject();
        }
    }
}