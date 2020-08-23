using System;

namespace AliceMQ.Core.Types
{
    public interface INetworkRecoveryInfo
    {
        bool AutomaticRecoveryEnabled { get; }
        TimeSpan NetworkRecoveryInterval { get; }
    }
}