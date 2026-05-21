using Bank.EventBus.Worker.Models.Dto;

namespace Bank.EventBus.Worker.Models;

public class BusCollectionsOperations
{

    public BusCollectionsOperations(DateTime createdAt, Guid collectionId, Guid busOperationId)
    {
        CreatedAt = createdAt;
        CollectionId = collectionId;
        BusOperationId = busOperationId;
    }
    
    public Guid Id {get; set;}
    public DateTime CreatedAt {get; set;}
    
    public Guid CollectionId {get; set;}
    public Collections Collection {get; set;}
    
    public Guid BusOperationId {get; set;}
    public BusOperations Operation {get; set;}
}