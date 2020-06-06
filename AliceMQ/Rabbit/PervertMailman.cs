using System;
using System.Collections.Generic;
using System.Linq;
using AliceMQ.MailMan.Interface;
using RabbitMQ.Client;

namespace AliceMQ.MailMan
{
    public sealed class PervertMailman : IPervertMailMan
    {
        private readonly Mailman mailMan;

        public PervertMailman(Mailman mailMan)
        {
            this.mailMan = mailMan;
        }

        public void PublishOne<T>(T message, string routingKey)
        {
            mailMan.PublishOne(message, routingKey);
        }

        public void PublishSome<T>(IList<T> messages, string routingKey)
        {
            mailMan.PublishSome(messages, routingKey);
        }


        // custom function for setting dynamic properties based on message, deprecated
        [Obsolete("such a low level of customization shouldn't be required.")]
        public void CustomPublishSome<T, TP>(IList<IMessageProperty<T, TP>> messagePropertyTuples, string routingKey,
            Action<TP, IBasicProperties> applyDynamicProperties)
        {
            mailMan.PublishSome(messagePropertyTuples.Select(mp => mp.Message), routingKey,
                CollectPropertiesCustomizationOnChannel(messagePropertyTuples, applyDynamicProperties));
        }

        private static Action<IBasicProperties> CollectPropertiesCustomizationOnChannel<T, TP>(
            IEnumerable<IMessageProperty<T, TP>> messagePropertyTuples,
            Action<TP, IBasicProperties> applyDynamicProperties)
        {
            return messagePropertyTuples
                .Select(mp => (Action<IBasicProperties>)(p => applyDynamicProperties(mp.Property, p)))
                .Aggregate((previous, next) => previous + next);
        }
    }
}