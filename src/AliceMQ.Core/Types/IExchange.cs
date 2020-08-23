using System.Collections.Generic;

namespace AliceMQ.Core.Types
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