using System;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Interface
{
    public interface IConfirmableEnvelope<out T> : IConfirmableMessage
    {
        bool IsOk();
        bool IsError();
        T Content();
        Exception Exception();
        BasicDeliverEventArgs RawData { get; }
    }
}