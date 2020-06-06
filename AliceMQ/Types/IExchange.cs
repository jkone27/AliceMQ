using System.Collections.Generic;

namespace AliceMQ.MailMan
{
    public interface IExchange
    {
      string ExchangeName { get; }
      string ExchangeType { get; }
      bool Durable { get; }
      bool AutoDelete { get; }
      IDictionary<string, object> Properties { get; }
    }
}