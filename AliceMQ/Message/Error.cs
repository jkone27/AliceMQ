using System;
using AliceMQ.MailBox.Core;

namespace AliceMQ.MailBox.Message
{
    public class Error : IContext
    {

        public Error(Exception ex, IContext context)
        {
            Ex = ex;
            _context = context;
        }

        public Exception Ex { get; }

        private readonly IContext _context;

        public IMailboxContext MbContext => _context.DeliveryContext;

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