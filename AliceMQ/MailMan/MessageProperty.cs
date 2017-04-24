using AliceMQ.MailMan.Interface;

namespace AliceMQ.MailMan
{
    public class MessageProperty<T, TP> : IMessageProperty<T, TP>
    {
        public T Message { get; }

        public TP Property { get; }

        public MessageProperty(T message, TP property)
        {
            Message = message;
            Property = property;
        }
    }
}