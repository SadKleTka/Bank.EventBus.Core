using Bank.EventBus.Worker;
using Bank.EventBus.Worker.Data;
using ClassLibrary;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("100.122.221.84:6379, abortConnect=false"));
builder.Services.AddHostedService<RedisRefresh>();
builder.Services.AddSingleton<RabbitMqConnectionProvider>();

var host = builder.Build();
host.Run();