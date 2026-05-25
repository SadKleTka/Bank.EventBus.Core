using Bank.EventBus.Core.Service;
using Bank.EventBus.Core.Data;
using Bank.EventBus.Worker.Models;
using Bank.EventBus.Worker.Models.Dto;
using ClassLibrary;
using ClassLibrary.Models.RDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;

namespace Bank.EventBus.Core.Test;

[TestClass]
public sealed class BusServiceTests
{
    private AppDbContext _mockDbContext;
    private Mock<IRabbitMqConnectionProvider> _mockRabbitMqConnectionProvider;
    private Mock<ILogger<BusService>> _mockLogger;
    private BusService _busService;
    private Mock<IConnection> _connectionMock;
    private Mock<IChannel> _channelMock;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockDbContext = new AppDbContext(options);
        
        _mockRabbitMqConnectionProvider = new Mock<IRabbitMqConnectionProvider>();
        _mockRabbitMqConnectionProvider = new Mock<IRabbitMqConnectionProvider>();
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IChannel>();
        _mockRabbitMqConnectionProvider.Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(_connectionMock.Object);
        _connectionMock.Setup(x => x.CreateChannelAsync())
            .ReturnsAsync(_channelMock.Object);
        
        _mockLogger = new Mock<ILogger<BusService>>();
        _busService = new BusService(_mockDbContext, _mockRabbitMqConnectionProvider.Object, _mockLogger.Object);
    }
    
    [TestMethod]
    public async Task GetAllOperations_Success()
    {
        var operation = new BusOperations("Get", "1.0", "getting");

        _mockDbContext.Operations.Add(operation);
        
        await _mockDbContext.SaveChangesAsync();
        
        var result = _busService.GetAllOperations();
        Assert.IsNotEmpty(result);
    }

    [TestMethod]
    public void GetAllOperations_OperationsIsEmpty()
    {
        var result =  _busService.GetAllOperations();
        
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetAllCollections_Success()
    {
        var collection = new Collections(
            "Get", 
            "getting",
            "bus.info.exchange", 
            "bus.info.queue", 
            "bus.info.queue");
        _mockDbContext.Collections.Add(collection);
        
        await _mockDbContext.SaveChangesAsync();
        
        var result = _busService.GetAllCollections();
        Assert.IsNotEmpty(result);
    }

    [TestMethod]
    public void GetAllCollections_CollectionsIsEmpty()
    {
        var result =  _busService.GetAllCollections();
        
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task CreateCollection_CreateSuccess_ReturnSuccessMessage()
    {
        var collection = new CollectionToCreate(
            "Get", 
            "getting",
            "bus.info.exchange", 
            "bus.info.queue", 
            "bus.info.queue");
        
        var result = await _busService.CreateCollection(collection);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("success", result.MessageToSent);
    }
    
    [TestMethod]
    public async Task CreateCollection_NullRequest_ReturnErrorMessage()
    {
        var result = await _busService.CreateCollection(null);
        
        Assert.IsNotNull(result);
        Assert.AreNotEqual("success", result.MessageToSent);
    }

    [TestMethod]
    public async Task CreateCollection_CollectionAlreadyExist_ReturnErrorMessage()
    {
        var collection = new CollectionToCreate(
            "Get", 
            "getting",
            "bus.info.exchange", 
            "bus.info.queue", 
            "bus.info.queue");

        var newCollection = new Collections(
            collection.Title,
            collection.Description,
            collection.ExchangeName,
            collection.QueueName,
            collection.RouteKey);
        
        _mockDbContext.Collections.Add(newCollection);
        await _mockDbContext.SaveChangesAsync();
        
        var result = await _busService.CreateCollection(collection);
        
        Assert.IsNotNull(result);
        Assert.AreNotEqual("success", result.MessageToSent);
    }

    [TestMethod]
    public async Task CreateOperation_CreateSuccess_ReturnSuccessMessage()
    {
        var operation = new OperationToCreate("Get", "1.0", "getting");
        
        var result = await _busService.CreateOperation(operation);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("success", result.MessageToSent);
    }

    [TestMethod]
    public async Task CreateOperation_NullRequest_ReturnErrorMessage()
    {
        var result = await _busService.CreateOperation(null);
        
        Assert.IsNotNull(result);
        Assert.AreNotEqual("success", result.MessageToSent);
    }

    [TestMethod]
    public async Task CreateOperation_OperationAlreadyExist_ReturnErrorMessage()
    {
        var operation = new OperationToCreate("Get", "1.0", "getting");
        var newOperation = new BusOperations(
            operation.Type,
            operation.Version,
            operation.Description);
        
        _mockDbContext.Operations.Add(newOperation);
        await _mockDbContext.SaveChangesAsync();
        
        var result = await _busService.CreateOperation(operation);
        
        Assert.IsNotNull(result);
        Assert.AreNotEqual("success", result.MessageToSent);
    }
    
    [TestMethod]
    public async Task Bind_CollectionNotFound_ReturnErrorMessage()
    {
        var options = new BindCollectionOperation("110", "231");
        var result = await _busService.Bind(options);
        
        Assert.IsNotNull(result);
        
        Assert.AreNotEqual("success", result.MessageToSent);

    }
    
    [TestMethod]
    public async Task Bind_BindSuccess_ReturnSuccessMessage()
    {
        var collection = new Collections(
            "Get", 
            "getting",
            "bus.info.exchange", 
            "bus.info.queue", 
            "bus.info.queue");

        var operation = new BusOperations("Get", "1.0", "getting");
        
        _mockDbContext.Collections.Add(collection);
        _mockDbContext.Operations.Add(operation);
        await _mockDbContext.SaveChangesAsync();
        
        var options = new BindCollectionOperation(collection.Id.ToString(), operation.Id.ToString());
        var result = await _busService.Bind(options);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("success", result.MessageToSent);
    }
}