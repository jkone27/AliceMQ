using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace AliceMQ.Rabbit
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

        public static IBasicProperties AssignProperties(this IBasicProperties properties,
            Action<IBasicProperties> propertiesSetter)
        {
            propertiesSetter?.Invoke(properties);
            return properties;
        }
    }
}