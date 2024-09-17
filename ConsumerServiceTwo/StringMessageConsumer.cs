using Lib;
using Lib.Messages;
using MassTransit;

namespace ConsumerServiceTwo;

public class StringMessageConsumer : IConsumer<StringMessage>
{
    private readonly MessageDbContext _dbContext;

    public StringMessageConsumer(MessageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<StringMessage> context)
    {
        _dbContext.Strings.Add(new()
        {
            StringValue = context.Message.StringValue,
            ServiceName = "ConsumerServiceTwo"
        });
        await _dbContext.SaveChangesAsync();
    }
}

public class StringMessageConsumerDefinition : ConsumerDefinition<StringMessageConsumer>
{
    readonly string _topicDefiniton;

    public StringMessageConsumerDefinition(string topicDefiniton = "two.string")
    {
        _topicDefiniton = topicDefiniton;
        EndpointName = "ConsumerServiceTwo";
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
            rmq.Bind<StringMessage>((bindCfg) =>
            {
                bindCfg.RoutingKey = _topicDefiniton;
                bindCfg.ExchangeType = "topic";
            });
        }
    }
}
