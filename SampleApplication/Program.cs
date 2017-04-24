using System;
using System.Text;
using System.Threading.Tasks;
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
            public string Content { get; }

            public Msg(string content)
            {
                Content = content;
            }
        }
        static void Main(string[] args)
        {
            var connectionFactoryParameters = new ConnectionFactoryParams();
            var mailArgs = new MailArgs("A", "A.q");
            var mailboxArgs = new MailboxArgs(mailArgs);

            var mb = new MailBox(connectionFactoryParameters, mailboxArgs, false);

            var serialization = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new FromPascalToJsContractResolver() 
            };

            var custom = new CustomMailBox<Msg>(mb, serialization);
            var confirmable = new ConfirmableMailbox<Msg>(custom);

            var p = new Mailman(mailArgs, formatting: Formatting.Indented, jsonSerializerSettings: serialization);

            Task.Run(() => Console.WriteLine("waiting for messages.."));


            mb.Subscribe(am =>
            {
                Console.WriteLine("mailbox - " + Encoding.UTF8.GetString(am.Body));
            }, Console.WriteLine, () => Console.WriteLine("completed"));

            custom.Subscribe(am =>
            {
                Console.WriteLine("custom mailbox - " + am.Datum.Content);
            });

            confirmable.Subscribe(am =>
            {
                Console.WriteLine("confirmable mailbox - " + am.Message.Datum.Content);
                am.Accept();
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
                p.PublishOne(new Msg("msg" + count++), "");
                exit = Console.ReadKey().Key;
            }

            confirmable.Dispose();
        }
    }
}
