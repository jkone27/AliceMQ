using System;
using System.Reactive.Linq;
using AliceMQ.MailBox.Core.Custom;
using AliceMQ.MailBox.DeliveryArgs;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Interface;
using AliceMQ.MailBox.Message;

namespace AliceMQ.MailBox.Core
{
    public class ConfirmableMailbox<T> :
        IConfirmableMailBox<IConfirmableEnvelope<T>>
    {
        private readonly IAckableMailbox<IMessage> _customMailBox;


        public ConfirmableMailbox(EndPoint simpleEndPoint,
            Sink sink,
            Func<string, T> deserializer)
        {
            _customMailBox = new CustomMailBox<T>(simpleEndPoint, sink, deserializer);
        }

        protected ConfirmableMailbox(
            IAckableMailbox<IMessage> customMailBox)
        {
            _customMailBox = customMailBox;
        }

        private void OnNextAck(AcceptDeliveryArgs a, IConfirmableDelivery m)
        {
            _customMailBox.AckRequest(a.DeliveryTag, a.Multiple);
            m.ConfirmDelivery();
        }

        private void OnNextNack(RejectDeliveryArgs n, IConfirmableDelivery m)
        {
            _customMailBox.NackRequest(n.DeliveryTag, n.Multiple, n.Requeue);
            m.ConfirmDelivery();
        }

        public IDisposable Subscribe(IObserver<IConfirmableEnvelope<T>> observer)
        {
            return _customMailBox
                .Select(s => new ConfirmableEnvelope<T>(s))
                .Do(s =>
                {
                    s.AcksRequests.Subscribe(n => OnNextAck(n, s));
                    s.NacksRequests.Subscribe(n => OnNextNack(n, s));
                })
                .AsObservable()
                .Subscribe(observer);
        }

        public IDisposable Connect() => _customMailBox.Connect();

        public void Dispose()
        {
            _customMailBox.Dispose();
        }
    }
}