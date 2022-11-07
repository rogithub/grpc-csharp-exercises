using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Status;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase
{
    private readonly IGrpcStatusClient _client;

    private readonly ILogger<StatusController> _logger;

    public StatusController(ILogger<StatusController> logger, IGrpcStatusClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet]
    public async Task<IEnumerable<ClientStatusModel>> GetAllStatuses()
    {
        return await _client.GetAllStatuses();
    }

    [HttpGet("{clientName}")]
    public async Task<ClientStatusModel> GetClientStatus(string clientName)
    {
        return await _client.GetClientStatus(clientName);
    }

    [HttpPost("{clientName}/{status}")]
    public async Task<bool> UpdateClientStatus(string clientName, ClientStatus status)
    {
        return await _client.UpdateClientStatus(clientName, status);
    }
}
