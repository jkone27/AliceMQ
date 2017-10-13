using System;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core.Simple
{
    public class AutoMailBox: IAutoMailBox<BasicDeliverEventArgs>
    {
        private readonly MailBoxBase _mailBox;

        public AutoMailBox(EndPoint simpleEndpoint,
            Sink sink)
        {
            _mailBox = new MailBoxBase(simpleEndpoint, sink, true);
        }

        public IDisposable Subscribe(IObserver<BasicDeliverEventArgs> observer)
        {
            return _mailBox.Subscribe(observer);
        }

        public IDisposable Connect()
        {
            return _mailBox.Connect();
        }

        public void Dispose()
        {
            _mailBox.Dispose();
        }
    }
}