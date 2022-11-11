using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Performance;
using MonitorClient = Performance.Monitor.MonitorClient;

namespace ApiGateway;

public interface IGrpcPerformanceClient
{
    Task<ResponseModel.PerformanceStatusModel>
    GetPerformanceStatus(string clientName);
}


internal class GrpcPerformanceClient: IGrpcPerformanceClient, IDisposable
{
    private readonly GrpcChannel channel;
    public GrpcPerformanceClient(string serverUrl)
    {
        
        this.channel = BuildInsecureChannel(serverUrl);
    }

    public static Grpc.Net.Client.GrpcChannel BuildInsecureChannel(string serverUrl)
    {
        // BEGIN WARNING! Do not use this in Prod.
        // **************************************
        var httpHandler = new HttpClientHandler();
        // Return `true` to allow certificates that are untrusted/invalid
        httpHandler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        // END WARNING! Do not use this in Prod.
        // **************************************

        return GrpcChannel.ForAddress(serverUrl,
        new GrpcChannelOptions { HttpHandler = httpHandler });
    }

    public async Task<ResponseModel.PerformanceStatusModel> GetPerformanceStatus(string clientName)
    {
        var client = new MonitorClient(channel);
        var response = await client.GetPerformanceAsync(new PerformanceStatusRequest
        {
            ClientName = clientName
        });

        return new ResponseModel.PerformanceStatusModel
        {
            CpuPercentageUsage = response.CpuPercentageUsage,
            MemoryUsage = response.MemoryUsage,
            ProcessesRunning = response.ProcessesRunning,
            ActiveConnections = response.ActiveConnections
        };
    }

    public void Dispose()
    {
        channel.Dispose();
    }
}

