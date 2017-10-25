using System;
using System.Text;
using System.Threading.Tasks;
using AliceMQ.MailBox;
using AliceMQ.MailBox.Core;
using AliceMQ.MailBox.EndPointArgs;
using AliceMQ.MailMan;
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

            var mb = new MailBoxBase(endPoint, sink);

            //var mb = new MailBox(endPoint, sink);

            //var custom = new CustomMailBox<Msg>(
            //    endPoint,
            //    sink,
            //    s => JsonConvert.DeserializeObject<Msg>(s, serialization));

            //var confirmable = new ConfirmableMailbox<Msg>(
            //    endPoint, 
            //    sink,
            //    s => JsonConvert.DeserializeObject<Msg>(s, serialization));

            Task.Run(() => Console.WriteLine("waiting for messages.."));


            var d = mb.Subscribe(am =>
            {
                Console.WriteLine("A - " + Encoding.UTF8.GetString(am.EventArgs.Body));
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


            var exit = ConsoleKey.N;
            var count = 0;
            while (exit != ConsoleKey.Y)
            {
                p.PublishOne(new Msg(count++), "");
                //p.PublishOne("{ wrong message }", ""); //publish a broken message to test exception handling
                exit = Console.ReadKey().Key;
            }

            d.Dispose();

        }
    }
}
