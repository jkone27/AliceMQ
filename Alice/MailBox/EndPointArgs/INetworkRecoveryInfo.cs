using System;

namespace Alice.MailBox.EndPointArgs
{
    public interface INetworkRecoveryInfo
    {
        bool AutomaticRecoveryEnabled { get; }
        TimeSpan NetworkRecoveryInterval { get; }
    }
}