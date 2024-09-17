using Lib;
using Lib.Messages;
using MassTransit;

namespace ConsumerServiceTwo;

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
            ServiceName = "ConsumerServiceTwo"
        });
        await _dbContext.SaveChangesAsync();
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
            rmq.Bind<NumberMessage>((bindCfg) =>
            {
                rmq.AutoDelete = true;
                rmq.Durable = true;
                bindCfg.RoutingKey = _topicDefiniton;
                bindCfg.ExchangeType = "topic";
            });
        }
    }
}
