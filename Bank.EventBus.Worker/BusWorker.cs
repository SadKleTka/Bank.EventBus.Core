using System.Text;
using System.Text.Json;
using Bank.EventBus.Worker.Models;
using Bank.EventBus.Worker.Models.Dto;
using ClassLibrary;
using ClassLibrary.Models.RDto;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace Bank.EventBus.BusWorker;

public class BusWorker : BackgroundService
{
    
    private readonly ILogger<BusWorker> _logger;
    private readonly IDatabase _redis;
    private readonly IRabbitMqConnectionProvider _connection;

    public BusWorker(IRabbitMqConnectionProvider connection, ILogger<BusWorker> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
        _connection = connection;
    }
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting client service worker");
        try
        {
            string cacheCollectionsOperationsKey = "bank:cache:busCollectionsOperations";
            string cacheBusOperationsKey = "bank:cache:busOperations";
            string cacheCollectionsKey = "bank:cache:collections";
            List<BusOperations>? operations = null;
            List<BusCollectionsOperations>? collectionsOperations = null;
            List<Collections>? collections = null;
            
            operations = await GetFromCacheAsync<BusOperations>(cacheBusOperationsKey);
            collections = await GetFromCacheAsync<Collections>(cacheCollectionsKey);
            collectionsOperations =
                await GetFromCacheAsync<BusCollectionsOperations>(cacheCollectionsOperationsKey); 
            _ = Task.Run(async () =>
            {
                using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10.1));
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        operations = await GetFromCacheAsync<BusOperations>(cacheBusOperationsKey);
                        collections = await GetFromCacheAsync<Collections>(cacheCollectionsKey);
                        collectionsOperations =
                            await GetFromCacheAsync<BusCollectionsOperations>(cacheCollectionsOperationsKey);

                        _logger.LogInformation("Local cache has been updated");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error ocured while local cache has been updated");
                    }
                }
            }, stoppingToken);
            
            
            var connection = await _connection.GetConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false);
            
            var consumer = new AsyncEventingBasicConsumer(channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var property = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var clientRequest = JsonSerializer.Deserialize<ClientRequestDto>(json, property);

                    var operation =
                        operations?.FirstOrDefault(x => x.Type == clientRequest?.Type &&
                                                        x.Version == clientRequest?.Version);
              
                    var find =
                        collectionsOperations?.FirstOrDefault(x => x.BusOperationId == operation?.Id);
               
                    var collection = collections?.FirstOrDefault(x => x.Id == find?.CollectionId);
                    
                    if (operation is null)
                    {
                        throw new ArgumentException("Config missing. Routing to error queue.");
                    }

                    if (find is null)
                    {
                        await channel.BasicPublishAsync(
                            exchange: "",
                            routingKey: "bus.default.queue",
                            body: ea.Body);

                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Message has been sent to default queue");
                    }
                    else
                    {
                        if (collection is null)
                        {
                            throw new ArgumentException("Config missing. Routing to error queue.");
                        }

                        await channel.BasicPublishAsync(
                            exchange: collection.ExchangeName,
                            routingKey: collection.RoutingKey,
                            body: ea.Body);


                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Message has been sent");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing message");
                    await channel.BasicPublishAsync(
                        exchange: "bus.error.exchange",
                        routingKey: "bus.error.queue",
                        body: ea.Body);

                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await channel.BasicConsumeAsync("bus.input.queue", autoAck: false, consumer: consumer);
            
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Global ERROR!!!");
        }
        
    }
    
    private async Task<List<T>?> GetFromCacheAsync<T>(string cacheKey)
    {
        string? cachedData = await _redis.StringGetAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<List<T>>(cachedData);
        }
        throw new ArgumentException("Something is wrong with redis data");
    }
}