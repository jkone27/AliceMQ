using System;
using RabbitMQ.Client;

namespace AliceMQ.MailBox
{
    public class ConnectionFactoryParams
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public bool AutomaticRecoveryEnabled { get; set; }
        public TimeSpan NetworkRecoveryInterval { get; set; }

        public ConnectionFactoryParams(
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