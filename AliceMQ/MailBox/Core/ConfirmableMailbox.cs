using System;
using System.CodeDom;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection.Emit;
using AliceMQ.MailBox.DeliveryArgs;
using AliceMQ.MailBox.Interface;
using AliceMQ.MailBox.Message;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public class ConfirmableMailbox<T> : 
        IObservable<ConfirmableMessage<T>>, IDisposable
    {
        private readonly IConfirmable<Message<T>> _customMailBox;
        private IDisposable _ackHandler;
        private IDisposable _nackHandler;
        private IDisposable _ackSub;
        private IDisposable _nackSub;
        private readonly IDisposable _consumerSub;
        private readonly ISubject<ConfirmableMessage<T>> _subject;
        private readonly ISubject<ConfirmableMessage<T>> _backup;
        private readonly ISubject<ConfirmableMessage<T>> _innerFallback;

        public ConfirmableMailbox(
            IConfirmable<Message<T>> customMailBox)
        {
            _subject = new Subject<ConfirmableMessage<T>>();
            _backup = new Subject<ConfirmableMessage<T>>();
            _innerFallback = new Subject<ConfirmableMessage<T>>();
            _customMailBox = customMailBox;
            if(!_customMailBox.IsConfirmable)
                throw new MailboxException("non ackable consumer");
            SubscribeAcknowledge();
            _consumerSub = _customMailBox.Subscribe(OnNext, OnError, OnCompleted);
        }

        private void SubscribeAcknowledge()
        {
            _ackHandler = _subject.AsObservable().Merge(_innerFallback.AsObservable()).Subscribe(SetupAckObservable);
            _nackHandler = _subject.AsObservable().Merge(_innerFallback.AsObservable()).Subscribe(SetupNackObservable);
        }

        private void UnscribeAcknowledge()
        {
            _ackSub?.Dispose();
            _ackHandler?.Dispose();
            _nackSub?.Dispose();
            _nackHandler?.Dispose();
        }

        private void OnNextAck(AcceptDeliveryArgs a, IConfirmableDelivery m)
        {
            _customMailBox.AckRequest(a.DeliveryTag, a.Multiple);
            m.ConfirmDelivery();
        }

        private void SetupAckObservable(ConfirmableMessage<T> am)
        {
            _ackSub = am.AcksRequests.Subscribe(n => OnNextAck(n,am));
        }

        private void OnNextNack(RejectDeliveryArgs n, IConfirmableDelivery m)
        {
            _customMailBox.NackRequest(n.DeliveryTag, n.Multiple, n.Requeue);
            m.ConfirmDelivery();
        }

        private void SetupNackObservable(ConfirmableMessage<T> am)
        {
            _nackSub = am.NacksRequests.Subscribe(n => OnNextNack(n,am));
        }

        private void OnNext(Message<T> value)
        {
            _subject.OnNext(new ConfirmableMessage<T>(value));
        }

        private void OnError(Exception ex)
        {
            if (ex is CustomMailboxException)
            {
                try
                {
                    var e = (CustomMailboxException)ex;
                    var msg = new Message<T>(default(T), e.Metadata);
                    var confirmableMessage = new ConfirmableMessage<T>(msg);
                    _innerFallback.OnNext(confirmableMessage); //must be set in order to enable Confirm and Reject of faulted messages
                    _backup.OnError(new ConfirmableMessageException<T>(confirmableMessage, e.Message));
                }
                catch(Exception z)
                {
                    _backup.OnError(z);
                }
            }
            else
                _subject.OnError(ex);
        }

        private void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void Dispose()
        {
            _consumerSub?.Dispose();
            UnscribeAcknowledge();
        }

        public IDisposable Subscribe(IObserver<ConfirmableMessage<T>> observer)
        {
            return _subject.Merge(_backup).AsObservable().Subscribe(observer);
        }
    }
}