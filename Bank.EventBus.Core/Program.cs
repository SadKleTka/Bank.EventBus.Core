using Bank.EventBus.Core.Data;
using Bank.EventBus.Core.Service;
using ClassLibrary;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Bank.EventBus.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(); 

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<RabbitMqConnectionProvider>();
builder.Services.AddScoped<IBusService, BusService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}

app.MapControllers();
app.Run();