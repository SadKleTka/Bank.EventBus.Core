using Bank.EventBus.Worker;
using Bank.EventBus.Worker.Data;
using ClassLibrary;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<RabbitMqConnectionProvider>();

var host = builder.Build();
host.Run();