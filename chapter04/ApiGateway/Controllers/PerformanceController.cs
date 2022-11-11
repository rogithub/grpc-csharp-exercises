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
public class PerformanceController : ControllerBase
{
 
    private readonly MonitorClient factoryClient;
    private readonly IGrpcPerformanceClient clientWrapper;
    private readonly string serverUrl;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(ILogger<PerformanceController> logger,
        MonitorClient factoryClient,
        IGrpcPerformanceClient clientWrapper,
        IConfiguration configuration)
    {
        _logger = logger;

        this.factoryClient = factoryClient;
        this.clientWrapper = clientWrapper;
        serverUrl = configuration.GetSection("ServerUrl").Value;
    }

    [HttpGet("factory-client/{count}")]
    public async Task<ResponseModel> GetPerformanceFromFactoryClient(int count)
    {
        var stopWatch = Stopwatch.StartNew();
        var response = new ResponseModel();
        for (var i = 0; i < count; i++)
        {
            var grpcResponse = await factoryClient.GetPerformanceAsync(new PerformanceStatusRequest 
            { 
                ClientName = $"client {i + 1}" 
            });

            response.PerformanceStatuses.Add(new ResponseModel.PerformanceStatusModel {
                CpuPercentageUsage = grpcResponse.CpuPercentageUsage,
                MemoryUsage = grpcResponse.MemoryUsage,
                ProcessesRunning = grpcResponse.ProcessesRunning,
                ActiveConnections = grpcResponse.ActiveConnections
            });
        }

        response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
        return response;
    }


    [HttpGet("client-wrapper/{count}")]
    public async Task<ResponseModel> GetPerformanceFromClientWrapper(int count)
    {
        var stopWatch = Stopwatch.StartNew();
        var response = new ResponseModel();
        
        for (var i = 0; i < count; i++)
        {
            var grpcResponse = await clientWrapper.GetPerformanceStatus($"client {i + 1}");
            response.PerformanceStatuses.Add(grpcResponse);
        }
        
        response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
        return response;
    }

    [HttpGet("initialized-client/{count}")]
    public async Task<ResponseModel> GetPerformanceFromNewClient(int count)
    {
        var stopWatch = Stopwatch.StartNew();
        var response = new ResponseModel();
        
        for (var i = 0; i < count; i++)
        {
            using var channel = GrpcPerformanceClient.BuildInsecureChannel(serverUrl);
            var client = new MonitorClient(channel);
            var grpcResponse = await client.GetPerformanceAsync(new PerformanceStatusRequest { ClientName = $"client {i + 1}" });
            response.PerformanceStatuses.Add(new ResponseModel.PerformanceStatusModel
            {
                CpuPercentageUsage = grpcResponse.
                CpuPercentageUsage,
                MemoryUsage = grpcResponse.MemoryUsage,
                ProcessesRunning = grpcResponse.ProcessesRunning,
                ActiveConnections = grpcResponse.ActiveConnections
            });
        }
        
        response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
        return response;
    }
}