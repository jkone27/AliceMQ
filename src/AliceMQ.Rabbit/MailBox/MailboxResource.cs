using System.Reactive.Disposables;
using RabbitMQ.Client;

namespace AliceMQ.Rabbit.MailBox
{
    public class MailboxConnection
    {
        public MailboxConnection(IModel channel, CompositeDisposable disposables)
        {
            Channel = channel;
            Disposables = disposables;
        }
        public IModel Channel { get;}
        public CompositeDisposable Disposables {get; } // .add for each subscription
    }
}