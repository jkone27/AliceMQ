namespace AliceMQ.MailBox.Interface
{
    public interface IResult<out T>
    {
        T RawData { get; }
    }
}