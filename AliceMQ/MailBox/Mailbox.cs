using System;
using System.Reactive.Linq;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailBox.Message;
using AliceMQ.Rabbit;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public class Mailbox<T> : IObservable<IMessage>
    {
        private readonly Func<string, T> _serializer;
        private readonly ISimpleMailbox _simpleMailbox;

        public Mailbox(EndPoint endPoint, Sink sink, Func<string, T> serializer)
        {
            _serializer = serializer;
            _simpleMailbox = new SimpleMailbox(endPoint, sink);
        }

        protected Mailbox(ISimpleMailbox simpleMailbox, Func<string, T> serializer)
        {
            _simpleMailbox = simpleMailbox;
            _serializer = serializer;
        }

        public IDisposable Subscribe(IObserver<IMessage> observer)
        {
            return _simpleMailbox.Select<IDeliveryContext, IMessage>(s =>
                {
                    var context = new Context(s, 
                        _simpleMailbox.Sink.ConfirmationPolicy.Multiple, 
                        _simpleMailbox.Sink.ConfirmationPolicy.Requeue);
                    try
                    {
                        return new Ok<T>(_serializer(s.Payload), context);
                    }
                    catch (Exception ex)
                    {
                        return new Error(ex, context);
                    }
                })
                .Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
        }
    }
}