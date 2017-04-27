using System;
using RabbitMQ.Client;

namespace AliceMQ.MailBox
{
    public class EndpointArgs: INetworkRecoveryInfo
    {
        public string HostName { get; }
        public int Port { get; }
        public string UserName { get; }
        public string Password { get; }
        public string VirtualHost { get; }
        public bool AutomaticRecoveryEnabled { get;}
        public TimeSpan NetworkRecoveryInterval { get;  }

        public EndpointArgs(
            string hostName = "localhost", 
            int port = 5672, 
            string userName = ConnectionFactory.DefaultUser, 
            string password = ConnectionFactory.DefaultPass, 
            string virtualHost = ConnectionFactory.DefaultVHost,
            bool automaticRecoveryEnabled = true,
            int networkRecoveryIntervalMinutes = 1)
        {
            HostName = hostName;
            Port = port;
            UserName = userName;
            Password = password;
            VirtualHost = virtualHost;
            AutomaticRecoveryEnabled = automaticRecoveryEnabled;
            NetworkRecoveryInterval = TimeSpan.FromMinutes(networkRecoveryIntervalMinutes);
        }
       
    }
}