using Bank.Client.Web.Data;
using Bank.Client.Web.Services;
using ClassLibrary;
using ClassLibrary.Models.RDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;

namespace Bank.Client.Web.Test;

[TestClass]
public sealed class ClientServiceTests
{
    private Mock<ILogger<ClientService>> _loggerMock;
    private Mock<IRabbitMqConnectionProvider> _rabbitMqConnectionProviderMock;
    private AppDbContext _context;
    private ClientService _service;
    private Mock<IConnection> _connectionMock;
    private Mock<IChannel> _channelMock;
    
    
    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<ClientService>>();
        
        _rabbitMqConnectionProviderMock = new Mock<IRabbitMqConnectionProvider>();
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IChannel>();
        _rabbitMqConnectionProviderMock.Setup(x => x.GetConnectionAsync())
            .ReturnsAsync(_connectionMock.Object);
        _connectionMock.Setup(x => x.CreateChannelAsync())
            .ReturnsAsync(_channelMock.Object);
        
        _service = new ClientService(_loggerMock.Object, _rabbitMqConnectionProviderMock.Object, _context);
    }
    
    [TestMethod]
    public async Task PostAsync_RequestIsNull_ReturnErrorMessage()
    {
        var result = await _service.PostAsync(null);
        
        Assert.IsNotNull(result);
        Assert.AreNotEqual("success", result.MessageToSent);
    }

    [TestMethod]
    public async Task PostAsync_PostSuccess_ReturnSuccessMessage()
    {
        var request = new ClientRequestDto("Get", "1.0", "Swagger", null);
        var result = await _service.PostAsync(request);
        
        Assert.IsNotNull(result);
        Assert.AreEqual("success", result.MessageToSent);
    }
}