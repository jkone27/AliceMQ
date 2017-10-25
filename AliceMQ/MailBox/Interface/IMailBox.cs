using System;
using System.Reactive.Subjects;

namespace AliceMQ.MailBox.Interface
{
    
    public interface IConfirmableMailBox<out T> : IConnectableObservable<T>, IDisposable
    {
    }

    public interface IMailBox<out T> : IObservable<T>
    {
    }

    public interface IAckableMailbox<out T> : IMailBox<T>, IAckableConsumer
    {
    }

    public interface IAutoMailBox<out T> : IMailBox<T>
    {
    }
}
