# AliceMQ

<img src="https://github.com/jkone27/AliceMQ/blob/master/Pics/Whiterabbit_tenniel.jpg?raw=true" width="20%" height="20%"/>

An easy to use frontend for MQ system (now supporting RabbitMq only, but would be nice to extend to other systems) using Reactive Extensions and a Publish/Subscribe paradigm.

*wiki under construction..*

## Mailman (Producer)

Usage of a mailman is dead simple:

```cs
using AliceMQ.MailMan; //..

var serialization = new JsonSerializerSettings
{
    MissingMemberHandling = MissingMemberHandling.Ignore,
    ContractResolver = new FromPascalToJsContractResolver()
};

var p = new Mailman(mailArgs, formatting: Formatting.Indented, jsonSerializerSettings: serialization);
p.PublishOne(new Msg("one"), "");
```

Now let's see the simplest form of consumer, which is just a thin layer from the real MQ system...

## Mailbox (Consumer)

Consumer subscription is identical for every type, with exception of AckObservingConsumer (who can request ack to messages, see later),
also every consumer is started and stopped the same simple way.

```cs
using AliceMQ.Mailbox;

var connectionFactoryParameters = new ConnectionFactoryParams();
var mailArgs = new MailArgs("A", "A.q");
var mailboxArgs = new MailboxArgs(mailArgs);

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
```

## ConfirmableMailbox (observable of ConfirmableMessage\<T\>)

the ConfirmableMailbox is build upon the typed consumer, and adds the ability to consume ConfirmableMessage (ackable) type messages, which have the ability to notify their observing consumer when a message wants to be accepted (acked) or rejected (nacked), decoupling message delivery confirmation from the consumer.

This can be done only if the original mailBox was enabled to give acks and nacks via its properties (otherwise an exception is readilly raised)

```cs
var confirmable = new ConfirmableMailbox<T>(custom);

confirmable.Subscribe(cm =>
{
    //OnNext
    //...
    cm.Accept(); //or cm.Reject();
    //...
}, OnError, OnComplete);
```

The class T is here internally wrapped within a confirmable container class, so that one can request one of its instances to confirm message delivery by either accepting or rejecting the message, requesting the observer of IConfirmableMessage (which is internal to ConfirmableMailbox) to ack or nack to its source.
Message confirmation (ack or nack) can be done only once, otherwise a specific esception is thrown as runtime. Observed wrapped messages all implement the following interface:


```cs
public interface IConfirmableMessage
{
    void Accept(bool multiple = false);
    void Reject(bool multiple = false, bool requeue = false);
}
```