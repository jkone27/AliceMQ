using RabbitMQ.Client;

namespace AliceMQ.MailMan.Interface
{
    //TODO: reorganize interfaces and Dep Inj
    public interface IStaticPropertiesSetter
    {
        void Set(IBasicProperties properties);
    }

    public interface IDynamicPropertiesSetter<in TD>
    {
        void Set(TD dynamicAttribute, IBasicProperties properties);
    }
}