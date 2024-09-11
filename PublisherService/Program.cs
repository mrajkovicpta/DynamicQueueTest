using Lib;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PublisherService;

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
    });
});

var host = builder.Build();
host.Run();
