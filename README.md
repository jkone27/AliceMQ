# AliceMQ

<img src="https://github.com/jkone27/AliceMQ/blob/master/Pics/Whiterabbit_tenniel.jpg?raw=true" width="20%" height="20%"/>

An easy to use frontend for MQ system (now supporting RabbitMq only, but would be nice to extend to other systems) using Reactive Extensions and a Publish/Subscribe paradigm.

## Mailman (Producer)

Usage of a mailman is dead simple:

```cs
using AliceMQ.MailMan; //..

var endpointArgs = new EndpointArgs();
var sourceArgs = new SourceArgs("A", "A.q");

var serialization = new JsonSerializerSettings
{
    MissingMemberHandling = MissingMemberHandling.Ignore,
    ContractResolver = new FromPascalToJsContractResolver()
};

var p = new Mailman(sourceArgs, formatting: Formatting.Indented, jsonSerializerSettings: serialization);
p.PublishOne(new Msg("one"), "");
```

Now let's see the simplest form of consumer, which is just a thin layer from the real MQ system...

## Mailbox (Consumer)

Consumer subscription is identical for every type, with exception of AckObservingConsumer (who can request ack to messages, see later),
also every consumer is started and stopped the same simple way.

```cs
using AliceMQ.Mailbox;

var connectionFactoryParameters = new ConnectionFactoryParams();
var mailboxArgs = new MailboxArgs(sourceArgs);

var mb = new MailBox(connectionFactoryParameters, mailboxArgs, autoAck: false);
mb.Subscribe(OnNext, OnError, OnComplete);
```

## CustomMailBox (Typed Consumer)

let's consider an example DTO class Msg, the typed consumer is build upon the common consumer.
Added functionality is just being able to properly deserialize the message from json string.

```cs
var serialization = new JsonSerializerSettings
{
    MissingMemberHandling = MissingMemberHandling.Ignore,
    ContractResolver = new FromPascalToJsContractResolver()
};

var custom = new CustomMailBox<Msg>(mb, serialization);

custom.Subscribe(am =>
{
    if (am.IsOk<Msg>())
    {
        Console.WriteLine("ok - " + am.AsOk<Msg>().Message.Bla);
    }
    else
    {
        Console.WriteLine("error - " + am.AsError().Ex);
    }
});
```

## ConfirmableMailbox (observable of ConfirmableEnvelope)

the ConfirmableMailbox is build upon the typed consumer, and adds the ability to consume ConfirmableEnvelope (ackable) messages, which have the ability to notify their observing consumer when a message wants to be accepted (acked) or rejected (nacked), decoupling message delivery confirmation from the consumer.

This can be done only if the original mailBox was enabled to give acks and nacks via its properties (otherwise an exception is readilly raised)

```cs
var confirmable = new ConfirmableMailbox(custom);

confirmable.Subscribe(cm =>
{
    if (am.Content.IsOk<Msg>())
    {
        Console.WriteLine("ok - " + am.Content.AsOk<Msg>().Message?.Bla);
        am.Accept();
    }
    else
    {
        Console.WriteLine("error - " + am.Content.AsError().Ex);
        am.Reject();
    }
}, OnError, OnComplete);
```

The class T is here internally wrapped within a confirmable container class, so that one can request one of its instances to confirm message delivery by either accepting or rejecting the message, requesting the observer of IConfirmableMessage (which is internal to ConfirmableMailbox) to ack or nack to its source.
Message confirmation (ack or nack) can be done only once, otherwise a specific esception is thrown as runtime. Observed ConfirmableEnvelope messages all implement the following interface:


```cs
public interface IConfirmableMessage
{
    void Accept(bool multiple = false);
    void Reject(bool multiple = false, bool requeue = false);
}
```

## Arguments

Both Mailman and Mailbox need that you provide some basic parameters for configuring the Endpoint, the Source (namely Exchange and Queue), and the Mailbox (with more sofisticated configurations).

#### EndpointArgs

```cs

public string HostName { get; }
public int Port { get; }
public string UserName { get; }
public string Password { get; }
public string VirtualHost { get; }
public bool AutomaticRecoveryEnabled { get;}
public TimeSpan NetworkRecoveryInterval { get;  }
```

#### SourceArgs

```cs
public ExchangeArgs ExchangeArgs { get; }
public QueueArgs QueueArgs { get; }
```

#### ExchangeArgs

```cs 
public string ExchangeName { get; set; }
public string ExchangeType { get; set; }
public bool Durable { get; set; }
public IDictionary<string, object> Properties { get; set; }
```

#### QueueArgs

```cs
public string QueueName { get; }
public bool Durable { get; }
public bool Exclusive { get; }
public bool AutoDelete { get; }
```

#### MailboxArgs

```cs
public string DeadLetterExchangeName { get; set; }
public IDictionary<string, object> QueueDeclareArguments { get; set; } //must be settable at runtime
public SourceArgs Source { get; set; }
public BasicQualityOfService BasicQualityOfService { get; }
public QueueBind QueueBind { get; }
```

#### QueueBind

```cs
public string RoutingKey { get; }
public IDictionary<string, object> Arguments { get; }
```

#### BasicQualityOfService

```cs
public ushort PrefetchCount { get; set; }
public bool Global { get; set; }
```

## TODO

- ~~Fluent Builder / Factory Interface for MailBox and Mailman (abandoned)~~
- Write more documentation for configuration classes (MailArgs, and MailBoxArgs)
- More Exaustive names