using Lib;
using Lib.Messages;
using MassTransit;

namespace ConsumerServiceTwo;

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
