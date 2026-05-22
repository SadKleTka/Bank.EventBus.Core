using Bank.Client.Web.Services;
using ClassLibrary.Models.RDto;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Client.Web.Controllers;

[ApiController]
[Route("input/request")]
public class ClientController : ControllerBase
{
    private readonly ILogger<ClientController> _logger;
    private readonly IClientService _service;
    
    public ClientController(ILogger<ClientController> logger, IClientService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<Message>> PostAsync
    (
        [FromBody] ClientRequestDto request
    )
    {
        _logger.LogInformation("Client post some value");

        return Ok(await _service.PostAsync(request));
    }
}