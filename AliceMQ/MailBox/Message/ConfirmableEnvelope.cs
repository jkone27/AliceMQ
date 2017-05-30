using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox.DeliveryArgs;
using AliceMQ.MailBox.Interface;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Message
{
    public class ConfirmableEnvelope<T> : IConfirmableDelivery, IConfirmableEnvelope<T>
    {
        private readonly ISubject<AcceptDeliveryArgs> _ackSubject;
        private readonly ISubject<RejectDeliveryArgs> _nackSubject;
        private readonly IMessage _content;
        public IObservable<AcceptDeliveryArgs> AcksRequests => Observable.Defer(() => _ackSubject.AsObservable());
        public IObservable<RejectDeliveryArgs> NacksRequests => Observable.Defer(() => _nackSubject.AsObservable());
        public bool Confirmed { get; private set; }

        public ConfirmableEnvelope(IMessage content)
        {
            _content = content;
            Confirmed = false;
            _ackSubject = new Subject<AcceptDeliveryArgs>();
            _nackSubject = new Subject<RejectDeliveryArgs>();
        }

        public bool IsOk() => _content.IsOk<T>();

        public bool IsError() => _content.IsError();

        public T Get() => _content.AsOk<T>().Message;
        public Exception GetException() => _content.AsError().Ex;

        public void Accept(bool multiple = false)
        {
            CheckConfirmed();
            _ackSubject.OnNext(new AcceptDeliveryArgs(RawData.DeliveryTag, multiple));
        }

        private void CheckConfirmed()
        {
            if (Confirmed)
                throw new AlreadyConfirmedMessageException();
        }

        public void ConfirmDelivery()
        {
            Confirmed = true;
        }

        public void Reject(bool multiple = false, bool requeue = false)
        {
            CheckConfirmed();
            _nackSubject.OnNext(new RejectDeliveryArgs(RawData.DeliveryTag, multiple, requeue));
        }

        public BasicDeliverEventArgs RawData => _content.RawData;
    }
}