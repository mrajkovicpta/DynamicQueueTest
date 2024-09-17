using System.Text;
using System.Text.Unicode;
using Lib.Messages;
using MassTransit;

namespace PublisherService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        while (!stoppingToken.IsCancellationRequested)
        {
            var number = Random.Shared.Next();
            var bytes = new byte[10];
            Random.Shared.NextBytes(bytes);
            var randString = Encoding.UTF8.GetString(bytes);
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publishEndpoint.Publish(new StringMessage($"one{number}"), (ctx) =>
            {
                ctx.SetRoutingKey("one.string");
            } ,stoppingToken);
            await publishEndpoint.Publish(new StringMessage($"two{number}"), (ctx) =>
            {
                ctx.SetRoutingKey("two.string");
            },stoppingToken);
            await publishEndpoint.Publish(new NumberMessage(number), (ctx) =>
            {
                ctx.SetRoutingKey("one.number");
            } ,stoppingToken);
            await publishEndpoint.Publish(new NumberMessage(number), (ctx) =>
            {
                ctx.SetRoutingKey("two.number");
            },stoppingToken);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(10000, stoppingToken);
        }
    }
}
