namespace Bank.EventBus.Worker.Models;

public class BusOperations
{
    public Guid Id {get; set;}
    public string Type {get; set;}
    public string Version {get; set;}
    public string Description {get; set;}
    
    public List<BusCollectionsOperations> Collections {get; set;} = new List<BusCollectionsOperations>();
}

