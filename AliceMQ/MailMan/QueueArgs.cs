namespace AliceMQ.MailMan
{
    public class QueueArgs
    {
        public QueueArgs(string queueName = "", bool durable = true, bool exclusive = false, bool autoDelete = false)
        {
            QueueName = queueName;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
        }

        public string QueueName { get; }
        public bool Durable { get; }
        public bool Exclusive { get; }
        public bool AutoDelete { get; }
    }
}