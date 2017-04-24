using System.Collections.Generic;

namespace AliceMQ.MailMan.Interface
{
    public interface IMailman
    {
        void PublishOne<T>(T message, string routingKey);

        void PublishSome<T>(IList<T> messages, string routingKey);

        void CustomPublishSome<T,TP>(
            IList<IMessageProperty<T, TP>> messagePropertyTuples, 
            string routingKey, 
            IDynamicPropertiesSetter<TP> propertiesSetter);
    }
}