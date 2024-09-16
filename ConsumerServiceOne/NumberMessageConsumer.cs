using Lib;
using Lib.Messages;
using MassTransit;

namespace ConsumerServiceOne;

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
    private const string _topicDefiniton = "one.number";

    public NumberMessageConsumerDefinition()
    {
        EndpointName = "ConsumerServiceOne";
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<NumberMessageConsumer> consumerConfigurator, IRegistrationContext context)
    {
        endpointConfigurator.ConfigureConsumeTopology = false;
        endpointConfigurator.ConcurrentMessageLimit = 1;
        if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
        {
            rmq.BindQueue = true;
            rmq.Bind<NumberMessage>((bindCfg) =>
            {
                bindCfg.RoutingKey = _topicDefiniton;
                bindCfg.ExchangeType = "topic";
            });
        }
    }
}
