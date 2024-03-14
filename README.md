[![Ceasefire Now](https://badge.techforpalestine.org/ceasefire-now)](https://techforpalestine.org/learn-more)
[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

# AliceMQ [![NuGet Badge](https://buildstats.info/nuget/AliceMQ)](https://www.nuget.org/packages/AliceMQ)

<img src="https://github.com/jkone27/AliceMQ/blob/master/Pics/Whiterabbit_tenniel.jpg?raw=true" width="20%" height="20%"/>

A reactive client library with support for RabbitMq and experimental support for google pubsub (TBR), 
using reactive extensions for .net

## local environment setup


for rabbitmq:

```docker
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

or for the google pubsub version you can run an emulator like

```docker
docker run --rm -ti -p 8681:8681 -e PUBSUB_PROJECT1=test-proj,topic1:subscription1 messagebird/gcloud-pubsub-emulator:latest     
```


[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/jkone27/AliceMQ/issues)

## Mailman (Producer)

Usage of a mailman is dead simple:

```cs
using AliceMQ.Core.Message;
using AliceMQ.Core.Types;
using AliceMQ.Rabbit.MailBox; 
using AliceMQ.Rabbit.Mailman; 
//for for g pubsub version: using AliceMQ.PubSub;
//parameters are slightly different..

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
```

Now let's see the simplest form of consumer, which is just a thin layer from the real MQ system...

## SimpleMailbox (Consumer of BasicDeliverEventArgs)

Consumer subscription is identical for every type, giving an istance of an IObservable<T> (rx).

```cs
using AliceMQ.Mailbox;

var mb = new SimpleMailbox(endPoint, sink);

using var d = mb.Subscribe(am =>
{
    Console.WriteLine("A - " + Encoding.UTF8.GetString(am.EventArgs.Body));
    am.Channel.BasicAck(am.EventArgs.DeliveryTag, false);
});

```

## Mailbox\<T> (Consumer of T)

let's consider an example DTO class Msg, the typed consumer is build upon the common consumer, which is enhanced with message body deserialization into an istance of a generic T type.

```cs
var sfm = new Mailbox<Msg>(endPoint, sink, s => JsonConvert.DeserializeObject<Msg>(s, serialization));

using var d = sfm.Subscribe(am =>
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
        am.Reject();
    }
},
ex => Console.WriteLine("COMPLETE ERROR"),
() => Console.WriteLine("COMPLETE"));
```

### Status
[![Build Status](https://img.shields.io/travis/jkone27/AliceMQ.svg)](https://travis-ci.org/jkone27/AliceMQ)
