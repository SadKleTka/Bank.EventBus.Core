using ClassLibrary.Models.RDto;

namespace ClassLibrary.Models;

public class ClientRequest
{
    public string Type { get; set; }
    public string Version { get; set; }
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    
    public BodyForClient? Body { get; set; }
}