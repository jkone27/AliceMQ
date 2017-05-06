using System;
using System.Text;
using System.Threading.Tasks;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailMan;
using AliceMQ.Serialize;
using Newtonsoft.Json;

namespace SampleApplication
{
    class Program
    {
        public class Msg
        {
            public int Bla { get; }

            public Msg(int bla)
            {
                Bla = bla;
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
            p.PublishOne(new Msg(-1),"");

            var mb = new MailBox(endpointArgs, mailboxArgs, false);

            var custom = new CustomMailBox<Msg>(mb, serialization);
            var confirmable = new ConfirmableMailbox(custom);

            Task.Run(() => Console.WriteLine("waiting for messages.."));

           
            mb.Subscribe(am =>
            {
                Console.WriteLine("A - " + Encoding.UTF8.GetString(am.Body));
                //mb.AckRequest(am.DeliveryTag, false);
            });

            custom.Subscribe(am =>
            {
                if (am.IsOk<Msg>())
                {
                    Console.WriteLine("B - " + am.AsOk<Msg>().Message.Bla);
                    //custom.AckRequest(am.RawData.DeliveryTag, false);
                }
                else
                {
                    Console.WriteLine("B- error: " + am.AsError().Ex);
                    //custom.NackRequest(am.RawData.DeliveryTag, false, false);
                }
            });

            confirmable.Subscribe(am =>
            {
                if (am.Content.IsOk<Msg>())
                {
                    Console.WriteLine("C - " + am.Content.AsOk<Msg>().Message?.Bla);
                    am.Accept();
                }
                else
                {
                    Console.WriteLine("C - error." + am.Content.AsError().Ex);
                    am.Reject();
                }
               
            });

            var exit = ConsoleKey.N;
            var count = 0;
            while (exit != ConsoleKey.Y)
            {
                p.PublishOne(new Msg(count++), "");
                p.PublishOne("{ wrong message }", ""); //publish a broken message to test exception handling
                exit = Console.ReadKey().Key;
            }
        }
    }
}
