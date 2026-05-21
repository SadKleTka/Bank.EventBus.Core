using Bank.EventBus.Worker.Models;
using Bank.EventBus.Worker.Models.Dto;
using ClassLibrary.Models.RDto;

namespace Bank.EventBus.Core.Service;

public interface IBusService
{
    Task<Message> CreateCollection(CollectionToCreate collectionToCreate);
    Task<Message> CreateOperation(OperationToCreate operationToCreate);
    List<CollectionToAnswer> GetAllCollections();
    List<OperationToAnswer> GetAllOperations();
    Task<Message> Bind(BindCollectionOperation bindCollectionOperation);
}