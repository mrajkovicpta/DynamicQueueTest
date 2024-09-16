using Lib;
using Lib.Messages;
using MassTransit;
using MassTransit.Transports.Fabric;

namespace ConsumerServiceOne;

public class StringMessageConsumer : IConsumer<StringMessage>
{
    private readonly MessageDbContext  _dbContext;

    public StringMessageConsumer(MessageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<StringMessage> context)
    {
        Console.WriteLine($"{context.RoutingKey()}");
        Console.WriteLine($"{context.MessageId}");
        _dbContext.Strings.Add(new()
        {
            StringValue = context.Message.StringValue,
            ServiceName = "ConsumerServiceOne"
        });
        await _dbContext.SaveChangesAsync();
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
       endpointConfigurator.ConcurrentMessageLimit = 1;
       endpointConfigurator.ConfigureConsumeTopology = false;
       if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
       {
           rmq.BindQueue = true;
           rmq.AutoDelete = true;
           rmq.Durable = true;

            rmq.Bind<StringMessage>((bindCfg) =>
            {
                bindCfg.Durable = true;
                bindCfg.AutoDelete = true;
                bindCfg.RoutingKey = _topicDefiniton;
                bindCfg.ExchangeType = "topic";
            });
       }
    }
}
