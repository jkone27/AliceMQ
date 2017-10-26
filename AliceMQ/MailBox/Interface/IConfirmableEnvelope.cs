using System;
using AliceMQ.MailBox.Core;

namespace AliceMQ.MailBox.Interface
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