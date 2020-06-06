namespace Alice.Core.Types
{
    public class QueueArgs : IQueueArgs
    {
        public QueueArgs(string queueName = "", bool durable = true, bool exclusive = false, bool autoDelete = false)
        {
            QueueName = queueName;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
        }

        public string QueueName { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
    }
}