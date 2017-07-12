using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailMan.Interface;
using RabbitMQ.Client;

namespace AliceMQ.MailMan
{
    public class Mailman : 
        RabbitMailmanLogic, 
        IMailman
    {
        private readonly Action<IBasicProperties> _staticPropertiesSetter;
        private readonly Action<Exception> _publishErrorAction;

        public Mailman(
            ExchangeArgs exchangeArgs, 
            EndpointArgs endpointArgs,
            Func<object, string> serializer,
            Action<Exception> publishErrorAction = null,
            Action<IBasicProperties> staticPropertiesSetter = null
        )
            : base(endpointArgs, exchangeArgs, serializer)
        {
            _staticPropertiesSetter = staticPropertiesSetter ?? (_ => { });
            _publishErrorAction = publishErrorAction ?? (p => { });
        }

        public Mailman( 
            SimpleEndpointArgs simpleEndpointArgs, 
            ExchangeArgs exchangeArgs,
            Func<object, string> serializer,
            Action<Exception> publishErrorAction = null,
            Action<IBasicProperties> staticPropertiesSetter = null
            )
			: base(simpleEndpointArgs, exchangeArgs, serializer)
        {
            _staticPropertiesSetter = staticPropertiesSetter;
            _publishErrorAction = publishErrorAction ?? (p => { });
        }


        public void PublishOne<T>(T message, string routingKey)
        {
            try
            {
                base.PublishOne(message, routingKey, _staticPropertiesSetter);
            }
            catch (Exception ex)
            {
                _publishErrorAction(ex);
            }
        }

        public void PublishSome<T>(IList<T> messages, string routingKey)
        {
            try
            {
                base.PublishSome(messages, routingKey, _staticPropertiesSetter);
            }
            catch (Exception ex)
            {
                _publishErrorAction(ex);
            }
        }

        public void CustomPublishSome<T,TP>(
            IList<IMessageProperty<T, TP>> messagePropertyTuples, 
            string routingKey,
            Action<TP, IBasicProperties> dynamicPropertiesSetter)
        {
            try
            {
                base.CustomPublishSome(messagePropertyTuples, routingKey, dynamicPropertiesSetter);
            }
            catch (Exception ex)
            {
                _publishErrorAction(ex);
            }
        }
    }
}