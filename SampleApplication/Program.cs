using System;
using System.Threading.Tasks;
using AliceMQ.ExtensionMethods;
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

            var sfm = new CustomMailbox<Msg>(endPoint, sink, s => JsonConvert.DeserializeObject<Msg>(s, serialization));

            Task.Run(() => Console.WriteLine("waiting for messages.."));


            //var d = mb.Subscribe(am =>
            //{
            //    Console.WriteLine("A - " + Encoding.UTF8.GetString(am.EventArgs.Body));
            //    am.Channel.BasicAck(am.EventArgs.DeliveryTag, false);
            //});

            var z1 = sfm.Subscribe(am =>
            {
                if (am.IsOk<Msg>())
                {
                    var msg = am.AsOk<Msg>().Message;
                    Console.WriteLine("ok - " + msg.Bla);
                    am.Confirm();
                }
                else
                {
                    Console.WriteLine("error - " + am.AsError().Ex);
                    am.Confirm();
                }
                    
            }, 
            ex => Console.WriteLine("COMPLETE ERROR"), 
            () => Console.WriteLine("COMPLETE"));

            var exit = ConsoleKey.N;
            var count = 0;
            while (exit != ConsoleKey.Y)
            {
                if(count++ % 2 != 0)
                    p.PublishOne(new Msg(count), "");
                else
                    p.PublishOne("{ wrong message }", ""); //publish a broken message to test exception handling
                exit = Console.ReadKey().Key;
            }

            //d.Dispose();
            z1.Dispose();

        }
    }
}
