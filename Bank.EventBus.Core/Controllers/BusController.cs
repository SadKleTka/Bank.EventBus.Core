using Bank.EventBus.Core.Service;
using Bank.EventBus.Worker.Models;
using Bank.EventBus.Worker.Models.Dto;
using ClassLibrary.Models.RDto;
using Microsoft.AspNetCore.Mvc;

namespace Bank.EventBus.Core.Controllers;

[ApiController]
[Route("/api/v1")]
public class BusController : ControllerBase
{
    private readonly ILogger<BusController> _logger;
    private readonly IBusService _service;
    public BusController(ILogger<BusController> logger, IBusService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost("operations/collections/bind")]
    public async Task<ActionResult<Message>> Bind
    (
        BindCollectionOperation bindCollectionOperation
    )
    {
        _logger.LogInformation("Bind collection operation has been initialized.");
        return Ok(await _service.Bind(bindCollectionOperation));
    }

    [HttpGet("operations")]
    public ActionResult<List<OperationToAnswer>> GetAllOperations()
    {
        _logger.LogInformation("Get operations method has been initialized.");
        return Ok(_service.GetAllOperations());
    }

    [HttpGet("collections")]
    public ActionResult<List<CollectionToAnswer>> GetAllCollections()
    {
        _logger.LogInformation("Collection creation method has been initialized.");
        return Ok(_service.GetAllCollections());
    }

    [HttpPost("new/collection")]
    public async Task<ActionResult<Message>> CreateCollection
    (
        [FromBody] CollectionToCreate collectionToCreate
    )
    {
        _logger.LogInformation("Collection creation method has been initialized.");
        return Ok(await _service.CreateCollection(collectionToCreate));
    }

    [HttpPost("new/operation")]
    public async Task<ActionResult<Message>> CreateOperation
    (
        [FromBody] OperationToCreate operationToCreate
    )
    {
        _logger.LogInformation("Operation creation method has been initialized.");
        return Ok(await _service.CreateOperation(operationToCreate));
    }
}