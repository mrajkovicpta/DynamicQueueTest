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

    public NumberMessageConsumer() { }

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
