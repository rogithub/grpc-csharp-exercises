using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Performance;

using MonitorClient = Performance.Monitor.MonitorClient;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class ConcurrencyController : ControllerBase
{
     
    private readonly string serverUrl;
    private readonly ILogger<ConcurrencyController> _logger;

    public ConcurrencyController(ILogger<ConcurrencyController> logger,        
        IConfiguration configuration)
    {
        _logger = logger;
        
        serverUrl = configuration.GetSection("ServerUrl").Value;
    }

    [HttpGet("single-connection/{count}")]
    public ResponseModel GetDataFromSingleConnection(int count)
    {
        using var channel = GrpcPerformanceClient.BuildInsecureChannel(serverUrl);
        var stopWatch = Stopwatch.StartNew();
        var response = new ResponseModel();        
        var concurrentJobs = new List<Task>();

        for (var i = 0; i < count; i++)
        {
            var client = new MonitorClient(channel);
            concurrentJobs.Add(Task.Run(() =>
            {
                client.GetPerformance(new PerformanceStatusRequest { ClientName = $"client {i + 1}" });
            }));
        }
        Task.WaitAll(concurrentJobs.ToArray());
        response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
        return response;
    }    

    [HttpGet("multiple-connections/{count}")]
    public ResponseModel GetDataFromMultipleConnections(int count)
    {                
        var httpHandler = GrpcPerformanceClient.BuildInsecureHandler();      
        httpHandler.PooledConnectionIdleTimeout = System.Threading.Timeout.InfiniteTimeSpan;
        httpHandler.KeepAlivePingDelay = TimeSpan.FromSeconds(60);
        httpHandler.KeepAlivePingTimeout = TimeSpan.FromSeconds(30);
        httpHandler.EnableMultipleHttp2Connections = true; // <---- Allow client to open connections as needed
        using var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions 
        {
            HttpHandler = httpHandler 
        });

        var stopWatch = Stopwatch.StartNew();
        var response = new ResponseModel();
        var concurrentJobs = new List<Task>();
        for (var i = 0; i < count; i++)
        {
            concurrentJobs.Add(Task.Run(() => {
                var client = new MonitorClient(channel);
                client.GetPerformance(new PerformanceStatusRequest { ClientName = $"client {i + 1}" });
            }));
        }
        Task.WaitAll(concurrentJobs.ToArray());        
        response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
        return response;
    }

}