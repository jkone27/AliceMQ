using System;
using System.Collections.Generic;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox;
using AliceMQ.MailMan.Interface;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace AliceMQ.MailMan
{
    public class Mailman : 
        RabbitMailmanLogic, 
        IMailman
    {
        private readonly IStaticPropertiesSetter _staticPropertiesSetter;
        private readonly Action<Exception> _publishErrorAction;

        public Mailman(
            MailArgs mailArgs, 
            EndpointArgs endpointArgs,
            Action<Exception> publishErrorAction = null,
            IStaticPropertiesSetter staticPropertiesSetter = null,
            Formatting formatting = Formatting.None,
            JsonSerializerSettings jsonSerializerSettings = null
        )
            : base(endpointArgs, mailArgs, formatting, jsonSerializerSettings)
        {
            _staticPropertiesSetter = staticPropertiesSetter ?? new NoSetter();
            _publishErrorAction = publishErrorAction ?? (p => { });
        }

        public Mailman( 
            SimpleEndpointArgs simpleEndpointArgs, 
            MailArgs mailArgs,
            Action<Exception> publishErrorAction = null,
            IStaticPropertiesSetter staticPropertiesSetter = null,
            Formatting formatting = Formatting.None,
            JsonSerializerSettings jsonSerializerSettings = null
            )
            : base(simpleEndpointArgs, mailArgs, formatting, jsonSerializerSettings)
        {
            _staticPropertiesSetter = staticPropertiesSetter;
            _publishErrorAction = publishErrorAction ?? (p => { });
        }


        public void PublishOne<T>(T message, string routingKey)
        {
            ((Action<
                T, string, Action<IBasicProperties>
                >) base.PublishOne)
            .TryDo(message, routingKey, _staticPropertiesSetter.Set, _publishErrorAction);
        }

        public void PublishSome<T>(IList<T> messages, string routingKey)
        {
            ((Action<
                IList<T>, string, Action<IBasicProperties>
                >) base.PublishSome)
            .TryDo(messages,routingKey, _staticPropertiesSetter.Set, _publishErrorAction);
        }

        public void CustomPublishSome<T,TP>(IList<IMessageProperty<T, TP>> messagePropertyTuples, string routingKey,
            IDynamicPropertiesSetter<TP> propertiesSetter)
        {
            ((Action<
                IList<IMessageProperty<T, TP>>, string, Action<TP,IBasicProperties>
                >) 
                base.CustomPublishSome)
           .TryDo(messagePropertyTuples, routingKey, propertiesSetter.Set, _publishErrorAction);
        }
    }
}