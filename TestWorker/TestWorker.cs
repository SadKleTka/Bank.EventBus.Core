using System.Text;
using System.Text.Json;
using ClassLibrary;
using ClassLibrary.Models.RDto;
using RabbitMQ.Client;

namespace Bank.Client.Web.Test;
//Тестовая нагрузка на RabbitMq, а так же проверка prefetch count(qos)
public class TestWorker : BackgroundService
{
    private readonly ILogger<TestWorker> _logger;
    private readonly IRabbitMqConnectionProvider _connectionFactory;
    private IChannel _channel;
    private IConnection _connection;

    public TestWorker(IRabbitMqConnectionProvider connection, ILogger<TestWorker> logger)
    {
        _logger = logger;
        _connectionFactory = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TestWorker started");
        try
        {
            _connection = await _connectionFactory.GetConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            var test = new ClientRequestDto("Test", "1.0", "Swagger", null);
            var transaction = new ClientRequestDto("Transaction", "1.0", "Postman", null);
            var get = new ClientRequestDto("Get", "1.0", "Postman", null);

            var json1 = JsonSerializer.Serialize(test);
            var json2 = JsonSerializer.Serialize(transaction);
            var json3 = JsonSerializer.Serialize(get);
            var json4 = JsonSerializer.Serialize("transaction");

            var body1 = Encoding.UTF8.GetBytes(json1);
            var body2 = Encoding.UTF8.GetBytes(json2);
            var body3 = Encoding.UTF8.GetBytes(json3);
            var body4 = Encoding.UTF8.GetBytes(json4);


            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (_channel.IsOpen)
                {
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body1);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body2);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body3);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body4);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body4);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body4);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body4);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body4);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body4);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body4);
                    await _channel.BasicPublishAsync("bus.input.exchange", "bus.input.queue", body4);
                }
                else
                {
                    _logger.LogInformation("Channel is closed");
                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TestWorker error");
        }
        finally
        {
            await _channel.CloseAsync();
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}