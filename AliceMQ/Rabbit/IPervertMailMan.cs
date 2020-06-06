using System;
using System.Collections.Generic;
using AliceMQ.MailMan.Interface;
using RabbitMQ.Client;

namespace AliceMQ.MailMan
{
    public interface IPervertMailMan : IMailman
    {
        void CustomPublishSome<T, TP>(IList<IMessageProperty<T, TP>> messagePropertyTuples, string routingKey,
            Action<TP, IBasicProperties> applyDynamicProperties);
    }
}