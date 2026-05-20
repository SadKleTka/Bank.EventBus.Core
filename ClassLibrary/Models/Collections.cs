namespace Bank.EventBus.Worker.Models.Dto;

public class Collections
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ExchangeName {get; set;}
    public string QueueName {get; set;}
    public string RoutingKey {get; set;}
    
    public List<BusCollectionsOperations> Operations {get; set;}
}