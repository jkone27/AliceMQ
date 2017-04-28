using System;
using System.Text;
using System.Threading.Tasks;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.Message;
using AliceMQ.MailMan;
using AliceMQ.Serialize;
using Newtonsoft.Json;

namespace SampleApplication
{
    class Program
    {
        public class Msg
        {
            public int Content { get; }

            public Msg(int content)
            {
                Content = content;
            }
        }
        static void Main(string[] args)
        {
            var endpointArgs = new EndpointArgs();
            var mailArgs = new MailArgs("A", "A.q");

            var mailboxArgs = new MailboxArgs(mailArgs);
            var serialization = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new FromPascalToJsContractResolver()
            };
            var p = new Mailman(mailArgs, endpointArgs, formatting: Formatting.Indented, jsonSerializerSettings: serialization);
            //first message published creates exchange if non existent
            p.PublishOne(new Msg(1),"");

            var mb = new MailBox(endpointArgs, mailboxArgs, false);

     

            var custom = new CustomMailBox<Msg>(mb, serialization);
            var confirmable = new ConfirmableMailbox<Msg>(custom);

           

            Task.Run(() => Console.WriteLine("waiting for messages.."));


            mb.Subscribe(am =>
            {
                Console.WriteLine("mailbox - " + Encoding.UTF8.GetString(am.Body));
            });

            custom.Subscribe(am =>
            {
                Console.WriteLine("custom mailbox - " + am.Datum.Content);
            });

            confirmable.Subscribe(am =>
            {
                Console.WriteLine("confirmable mailbox - " + am.Message.Datum.Content);
                am.Accept();
            },
            e =>
            {
                var cm = e.AsConfirmable<Msg>().ConfirmableMessage;
                Console.WriteLine("confirmable mailbox - Exception! accepting message. (discard)");
                cm.Accept();
            });

            confirmable.Subscribe(am =>
            {
                try
                {
                    Console.WriteLine("confirmable mailbox nr.2 - already acked " + am.Message.Datum.Content);
                    am.Accept();
                }
                catch (AlreadyConfirmedMessageException ex)
                {
                    Console.WriteLine(ex.GetType().Name); //ok, not possible to ack twice!
                }
            });

            var exit = ConsoleKey.N;
            var count = 0;
            while (exit != ConsoleKey.Y)
            {
                p.PublishOne(new Msg(count++), "");
                p.PublishOne("{ \"Content\": \"wrong message\" }", ""); //publish a broken message to test exception handling
                exit = Console.ReadKey().Key;
            }

            confirmable.Dispose();
        }
    }
}
