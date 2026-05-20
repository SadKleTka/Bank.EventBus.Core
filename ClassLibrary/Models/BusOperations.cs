namespace Bank.EventBus.Worker.Models;

public class BusOperations
{

    public BusOperations(string type, string version, string description)
    {
        Type = type;
        Version = version;
        Description = description;
    }
    public Guid Id {get; set;}
    public string Type {get; set;}
    public string Version {get; set;}
    public string Description {get; set;}
    
    public List<BusCollectionsOperations> Collections {get; set;}
}