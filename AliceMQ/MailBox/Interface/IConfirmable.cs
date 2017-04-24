using System;

namespace AliceMQ.MailBox.Interface
{
    public interface IConfirmable<out T> : IObservable<T>, IAckableConsumer
    {
        bool IsConfirmable { get; }
    }
}
