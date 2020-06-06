namespace Alice.Domain

type Provider = RabbitMq | GooglePubSub

type Message = Received | Error

type Topic = unit //exchange

type Subscription = unit //queue