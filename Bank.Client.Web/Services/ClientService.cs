using System.Text;
using System.Text.Json;
using Bank.Client.Web.Data;
using ClassLibrary;
using ClassLibrary.Models;
using ClassLibrary.Models.RDto;
using RabbitMQ.Client;

namespace Bank.Client.Web.Services;

public class ClientService : IClientService
{
    private readonly ILogger<ClientService> _logger;
    private readonly RabbitMqConnectionProvider _connection;
    private readonly AppDbContext _context;

    public ClientService(ILogger<ClientService> logger, RabbitMqConnectionProvider connection, AppDbContext context)
    {
        _logger = logger;
        _connection = connection;
        _context = context;
    }

    public async Task<Message> PostAsync(ClientRequestDto request)
    {
        string exchange = "bus.input.exchange";
        string queue = "bus.input.queue";
        try
        {
            var newRequest = new ClientRequest(
                request.Type,
                request.Version,
                DateTime.UtcNow,
                request.Source,
                request.Body);
            
            await _context.ClientRequests.AddAsync(newRequest);
            await _context.SaveChangesAsync();
            
            var connection = await _connection.GetConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            
            var json = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(json);
            
            await channel.BasicPublishAsync(exchange, queue, body);
            
            
            return new Message("success", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while client been sending the message");
            return new Message($"error {ex}", DateTime.UtcNow);
        }
    }
}