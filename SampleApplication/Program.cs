using System;
using System.Text;
using System.Threading.Tasks;
using AliceMQ.ExtensionMethods;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.Core.Custom;
using AliceMQ.MailBox.Core.Simple;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailMan;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            var source = new Source("A", "A.q");
            var endPoint = new EndPoint();
            var sink = new Sink(source);

            var serialization = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var p = new Mailman(endPoint, source.Exchange, s => JsonConvert.SerializeObject(s, serialization));

            //first message published creates exchange if non existent
            p.PublishOne(new Msg(-1),"");

            var mb = new MailBox(endPoint, sink);

            //var custom = new CustomMailBox<Msg>(
            //    endPoint,
            //    sink,
            //    s => JsonConvert.DeserializeObject<Msg>(s, serialization));

            //var confirmable = new ConfirmableMailbox<Msg>(
            //    endPoint, 
            //    sink,
            //    s => JsonConvert.DeserializeObject<Msg>(s, serialization));

            Task.Run(() => Console.WriteLine("waiting for messages.."));


            mb.Subscribe(am =>
            {
                Console.WriteLine("A - " + Encoding.UTF8.GetString(am.Body));
            });

            //custom.Subscribe(am =>
            //{
            //    if (am.IsOk<Msg>())
            //    {
            //        Console.WriteLine("B - " + am.AsOk<Msg>().Message.Bla);
            //    }
            //    else
            //    {
            //        Console.WriteLine("B - error: " + am.AsError().Ex);
            //    }
            //});

            //confirmable.Subscribe(am =>
            //{
            //    if (am.IsOk())
            //    {
            //        Console.WriteLine("C - " + am.Content().Bla);
            //        am.Accept();
            //    }
            //    else
            //    {
            //        Console.WriteLine("C - error." + am.Exception());
            //        am.Reject();
            //    }

            //});

            var d1 = mb.Connect();
            //var d2 = custom.Connect();
            //var d3 = confirmable.Connect();

            var exit = ConsoleKey.N;
            var count = 0;
            while (exit != ConsoleKey.Y)
            {
                p.PublishOne(new Msg(count++), "");
                //p.PublishOne("{ wrong message }", ""); //publish a broken message to test exception handling
                exit = Console.ReadKey().Key;
            }

            //confirmable.Dispose();
            d1.Dispose();
            //d2.Dispose();
            //d3.Dispose();
        }
    }
}
