# Configuring MassTransit to use Topic Exchanges with routing

## When to use this
This is best used when there are multiple services that want to read a specific message but due to the volume of messages, reading each one will throttle performance. For example, having numerous PLC readings wrapped into a single PLCReadMessage - we can seperate these reads into different comains and add routing keys to them to send them to an appropriate service.

## Prerequisites for running this project:

You should have a db file for SQLite to work at ```C:\SQLite\BusTest.db``` and a docker container for rabbitmq running with the port 5672 exposed and the default user. You can also enable management by exposing the port 15672 as well and running ```rabbitmq-plugins enable rabbitmq_management``` in the shell of the docker container.

## Creating a consumer

A consumer is created as usual. Simply create a class that implements the interface IConsumer<T> where T is the class/record of the message you want to listen to and process.
```csharp
public class NumberMessageConsumer : IConsumer<NumberMessage>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public NumberMessageConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Consume(ConsumeContext<NumberMessage> context)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MessageDbContext>();
        dbContext.Numbers.Add(new()
        {
            NumberValue = context.Message.Number,
            ServiceName = "ConsumerServiceOne"
        });
        await dbContext.SaveChangesAsync();
    }
}
```

## Creating a consumer definition and configuring a consumer

Next, we create a consumer definiton. This should be a class inheriting the ConsumerDefiniton<TConsumer> class where TConsumer is the conusmer we want to define. The consumer should have a constructor that sets the endpoint name to the name of the serivce or assembly which it is in and have a private constant string property which has the topic defition for the consumer. A topic defintion should be a series of words delimited by dots. The characters ```*``` and ```#``` are used to denote a **single** or **any amount** word(s) respectively and will match regardless of the word used in its place.

We should then override the ConfigureConsumer method to bind the message to the consumer like so:
```csharp
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<StringMessageConsumer> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ConcurrentMessageLimit = 1;
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
        {
            rmq.BindQueue = true;
            rmq.AutoDelete = true;
            rmq.Durable = true;
            rmq.Bind<StringMessage>((bindCfg) =>
            {
                bindCfg.RoutingKey = _topicDefiniton;
                bindCfg.ExchangeType = "topic";
            });
        }
    }
```
Note:
 - The endpointConfigurations ```rmq.AutoDelete = true;``` and ```rmq.Durable=true;``` should ***ONLY*** be set in the first consumer to be configured. When this is present in multiple consumer definitons, the program will fail on startup with an error saying the property has been modified after being used.
 - Additionally, configuring message bindings ```bindCfg.AutoDelete=true;``` and ```bindCfg.Durable=true;``` will fail and simply configure the message with defaults and additionally make any subsequent project fail to bind the messages to the queue. This will fail silently so beware.

 Then we have to add the consumer definition to MassTransit on startup and call ConfigureEndpoints on the RabbitMq configurator like so:
```csharp
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<StringMessageConsumer, StringMessageConsumerDefinition>((consumerCfg) => { });
    cfg.UsingRabbitMq((busContext, rabbitCfg) =>
    {
        var configuration = busContext.GetService<IConfiguration>();
        var rabbitConfig = configuration.GetSection("Rabbit").Get<RabbitConfig>();
        rabbitCfg.Host(rabbitConfig.Host, "/", h =>
        {
            h.Username(rabbitConfig.Username);
            h.Password(rabbitConfig.Password);
            rabbitCfg.ConfigureEndpoints(busContext);
        });
    });
});
```

## Publishing a message

In whatever project we intend to publish messages in we have to configure them as topic exchanges in the MassTransit configuration like so:
```csharp
builder.Services.AddMassTransit(cfg =>
{
    cfg.UsingRabbitMq((busContext, rabbitCfg) =>
    {
        var configuration = busContext.GetService<IConfiguration>();
        var rabbitConfig = configuration.GetSection("Rabbit").Get<RabbitConfig>();
        rabbitCfg.Host(rabbitConfig.Host, "/", h =>
        {
            h.Username(rabbitConfig.Username);
            h.Password(rabbitConfig.Password);
        });
        rabbitCfg.Publish<StringMessage>(cfg =>
        {
            cfg.ExchangeType = "topic";
        });
    });
});
```
And finally we will have to add the routing key to the message when sending it:
```csharp
var bus = scope.ServiceProvider.GetRequiredService<IBus>();
await bus.Publish(new StringMessage($"example string message"), (ctx) =>
{
    ctx.SetRoutingKey("your.routing.key.here");
} ,stoppingToken);
```
Of course, these routing keys are best kept in a seperate class as a constant or in a database.

