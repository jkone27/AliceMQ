# AliceMQ

<img src="https://github.com/jkone27/AliceMQ/blob/master/Pics/Whiterabbit_tenniel.jpg?raw=true" width="20%" height="20%"/>

An easy to use frontend for MQ system (now supporting RabbitMq only, but would be nice to extend to other systems) using Reactive Extensions and a Publish/Subscribe paradigm.

[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/jkone27/AliceMQ/issues)

## Mailman (Producer)

Usage of a mailman is dead simple:

```cs
using AliceMQ.MailMan; //..

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

var d = mb.Subscribe(am =>
{
    Console.WriteLine("A - " + Encoding.UTF8.GetString(am.EventArgs.Body));
    am.Channel.BasicAck(am.EventArgs.DeliveryTag, false);
});

//...
d.Dispose();

```

## Mailbox\<T> (Consumer of T)

let's consider an example DTO class Msg, the typed consumer is build upon the common consumer, which is enhanced with message body deserialization into an istance of a generic T type.

```cs
var sfm = new Mailbox<Msg>(endPoint, sink, s => JsonConvert.DeserializeObject<Msg>(s, serialization));

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
        am.Reject();
    }
},
ex => Console.WriteLine("COMPLETE ERROR"),
() => Console.WriteLine("COMPLETE"));

//...
d.Dispose();
```

## Utility Types

Both Mailman and Mailbox need that you provide some basic parameters for configuring the Endpoint, the Source (namely Exchange and Queue), and the Mailbox (with more sofisticated configurations).

### EndpointArgs

```cs
string ConnectionUrl
bool AutomaticRecoveryEnabled
TimeSpan NetworkRecoveryInterval
```

### Source

```cs
IExchange Exchange
IQueueArgs QueueArgs
```

### IExchange

```cs
string ExchangeName
string ExchangeType
bool Durable
bool AutoDelete
IDictionary<string, object> Properties
```

### IQueueArgs

```cs
string QueueName
bool Durable
bool Exclusive
bool AutoDelete
```

### Sink

```cs
string DeadLetterExchangeName
IDictionary<string, object> QueueDeclareArguments
Source Source
BasicQualityOfService BasicQualityOfService
ConfirmationPolicy ConfirmationPolicy 
QueueBind QueueBind
```

### QueueBind

```cs
string RoutingKey
IDictionary<string, object> Arguments
```

### BasicQualityOfService

```cs
ushort PrefetchCount
bool Global
```

### ConfirmationPolicy

```cs
bool AutoAck
bool Multiple
bool Requeue
```

### Status
[![Build Status](https://img.shields.io/travis/jkone27/AliceMQ.svg)](https://travis-ci.org/jkone27/AliceMQ)