using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace ClassLibrary;

public class RabbitMqConnectionProvider 
{
    private readonly ILogger<RabbitMqConnectionProvider> _logger;
    
    private IConnection? _connection; 
    
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RabbitMqConnectionProvider(ILogger<RabbitMqConnectionProvider> logger)
    {
        _logger = logger;
    }

    public async Task<IConnection> GetConnectionAsync() 
    {
        if (_connection != null)
            return _connection;

        await _semaphore.WaitAsync();
        try
        {
            if (_connection == null)
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    Port = 5674, 
                    UserName = "guest",
                    Password = "guest"
                };
                
                _connection = await factory.CreateConnectionAsync();
                _logger.LogInformation("Connection with RabbitMq has been established.");
            }
            return _connection;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occured: " + ex.Message);
            throw; 
        }
        finally
        {
            _semaphore.Release();
        }
    }
}