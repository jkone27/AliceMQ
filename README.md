# AliceMQ

<img src="https://github.com/jkone27/AliceMQ/blob/master/Pics/Whiterabbit_tenniel.jpg?raw=true" width="20%" height="20%"/>

An easy to use frontend for MQ system (now supporting RabbitMq only, but would be nice to extend to other systems) using Reactive Extensions and a Publish/Subscribe paradigm.

[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/jkone27/AliceMQ/issues)

## Mailman (Producer)

Usage of a mailman is dead simple:

```cs
using AliceMQ.MailMan; //..

var endpointArgs = new EndpointArgs();
var sourceArgs = new SourceArgs("A", "A.q");

var serialization = new JsonSerializerSettings
{
    MissingMemberHandling = MissingMemberHandling.Ignore,
    ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
};

var p = new Mailman(sourceArgs.ExchangeArgs, s => JsonConvert.SerializeObject(s, serialization));
p.PublishOne(new Msg("one"), "");
```

Now let's see the simplest form of consumer, which is just a thin layer from the real MQ system...

## Mailbox (Consumer)

Consumer subscription is identical for every type,
giving an istance of an IConnectableObservable<T>, which must be started after subscriptions invoking its Connect() method (and disposing that after usage).

```cs
using AliceMQ.Mailbox;

var connectionFactoryParameters = new ConnectionFactoryParams();
var mailboxArgs = new MailboxArgs(sourceArgs);

var mb = new MailBox(connectionFactoryParameters, mailboxArgs, autoAck: false);
mb.Subscribe(OnNext, OnError, OnComplete);
var d = mb.Connect();
//...
d.Dispose();
```

## CustomMailBox (Typed Consumer)

let's consider an example DTO class Msg, the typed consumer is build upon the common consumer, which is enhanced with message body deserialization into an istance of a generic T type.

```cs
var serialization = new JsonSerializerSettings
{
    MissingMemberHandling = MissingMemberHandling.Ignore,
    ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
};

var custom = new CustomMailBox<Msg>(mb, s => JsonConvert.DeserializeObject<Msg>(s, serialization));

custom.Subscribe(am =>
{
    if (am.IsOk<Msg>())
        Console.WriteLine("ok - " + am.AsOk<Msg>().Message.Bla);
    else
        Console.WriteLine("error - " + am.AsError().Ex);
});
var d = custom.Connect();
//...
d.Dispose();
```

## ConfirmableMailbox (observable of ConfirmableEnvelope)

the ConfirmableMailbox is build upon the typed consumer, and adds the ability to consume ConfirmableEnvelope<T> (ackable) messages, which have the ability to notify their observing consumer when a message wants to be accepted (acked) or rejected (nacked), decoupling message delivery confirmation from the consumer.

```cs
var confirmable = new ConfirmableMailbox(custom);

confirmable.Subscribe(am =>
{
    if (am.IsOk())
    {
        Console.WriteLine("C - " + am.Content().Bla);
        am.Accept();
    }
    else
    {
        Console.WriteLine("C - error." + am.Exception());
        am.Reject();
    }
});
var d = confirmable.Connect();
//...
d.Dispose();
```

The IConvirmableEnvelope<T> istances also implement the IConfirmableMessage interface, providing a common abstraction for accepting (ack) or rejecting (nack) messages from the queue.


```cs
public interface IConfirmableMessage
{
    void Accept(bool multiple = false);
    void Reject(bool multiple = false, bool requeue = false);
}
```

## Mailbox and Mailman Args

Both Mailman and Mailbox need that you provide some basic parameters for configuring the Endpoint, the Source (namely Exchange and Queue), and the Mailbox (with more sofisticated configurations).

### EndpointArgs

```cs
string HostName
int Port
string UserName
string Password
string VirtualHost
bool AutomaticRecoveryEnabled
TimeSpan NetworkRecoveryInterval
```

### SourceArgs

```cs
ExchangeArgs ExchangeArgs
QueueArgs QueueArgs
```

### ExchangeArgs

```cs
string ExchangeName
string ExchangeType
bool Durable
bool AutoDelete
IDictionary<string, object> Properties
```

### QueueArgs

```cs
string QueueName
bool Durable
bool Exclusive
bool AutoDelete
```

### MailboxArgs

```cs
string DeadLetterExchangeName
IDictionary<string, object> QueueDeclareArguments
SourceArgs Source
BasicQualityOfService BasicQualityOfService
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

### Status
[![Build Status](https://img.shields.io/travis/jkone27/AliceMQ.svg)](https://travis-ci.org/jkone27/AliceMQ)