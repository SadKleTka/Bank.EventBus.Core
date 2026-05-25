using Bank.Client.Web.Data;
using Bank.Client.Web.Services;
using ClassLibrary;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration() 
    .ReadFrom.Configuration(builder.Configuration) 
    
    .Enrich.FromLogContext() 
    
    .WriteTo.Console() 
    
    .CreateLogger(); 

builder.Host.UseSerilog();



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(); 

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<RabbitMqConnectionProvider>();
builder.Services.AddScoped<IClientService, ClientService>();


var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}

app.MapControllers();
app.Run();