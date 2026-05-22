namespace ClassLibrary.Models.RDto;

public record ClientRequestDto(string Type, string Version, string Source, BodyForClient? Body);