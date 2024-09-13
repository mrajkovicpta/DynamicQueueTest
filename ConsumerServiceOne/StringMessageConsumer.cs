﻿using Lib;
using Lib.Messages;
using MassTransit;
using MassTransit.Transports.Fabric;

namespace ConsumerServiceOne;

public class StringMessageConsumer : IConsumer<StringMessage>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public StringMessageConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Consume(ConsumeContext<StringMessage> context)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MessageDbContext>();
        dbContext.Strings.Add(new()
        {
            StringValue = context.Message.StringValue,
            ServiceName = "ConsumerServiceOne"
        });
        await dbContext.SaveChangesAsync();
    }

}

public class StringMessageConsumerDefinition : ConsumerDefinition<StringMessageConsumer>
{
    readonly string _topicDefiniton;

    public StringMessageConsumerDefinition(string topicDefiniton = "one.string")
    {
        _topicDefiniton = topicDefiniton;
        EndpointName = "ConsumerServiceOne";
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<StringMessageConsumer> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ConcurrentMessageLimit = 1;
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
        {
            rmq.BindQueue = true;
            rmq.AutoDelete = true;
            rmq.Durable = true;
            rmq.Consumer<StringMessageConsumer>(() => new StringMessageConsumer(context.GetService<IServiceScopeFactory>()));
            rmq.Bind<StringMessage>((bindCfg) =>
            {
                bindCfg.RoutingKey = _topicDefiniton;
                bindCfg.ExchangeType = "topic";
            });
        }
    }
}
