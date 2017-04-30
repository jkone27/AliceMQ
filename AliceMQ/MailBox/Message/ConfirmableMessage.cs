using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AliceMQ.MailBox.DeliveryArgs;

namespace AliceMQ.MailBox.Message
{
    public class ConfirmableMessage<T> : IConfirmableMessage, IConfirmableDelivery
    {
        private readonly ISubject<AcceptDeliveryArgs> _ackSubject;
        private readonly ISubject<RejectDeliveryArgs> _nackSubject;
        public IObservable<AcceptDeliveryArgs> AcksRequests => _ackSubject.AsObservable();
        public IObservable<RejectDeliveryArgs> NacksRequests => _nackSubject.AsObservable();

        public bool Confirmed { get; private set; }

        private static readonly ConcurrentDictionary<ulong,IEnumerable<ConfirmableMessage<T>>> AckableMessages;

        static ConfirmableMessage()
        {
            AckableMessages = new ConcurrentDictionary<ulong, IEnumerable<ConfirmableMessage<T>>>();
        }

        public ConfirmableMessage(Message<T> message)
        {
            Message = message;
            Confirmed = false;
            _ackSubject = new Subject<AcceptDeliveryArgs>();
            _nackSubject = new Subject<RejectDeliveryArgs>();
            AckableMessages.AddOrUpdate(message.DeliveryArgs.DeliveryTag, new[] {this}, (k, v) => v.Concat(new[] {this}));
        }

        public Message<T> Message { get; }

        public void Accept(bool multiple = false)
        {
            CheckConfirmed();
            _ackSubject.OnNext(new AcceptDeliveryArgs(Message.DeliveryArgs.DeliveryTag, multiple));
        }

        private void CheckConfirmed()
        {
            if (Confirmed)
                throw new AlreadyConfirmedMessageException();
        }

        public void ConfirmDelivery()
        {
            foreach(var m in AckableMessages[Message.DeliveryArgs.DeliveryTag].Where(a => !a.Confirmed))
                m.Confirmed = true;
        }

        public void Reject(bool multiple = false, bool requeue = false)
        {
            CheckConfirmed();
            _nackSubject.OnNext(new RejectDeliveryArgs(Message.DeliveryArgs.DeliveryTag, multiple, requeue));
        }
    }
}