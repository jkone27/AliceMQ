using System;

namespace Alice.Core.Types
{
    public interface INetworkRecoveryInfo
    {
        bool AutomaticRecoveryEnabled { get; }
        TimeSpan NetworkRecoveryInterval { get; }
    }
}