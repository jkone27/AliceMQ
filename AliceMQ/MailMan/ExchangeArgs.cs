using System.Collections.Generic;

namespace AliceMQ.MailMan
{
    public class ExchangeArgs
    {
        public ExchangeArgs() { }
        public ExchangeArgs(string exchangeName, string exchangeType, IDictionary<string, object> properties = null, bool durable = true)
        {
            ExchangeName = exchangeName;
            ExchangeType = exchangeType;
            Durable = durable;
            Properties = properties ?? new Dictionary<string, object>();
        }

        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }
        public bool Durable { get; set; }
        //must be settable at runtime
        public IDictionary<string, object> Properties { get; set; }
    }
}