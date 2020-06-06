namespace Alice.Domain

type Provider = RabbitMq | GooglePubSub

type Message<'a> = 
    | Received of 'a
    | Error of string

type Topic = 
    { ConnectionString: string }

type Subscription = unit //queue

type IPublisher = 
    abstract member Topic : Topic   
    abstract member PublishAsync : Message<'a> -> unit

type ISubscriber = 
    abstract member Topic : Topic   
    abstract member HandleAsync : Message<'a> -> unit
