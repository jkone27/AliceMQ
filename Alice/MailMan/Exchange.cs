using System.Collections.Generic;

namespace Alice.MailMan
{
    public class Exchange : IExchange
    {
        public Exchange() { }
        public Exchange(string exchangeName, 
			string exchangeType, 
			IDictionary<string, object> properties = null, 
			bool durable = true,
		    bool autoDelete = false)
        {
            ExchangeName = exchangeName;
            ExchangeType = exchangeType;
            Durable = durable;
            Properties = properties ?? new Dictionary<string, object>();
			AutoDelete = autoDelete;
        }

        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }
        public bool Durable { get; set; }
		public bool AutoDelete { get; set; }
        //must be settable at runtime
        public IDictionary<string, object> Properties { get; set; }
    }

    public interface IExchange
    {
      string ExchangeName { get; }
      string ExchangeType { get; }
      bool Durable { get; }
      bool AutoDelete { get; }
      IDictionary<string, object> Properties { get; }
    }
}