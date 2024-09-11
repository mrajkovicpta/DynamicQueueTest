using Lib;
using Lib.Messages;
using MassTransit;

namespace ConsumerServiceOne;

public class StringMessageConsumer : IConsumer<StringMessage>
{
    private readonly MessageDbContext _dbContext;

    public StringMessageConsumer(MessageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public StringMessageConsumer() { }

    public async Task Consume(ConsumeContext<StringMessage> context)
    {
        _dbContext.Strings.Add(new()
        {
            StringValue = context.Message.StringValue,
            ServiceName = "ConsumerServiceOne"
        });
        await _dbContext.SaveChangesAsync();
    }
}
