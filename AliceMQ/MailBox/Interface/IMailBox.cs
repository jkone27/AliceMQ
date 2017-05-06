using System;

namespace AliceMQ.MailBox.Interface
{
    public interface IMailBox<out T> : IObservable<T>, IAckableConsumer
    {
        bool IsConfirmable { get; }
    }

    public interface ISafeObservable<out T> : IObservable<IResult<T>>
    {
    }
}
