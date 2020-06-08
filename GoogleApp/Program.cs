using System;
using System.Text;
using System.Threading.Tasks;
using AliceMQ.GooglePubSub;
using Newtonsoft.Json;

namespace GoogleApp
{
    class Program
    {
        //docker run --rm -ti -p 8681:8681 -e PUBSUB_PROJECT1=test-proj,topic1:subscription1 messagebird/gcloud-pubsub-emulator:latest
        const string projectId = "test-proj";
        const string topicId = "topic1";
        const string subscriptionId = "subscription1";

        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", "localhost:8681");
            var emulatorHostAndPort = Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");
            //var pubsubProjectId = Environment.GetEnvironmentVariable("PUBSUB_PROJECT_ID");

            var serialization = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };

            var source = new GoogleSource(topicId, projectId, subscriptionId);
            var p = new GoogleMailman(emulatorHostAndPort, projectId, topicId, s => JsonConvert.SerializeObject(s, serialization));

            //first message published creates exchange if non existent
            var msgId = await p.PublishOneAsync(new Msg(-1), subscriptionId);

            Console.WriteLine(msgId);

            var sink = new GoogleSink(source, subscriptionId);

            var mb = new GoogleMailbox(projectId, emulatorHostAndPort, sink);

            //fake disposable
            var _ = mb.Subscribe(am =>
            {
                Console.WriteLine("A - " + am.Payload);
                am.Ack(false);

                throw new Exception("completed");
            }, 
            Console.WriteLine,
            Console.WriteLine);

            while (true)
            {
                if(Console.ReadKey().Key == ConsoleKey.Y)
                {
                    break;
                }
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


