using System;

namespace AliceMQ.MailBox
{
    public interface INetworkRecoveryInfo
    {
        bool AutomaticRecoveryEnabled { get; }
        TimeSpan NetworkRecoveryInterval { get; }
    }
}