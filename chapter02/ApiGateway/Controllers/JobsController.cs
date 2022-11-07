using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Status;

namespace ApiGateway.Controllers;


[ApiController]
[Route("[controller]")]
public class JobsController : ControllerBase
{
    private readonly IGrpcJobsClient _client;

    private readonly ILogger<JobsController> _logger;

    public JobsController(ILogger<JobsController> logger, IGrpcJobsClient client)
    {
        _logger = logger;
        _client = client;
    }


    [HttpPost("")]
    public void SendJobs([FromBody] IEnumerable<JobModel> jobs)
    {
        _ = client.SendJobs(jobs);
    }

    [HttpPost("{jobsCount}")]
    public void TriggerJobs(int jobsCount)
    {
        _ = client.TriggerJobs(jobsCount);
    }
}
