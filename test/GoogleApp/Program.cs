using System;
using System.Text;
using System.Threading.Tasks;
using AliceMQ.PubSub;
using Newtonsoft.Json;

namespace PubSubApp
{
    class Program
    {
        //docker run --rm -ti -p 8681:8681 -e PUBSUB_PROJECT1=test-proj,topic1:subscription1 messagebird/gcloud-pubsub-emulator:latest
        const string projectId = "test-proj";
        const string topicId = "topic1";
        const string subscriptionId = "subscription1";

        private static readonly Random rnd = new Random();

        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8681");
            var emulatorHostAndPort = Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");
            //var pubsubProjectId = Environment.GetEnvironmentVariable("PUBSUB_PROJECT_ID");

            var serialization = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };

            var source = new Source(topicId, projectId, subscriptionId);
            var p = new Mailman(emulatorHostAndPort, projectId, topicId, s => JsonConvert.SerializeObject(s, serialization));

            //first message published creates exchange if non existent
            var msgId = await p.PublishOneAsync(new Msg(rnd.Next(1,100)), subscriptionId);

            Console.WriteLine(msgId);

            var sink = new Sink(source, subscriptionId);

            var mb = new SimpleMailbox(projectId, emulatorHostAndPort, sink);

            //fake disposable
            using var x = mb.Subscribe(am =>
            {
                Console.WriteLine("A - " + am.Payload);
                am.Ack(false);
            }, 
            Console.WriteLine,
            Console.WriteLine);

            Console.WriteLine("press Y to exit.., any other key to publish a new msg");

            while (true)
            {
                if(Console.ReadKey().Key == ConsoleKey.Y)
                {
                    break;
                }

                var newMsgId = await p.PublishOneAsync(new Msg(rnd.Next(1, 100)), subscriptionId);

                Console.WriteLine(newMsgId);
            }

        }

        public class Msg
        {
            public int Value { get; }

            public Msg(int value)
            {
                Value = value;
            }
        }
    }
}


