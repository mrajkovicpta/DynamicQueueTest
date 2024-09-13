using Lib;
using Lib.Messages;
using MassTransit;

namespace ConsumerServiceTwo;

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

public class NumberMessageConsumerDefinition : ConsumerDefinition<NumberMessageConsumer>
{
    readonly string _topicDefiniton;

    public NumberMessageConsumerDefinition(string topicDefiniton = "two.number")
    {
        _topicDefiniton = topicDefiniton;
        EndpointName = "ConsumerServiceTwo";
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<NumberMessageConsumer> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ConcurrentMessageLimit = 1;
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
        {
            rmq.BindQueue = true;
            rmq.Consumer<NumberMessageConsumer>(() => new NumberMessageConsumer(context.GetService<IServiceScopeFactory>()));
            rmq.Bind<NumberMessage>((bindCfg) =>
            {
                bindCfg.RoutingKey = _topicDefiniton;
                bindCfg.AutoDelete = true;
                bindCfg.Durable = true;
                bindCfg.ExchangeType = "topic";
            });
        }
    }
}
