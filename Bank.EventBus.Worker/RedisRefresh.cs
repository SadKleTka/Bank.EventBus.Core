using System.Text.Json;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Bank.EventBus.Worker.Data;


namespace Bank.EventBus.RedisRefresh;

public class RedisRefresh : BackgroundService
{
    private readonly ILogger<RedisRefresh> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;

    public RedisRefresh(ILogger<RedisRefresh> logger, IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _redis = redis;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис RedisRefresh запущен.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

        await DoWorkAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await DoWorkAsync(stoppingToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Начинаем выгрузку данных в Redis...");

            using var scope = _scopeFactory.CreateScope();

            
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            

            var busCollectionsOperationsList = await dbContext.BusCollectionsOperations.AsNoTracking().ToListAsync(cancellationToken);
            var busOps = await dbContext.Operations.AsNoTracking().ToListAsync(cancellationToken);
            var collections = await dbContext.Collections.AsNoTracking().ToListAsync(cancellationToken);

            var db = _redis.GetDatabase();

            
            await db.StringSetAsync("bank:cache:busCollectionsOperations", JsonSerializer.Serialize(busCollectionsOperationsList));
            await db.StringSetAsync("bank:cache:busOperations", JsonSerializer.Serialize(busOps));
            await db.StringSetAsync("bank:cache:collections", JsonSerializer.Serialize(collections));

            _logger.LogInformation("Данные успешно обновлены в Redis.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении кэша Redis.");
        }
    }
}