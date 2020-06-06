using System;

namespace AliceMQ.MailBox.EndPointArgs
{
    public interface INetworkRecoveryInfo
    {
        bool AutomaticRecoveryEnabled { get; }
        TimeSpan NetworkRecoveryInterval { get; }
    }
}