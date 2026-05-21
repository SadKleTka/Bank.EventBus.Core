using ClassLibrary;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace Bank.EventBus.RedisRefresh;

public class BusWorker : BackgroundService
{
    
    private readonly ILogger<BusWorker> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly RabbitMqConnectionProvider _connection;

    public BusWorker(RabbitMqConnectionProvider connection, ILogger<BusWorker> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
        _connection = connection;
    }
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting client service worker");
        try
        {
            var connection = await _connection.GetConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            var consumer = new AsyncEventingBasicConsumer(channel);
            
            
            
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while starting client service");
        }
        
    }
}