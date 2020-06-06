﻿using System;
using Alice.Core.Message;
using Alice.Core.Types;
using AliceMQ.Rabbit.MailBox;
using AliceMQ.Rabbit.Mailman;
using Newtonsoft.Json;

namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = new Source("A", "A.q");
            var endPoint = new EndPoint();
            var sink = new Sink(source);

            var serialization = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };

            var p = new Mailman(endPoint, source.Exchange, s => JsonConvert.SerializeObject(s, serialization));

            //first message published creates exchange if non existent
            p.PublishOne(new Msg(-1),"");

            //var mb = new SimpleMailbox(endPoint, sink);

            //var d = mb.Subscribe(am =>
            //{
            //    Console.WriteLine("A - " + Encoding.UTF8.GetString(am.EventArgs.Body));
            //    am.Channel.BasicAck(am.EventArgs.DeliveryTag, false);
            //});

            var sfm = new Mailbox<Msg>(endPoint, sink, s => JsonConvert.DeserializeObject<Msg>(s, serialization));

            Console.WriteLine("waiting for messages..");

            var d = sfm.Subscribe(am =>
            {
                if (am.IsOk<Msg>())
                {
                    var msg = am.AsOk<Msg>().Message;
                    Console.WriteLine("ok - " + msg.Bla);
                    am.Confirm();
                }
                else
                {
                    Console.WriteLine("error - " + am.AsError().Ex.Message);
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
                    p.PublishOne("{ \"wrong\": \"message\" }", ""); //publish a broken message to test exception handling
                exit = Console.ReadKey().Key;
            }

            //d.Dispose();
            d.Dispose();

        }
    }
}
