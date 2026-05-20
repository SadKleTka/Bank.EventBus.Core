namespace ClassLibrary.Models.RDto;

public record CollectionToAnswer(Guid Id, string Title, string Description, string ExchangeName, string QueueName, string RoutingKey);