using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace AliceMQ.ExtensionMethods
{
    public static class BasicPropertiesExtensions
    {
        public static Encoding GetEncoding(this IBasicProperties basicProperties)
        {
            return 
                basicProperties.ContentEncoding != null ?
                    Encoding.GetEncoding(basicProperties.ContentEncoding)
                    : Encoding.UTF8;
        }

        public static Dictionary<string, object> DecodeHeaders(this IBasicProperties basicProperties)
        {
            return basicProperties.Headers.ToDictionary(s => s.Key,
                s => s.Value == null
                    ? null
                    : s.Value.GetType() == typeof(byte[])
                        ? basicProperties.GetEncoding().GetString((byte[]) s.Value)
                        : s.Value);
        }

        public static IBasicProperties Copy(this IBasicProperties basicProperties)
        {
            var bp = new RabbitMQ.Client.Framing.BasicProperties
            {
                AppId = basicProperties.AppId,
                ClusterId = basicProperties.ClusterId,
                ContentEncoding = basicProperties.ContentEncoding,
                ContentType = basicProperties.ContentType,
                CorrelationId = basicProperties.CorrelationId,
                DeliveryMode = basicProperties.DeliveryMode,
                Expiration = basicProperties.Expiration,
                Headers = basicProperties.Headers != null
                    ? new Dictionary<string, object>(basicProperties.Headers)
                    : null,
                Type = basicProperties.Type,
                MessageId = basicProperties.MessageId,
                Timestamp = basicProperties.Timestamp,
                Persistent = basicProperties.Persistent,
                Priority = basicProperties.Priority,
                ReplyTo = basicProperties.ReplyTo,
                UserId = basicProperties.UserId,
                ReplyToAddress = basicProperties.ReplyToAddress
            };

            return bp;
        }
        public static IBasicProperties CopyWithNewHeaders
            (this IBasicProperties basicProperties, IDictionary<string,object> headers)
        {
            return new RabbitMQ.Client.Framing.BasicProperties
            {
                AppId = basicProperties.AppId,
                ClusterId = basicProperties.ClusterId,
                ContentEncoding = basicProperties.ContentEncoding,
                ContentType = basicProperties.ContentType,
                CorrelationId = basicProperties.CorrelationId,
                DeliveryMode = basicProperties.DeliveryMode,
                Expiration = basicProperties.Expiration,
                Headers = headers,
                Type = basicProperties.Type,
                MessageId = basicProperties.MessageId,
                Timestamp = basicProperties.Timestamp,
                Persistent = basicProperties.Persistent,
                Priority = basicProperties.Priority,
                ReplyTo = basicProperties.ReplyTo,
                UserId = basicProperties.UserId,
                ReplyToAddress = basicProperties.ReplyToAddress
            };
        }

        public static IBasicProperties AssignProperties(this IBasicProperties properties,
            Action<IBasicProperties> propertiesSetter)
        {
            propertiesSetter(properties);
            return properties;
        }
    }
}