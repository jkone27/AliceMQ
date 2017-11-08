using System;
using Alice.MailBox.Core;

namespace Alice.MailBox.Interface
{
    public interface IConfirmableEnvelope<out T> : IConfirmableMessage
    {
        bool IsOk();
        bool IsError();
        T Content();
        Exception Exception();
        MailboxContext Context { get; }
    }
}