using System;

namespace AliceMQ.Core.Message
{
    public class Error : IContext
    {

        public Error(Exception ex, IContext context)
        {
            Ex = ex;
            _context = context;
        }

        public Exception Ex { get; }

        public IDeliveryContext DeliveryContext => _context.DeliveryContext;

        private readonly IContext _context;

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