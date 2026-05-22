using ClassLibrary.Models.RDto;

namespace ClassLibrary.Models;

public class ClientRequest
{

    public ClientRequest(string type, string version, DateTime dateTime, string source, BodyForClient? body)
    {
        Type = type;
        Version = version;
        DateTime = dateTime;
        Source = source;
        Body = body;
    }
    
    public ClientRequest() {}
    
    public string Type { get; set; }
    public string Version { get; set; }
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    
    public BodyForClient? Body { get; set; }
}