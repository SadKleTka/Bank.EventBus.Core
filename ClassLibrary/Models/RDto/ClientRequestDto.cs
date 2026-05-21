namespace ClassLibrary.Models.RDto;

public record ClientRequestDto(string Type, string Version, DateTime DateTime, string Source, BodyForClient? Body);