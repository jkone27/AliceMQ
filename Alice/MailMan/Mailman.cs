using System;
using System.Collections.Generic;
using Alice.MailBox.EndPointArgs;
using Alice.MailMan.Interface;
using RabbitMQ.Client;

namespace Alice.MailMan
{
    public class Mailman : 
        IMailman
    {
        private readonly Action<IBasicProperties> _staticPropertiesSetter;
        private readonly Action<Exception> _publishErrorAction;
        private readonly RabbitMailmanLogic _logic;

        public Mailman( 
            EndPoint simpleEndpoint, 
            IExchange exchange,
            Func<object, string> serializer,
            Action<Exception> publishErrorAction = null,
            Action<IBasicProperties> staticPropertiesSetter = null
            )
        {
            _logic = new RabbitMailmanLogic(simpleEndpoint, exchange, serializer);
            _staticPropertiesSetter = staticPropertiesSetter;
            _publishErrorAction = publishErrorAction ?? (p => { });
        }


        public void PublishOne<T>(T message, string routingKey)
        {
            try
            {
                _logic.PublishOne(message, routingKey, _staticPropertiesSetter);
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
                _logic.PublishSome(messages, routingKey, _staticPropertiesSetter);
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
                _logic.CustomPublishSome(messagePropertyTuples, routingKey, dynamicPropertiesSetter);
            }
            catch (Exception ex)
            {
                _publishErrorAction(ex);
            }
        }
    }
}