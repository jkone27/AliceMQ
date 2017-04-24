using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox.Interface;
using AliceMQ.MailBox.Message;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace AliceMQ.MailBox.Core
{
    public class CustomMailBox<T>: 
        IConfirmable<Message<T>>, IDisposable
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IConfirmable<BasicDeliverEventArgs> _mailBox;
        private readonly IDisposable _subscription;
        private readonly ISubject<Message<T>> _subject;

        public bool IgnoreMissingJsonFields => _jsonSerializerSettings.MissingMemberHandling == MissingMemberHandling.Ignore;

        public CustomMailBox(IConfirmable<BasicDeliverEventArgs> mailbox, 
            JsonSerializerSettings jsonSeralizerSettings = null)
        {
            _mailBox = mailbox;
            _subject = new Subject<Message<T>>();
            _jsonSerializerSettings = jsonSeralizerSettings;
            _subscription = _mailBox.Subscribe(OnNext, OnError, OnCompleted);
        }

        public void OnNext(BasicDeliverEventArgs args)
        {
            _subject.OnNext(ConsumeMessage(args));
        }

        private void OnCompleted()
        {
            _subject.OnCompleted();
        }

        private void OnError(Exception e)
        {
            _subject.OnError(e);
        }

        private Message<T> ConsumeMessage(BasicDeliverEventArgs e)
        {
            try
            {
                var typedMessage = TypedMessage(Payload(e));

                if (typedMessage == null)
                    throw new UntypedMessageException("typedMessage is null.");

                return new Message<T>(typedMessage, e);
            }
            catch (Exception ex)
            {
                _subject.OnError(ex);
                return null;
            }
        }

        private T TypedMessage(string payload)
        {
            try
            {
                return typeof(T) == typeof(string)
                    ? (T) (object) payload
                    : JsonConvert.DeserializeObject<T>(payload, _jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                throw new UntypedMessageException(ex.Message);
            }
        }

        private static string Payload(BasicDeliverEventArgs e)
        {
            try
            {
                return e.BasicProperties.GetEncoding().GetString(e.Body);
            }
            catch (Exception ex)
            {
                throw new UntypedMessageException(ex.Message);
            }
        }

        public bool AckRequest(ulong deliveryTag, bool multiple)
        {
            return _mailBox.AckRequest(deliveryTag, multiple);
        }

        public bool NackRequest(ulong deliveryTag, bool multiple, bool requeue)
        {
            return _mailBox.NackRequest(deliveryTag, multiple, requeue);
        }

        public bool IsConfirmable => _mailBox.IsConfirmable;

        public void Dispose() => _subscription?.Dispose();

        public IDisposable Subscribe(IObserver<Message<T>> observer) => _subject.AsObservable().Subscribe(observer);
    }
}