using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AliceMQ.MailBox.DeliveryArgs;
using AliceMQ.MailBox.Interface;

namespace AliceMQ.MailBox.Message
{
    public class ConfirmableEnvelope : IConfirmableMessage, IConfirmableDelivery
    {
        private readonly ISubject<AcceptDeliveryArgs> _ackSubject;
        private readonly ISubject<RejectDeliveryArgs> _nackSubject;
        public IObservable<AcceptDeliveryArgs> AcksRequests => Observable.Defer(() => _ackSubject.AsObservable());
        public IObservable<RejectDeliveryArgs> NacksRequests => Observable.Defer(() => _nackSubject.AsObservable());
        public bool Confirmed { get; private set; }

        public ConfirmableEnvelope(IMessage content)
        {
            Content = content;
            Confirmed = false;
            _ackSubject = new Subject<AcceptDeliveryArgs>();
            _nackSubject = new Subject<RejectDeliveryArgs>();
        }

        public IMessage Content { get; }

        public void Accept(bool multiple = false)
        {
            CheckConfirmed();
            _ackSubject.OnNext(new AcceptDeliveryArgs(Content.RawData.DeliveryTag, multiple));
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
            _nackSubject.OnNext(new RejectDeliveryArgs(Content.RawData.DeliveryTag, multiple, requeue));
        }
    }
}