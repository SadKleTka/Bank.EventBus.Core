using RabbitMQ.Client;

namespace ClassLibrary;

public interface IRabbitMqConnectionProvider
{
    public Task<IConnection> GetConnectionAsync();
}