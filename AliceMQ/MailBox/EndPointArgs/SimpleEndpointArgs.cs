using System;

namespace AliceMQ.MailBox.EndPointArgs
{
    public class SimpleEndpointArgs: INetworkRecoveryInfo
    {
        public bool AutomaticRecoveryEnabled { get; }
        public TimeSpan NetworkRecoveryInterval { get; }

        public string ConnectionUrl { get; }

        public SimpleEndpointArgs(string connectionUrl, bool automaticRecoveryEnabled, TimeSpan networkRecoveryInterval)
        {
            ConnectionUrl = connectionUrl;
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            NetworkRecoveryInterval = networkRecoveryInterval;
        }
    }
}