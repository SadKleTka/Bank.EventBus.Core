using Bank.Client.Web.Test;
using ClassLibrary;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<RabbitMqConnectionProvider>();
builder.Services.AddHostedService<TestWorker>();

var host = builder.Build();
host.Run();