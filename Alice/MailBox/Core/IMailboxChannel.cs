using System.Reactive.Disposables;
using RabbitMQ.Client;

namespace AliceMQ.MailBox.Core
{
    public interface IMailboxChannel
    {
        void Connect(out IModel channel, out CompositeDisposable compositeDisposable);
    }
}