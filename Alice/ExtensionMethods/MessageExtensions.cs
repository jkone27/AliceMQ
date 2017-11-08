using Alice.MailBox.Interface;
using Alice.MailBox.Message;

namespace Alice.ExtensionMethods
{
    public static class MessageExtensions
    {
        public static Ok<T> AsOk<T>(this IMessage message)
        {
            return message as Ok<T>;
        }

        public static Error AsError(this IMessage message)
        {
            return message as Error;
        }

        public static bool IsOk<T>(this IMessage message)
        {
            return message is Ok<T>;
        }

        public static bool IsError(this IMessage message)
        {
            return message is Error;
        }
    }
}
