namespace AliceMQ.MailMan.Interface
{
    public interface IMessageProperty<out T, out TP>
    {
        T Message { get; }

        TP Property { get; }
    }
}