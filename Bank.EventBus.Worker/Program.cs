using Bank.EventBus.BusWorker;
using Bank.EventBus.RedisRefresh;
using Bank.EventBus.Worker.Data;
using ClassLibrary;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);


Log.Logger = new LoggerConfiguration() 
    .ReadFrom.Configuration(builder.Configuration) 
    
    .Enrich.FromLogContext() 
    
    .WriteTo.Console() 
    
    .CreateLogger(); 

builder.Services.AddSerilog();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("100.122.221.84:6379, abortConnect=false"));
builder.Services.AddHostedService<RedisRefresh>();
builder.Services.AddSingleton<RabbitMqConnectionProvider>();
builder.Services.AddHostedService<BusWorker>();

var host = builder.Build();
host.Run();