using ConsumerServiceTwo;
using Lib;
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
    cfg.UsingRabbitMq((busContext, rabbitCfg) =>
    {
        var configuration = busContext.GetService<IConfiguration>();
        var rabbitConfig = configuration.GetSection("Rabbit").Get<RabbitConfig>();
        rabbitCfg.Host(rabbitConfig.Host, "/", h =>
        {
            h.Username(rabbitConfig.Username);
            h.Password(rabbitConfig.Password);
        });

        rabbitCfg.ReceiveEndpoint("ConsumerServiceTwo", endpoint =>
        {
            endpoint.AutoDelete = true;
            endpoint.Durable = true;
            endpoint.Consumer<StringMessageConsumer>(()=> new StringMessageConsumer(busContext.GetRequiredService<IServiceScopeFactory>()));
            endpoint.Consumer<NumberMessageConsumer>(()=> new NumberMessageConsumer(busContext.GetRequiredService<IServiceScopeFactory>()));
        });
    });
});

var host = builder.Build();
host.Run();
