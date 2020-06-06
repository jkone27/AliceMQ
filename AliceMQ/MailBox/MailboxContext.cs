using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public class MailboxContext : IMailboxContext
    {
        public BasicDeliverEventArgs EventArgs { get; set; }
        public IModel Channel { get; set; }

        public Encoding Encoding => throw new System.NotImplementedException();

        public string Payload => throw new System.NotImplementedException();

        public void Ack(bool multiple)
        {
            throw new System.NotImplementedException();
        }

        public void Nack(bool multiple, bool requeue)
        {
            throw new System.NotImplementedException();
        }
    }
}