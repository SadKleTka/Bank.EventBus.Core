namespace ClassLibrary.Models.RDto;

public record BodyForClient(string? Message, Guid? SenderId, Guid? ReceiverId);