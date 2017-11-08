using System;
using System.Reactive.Linq;
using Alice.ExtensionMethods;
using Alice.MailBox.EndPointArgs;
using Alice.MailBox.Interface;
using Alice.MailBox.Message;
using RabbitMQ.Client.Events;

namespace Alice.MailBox.Core
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
            return _simpleMailbox.Select<IMailboxContext, IMessage>(s =>
                {
                    try
                    {
                        var decodedString = Payload(s.EventArgs);
                        return new Ok<T>(_serializer(decodedString), s, _simpleMailbox.Sink.ConfirmationPolicy.Multiple, _simpleMailbox.Sink.ConfirmationPolicy.Requeue);
                    }
                    catch (Exception ex)
                    {
                        return new Error(s, ex, _simpleMailbox.Sink.ConfirmationPolicy.Multiple, _simpleMailbox.Sink.ConfirmationPolicy.Requeue);
                    }
                })
                .Subscribe(observer.OnNext, observer.OnError, observer.OnCompleted);
        }

        private static string Payload(BasicDeliverEventArgs e)
        {
            return e.BasicProperties.GetEncoding().GetString(e.Body);
        }
    }
}