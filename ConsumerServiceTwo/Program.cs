using ConsumerServiceTwo;
using Lib;
using Lib.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<MessageDbContext> ((provider, optionsBuilder) =>
{
    optionsBuilder.UseSqlite(provider.GetService<IConfiguration>().GetConnectionString("Default"));
});

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<StringMessageConsumer, StringMessageConsumerDefinition>((consumerCfg) => { });
    cfg.AddConsumer<NumberMessageConsumer, NumberMessageConsumerDefinition>((consumerCfg) => { });
    cfg.UsingRabbitMq((busContext, rabbitCfg) =>
    {
        var configuration = busContext.GetService<IConfiguration>();
        var rabbitConfig = configuration.GetSection("Rabbit").Get<RabbitConfig>();
        rabbitCfg.Host(rabbitConfig.Host, "/", h =>
        {
            h.Username(rabbitConfig.Username);
            h.Password(rabbitConfig.Password);
            rabbitCfg.ConfigureEndpoints(busContext);
        });
    });
});

var host = builder.Build();
host.Run();
