using Lib;
using Lib.Messages;
using MassTransit;

namespace ConsumerServiceOne;

public class NumberMessageConsumer : IConsumer<NumberMessage>
{
    private readonly MessageDbContext _dbContext;

    public NumberMessageConsumer(MessageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<NumberMessage> context)
    {
        _dbContext.Numbers.Add(new()
        {
            NumberValue = context.Message.Number,
            ServiceName = "ConsumerServiceOne"
        });
        await _dbContext.SaveChangesAsync();
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
                bindCfg.Durable = true;
                bindCfg.AutoDelete = true;
                bindCfg.RoutingKey = _topicDefiniton;
                bindCfg.ExchangeType = "topic";
            });
        }
    }
}
