using AliceMQ.MailMan.Interface;
using RabbitMQ.Client;

namespace AliceMQ.MailMan
{
    public class NoSetter : IStaticPropertiesSetter
    {
        public void Set(IBasicProperties properties)
        {
            //do nothing;
        }
    }
}