using Bank.Client.Web.Test;
using ClassLibrary;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
Log.Logger = new LoggerConfiguration() 
    .ReadFrom.Configuration(builder.Configuration) 
    
    .Enrich.FromLogContext() 
    
    .WriteTo.Console() 
    
    .CreateLogger(); 

builder.Services.AddSerilog();

builder.Services.AddSingleton<RabbitMqConnectionProvider>();
builder.Services.AddHostedService<TestWorker>();

var host = builder.Build();
host.Run();