using System.Collections.Generic;

namespace AliceMQ.MailMan.Interface
{
    public interface IMailman
    {
        void PublishOne<T>(T message, string routingKey);

        void PublishSome<T>(IList<T> messages, string routingKey);
    }
}