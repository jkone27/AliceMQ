using System;

namespace AliceMQ.MailBox.EndPointArgs
{
    public class EndPoint: INetworkRecoveryInfo
    {
        public bool AutomaticRecoveryEnabled { get; }
        public TimeSpan NetworkRecoveryInterval { get;}
        public string ConnectionUrl { get; }

        public EndPoint()
        {
            ConnectionUrl = "amqp://guest:guest@localhost:5672";
            AutomaticRecoveryEnabled = true;
            NetworkRecoveryInterval = TimeSpan.FromSeconds(30);
        }

        public EndPoint(string connectionUrl)
        {
            ConnectionUrl = connectionUrl;
            AutomaticRecoveryEnabled = true;
            NetworkRecoveryInterval = TimeSpan.FromSeconds(30);
        }

        public EndPoint(string connectionUrl, 
            bool automaticRecoveryEnabled, 
            TimeSpan networkRecoveryInterval)
        {
            ConnectionUrl = connectionUrl;
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            NetworkRecoveryInterval = networkRecoveryInterval;
        }

        
    }
}