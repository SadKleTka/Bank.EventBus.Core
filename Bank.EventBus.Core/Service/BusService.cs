using Bank.EventBus.Core.Data;
using Bank.EventBus.Worker.Models;
using Bank.EventBus.Worker.Models.Dto;
using ClassLibrary;
using ClassLibrary.Models.RDto;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace Bank.EventBus.Core.Service;

public class BusService : IBusService
{
    private readonly AppDbContext _context;
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly ILogger<BusService> _logger;
    
    public BusService(AppDbContext context, IRabbitMqConnectionProvider connectionProvider, ILogger<BusService> logger)
    {
        _context = context;
        _connectionProvider = connectionProvider;
        _logger = logger;
    }
    //Добавил возможность совмещать их в промежуточной таблице
    public async Task<Message> Bind(BindCollectionOperation? bind)
    {
        if (bind is null)
            return new Message("Bind cannot be null", DateTime.UtcNow);

        try
        {
            var collection =
                _context.Collections
                    .Include(o => o.Operations)
                    .FirstOrDefault(c => c.Id.ToString() == bind.CollectionId);
            if (collection is null)
                throw new ArgumentNullException($"{nameof(bind.CollectionId)} is not exist");
            
            var operation = 
                _context.Operations
                    .Include(o => o.Collections)
                    .FirstOrDefault(o => o.Id.ToString() == bind.OperationId);
            if (operation is null)
                throw new ArgumentNullException($"{nameof(bind.OperationId)} is not exist");
            
            var oldBind = await _context.BusCollectionsOperations
                .AnyAsync(o => o.BusOperationId == operation.Id && o.CollectionId == collection.Id);
            if (oldBind)
                throw new InvalidOperationException($"{nameof(oldBind)} is already exist");

            var newBind = new BusCollectionsOperations(
                    DateTime.UtcNow, 
                    operation.Id,
                    collection.Id);
            
            operation.Collections.Add(newBind);
            collection.Operations.Add(newBind);
            
            await _context.BusCollectionsOperations.AddAsync(newBind);
            await _context.SaveChangesAsync();
            return new Message("success", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return new Message(ex.Message, DateTime.UtcNow);
        }
    }
    
    //Получить все операции для просмотра
    public List<OperationToAnswer> GetAllOperations()
    {
         var operations = _context.Operations.ToList();
          
         return OperationMapper(operations);
    }
    
    //Получить все коллекции для просмотра
    public List<CollectionToAnswer> GetAllCollections()
    {
        var collections = _context.Collections.ToList();
          
        return CollectionMapper(collections);
    }

    //Создание новых операций
    public async Task<Message> CreateOperation(OperationToCreate? operation)
    {
        if (operation is null)
            return new Message("Operation cannot be null", DateTime.UtcNow);
        try
        {
            var checkOperation = _context.Operations.Any(o => o.Type == operation.Type);
            if (checkOperation)
                throw new ArgumentException($"{operation.Type} is already exist");

            var newOperation = new BusOperations(operation.Type, operation.Version, operation.Description);
            
            await _context.Operations.AddAsync(newOperation);
            await _context.SaveChangesAsync();
            
            return new Message("success", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return new Message(ex.Message, DateTime.UtcNow);
        }
    }
    
    //Добавление коллекций в бд
    public async Task<Message> CreateCollection(CollectionToCreate? collection)
    {
        if (collection is null)
            return new Message("Collection cannot be nulll", DateTime.UtcNow);
        try
        {
            var checkCollection = _context.Collections.Any(c => c.ExchangeName == collection.ExchangeName);
            if (checkCollection)
            {
                throw new ArgumentException($"{collection.Title} is already created");
            }

            var newCollection = new Collections(
                collection.Title,
                collection.Description,
                collection.ExchangeName,
                collection.QueueName,
                collection.RouteKey);
            
            var connection = await _connectionProvider.GetConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            await channel.ExchangeDeclareAsync(collection.ExchangeName, ExchangeType.Direct);
            await channel.QueueDeclareAsync(collection.QueueName, true, false, false);
            await channel.QueueBindAsync(collection.QueueName, collection.ExchangeName, collection.RouteKey);
            
            await _context.Collections.AddAsync(newCollection);
            await _context.SaveChangesAsync();
            
            return new Message("success", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return new Message(ex.Message, DateTime.UtcNow);
        }
    }

    private static List<OperationToAnswer> OperationMapper(List<BusOperations> operations)
    {
        List<OperationToAnswer> list = new List<OperationToAnswer>();
        foreach (var operation in operations)
        {
            list.Add(new OperationToAnswer(
                operation.Id,
                operation.Type,
                operation.Version,
                operation.Description));
        }
        return list;
    }

    private static List<CollectionToAnswer> CollectionMapper(List<Collections> collections)
    {
        List<CollectionToAnswer> list = new List<CollectionToAnswer>();
        foreach (var collection in collections)
        {
            list.Add(new CollectionToAnswer(
                collection.Id,
                collection.Title,
                collection.Description,
                collection.ExchangeName,
                collection.QueueName,
                collection.RoutingKey));
        }
        return list;
    }
}
