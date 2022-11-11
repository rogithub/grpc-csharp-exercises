using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Performance;
using MonitorClient = Performance.Monitor.MonitorClient;
using System.Net.Security;
using System.Collections.Generic;
using Grpc.Core;
using System.Runtime.InteropServices;

namespace ApiGateway;

public interface IGrpcPerformanceClient
{
    Task<ResponseModel.PerformanceStatusModel> GetPerformanceStatus(string clientName);
    Task<IEnumerable<ResponseModel.PerformanceStatusModel>> GetPerformanceStatuses(IEnumerable<string> clientNames);
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

    public static SocketsHttpHandler BuildInsecureHandler()
    {
        // BEGIN WARNING! Do not use this in Prod.
        // **************************************
        var sslOptions = new SslClientAuthenticationOptions
        {
            // Leave certs unvalidated for debugging
            RemoteCertificateValidationCallback = delegate { return true; },
        };

        var httpHandler = new SocketsHttpHandler()
        {
            SslOptions = sslOptions,
        };
        // END WARNING! Do not use this in Prod.
        // **************************************

        return httpHandler;
    }

    private ResponseModel.PerformanceStatusModel ReadResponse(PerformanceStatusResponse response)
    {
        return new ResponseModel.PerformanceStatusModel
        {
            CpuPercentageUsage = response.CpuPercentageUsage,
            MemoryUsage = response.MemoryUsage,
            ProcessesRunning = response.ProcessesRunning,
            ActiveConnections = response.ActiveConnections,
            DataLoad1 = response.DataLoad1.ToByteArray(),
            DataLoad2 = MemoryMarshal.TryGetArray(response.DataLoad2.Memory, out var segment) ? 
                segment.Array : 
                response.DataLoad2.Memory.ToArray()
        };
    }

    public async Task<ResponseModel.PerformanceStatusModel> GetPerformanceStatus(string clientName)
    {
        var client = new MonitorClient(channel);
        var response = await client.GetPerformanceAsync(new PerformanceStatusRequest
        {
            ClientName = clientName
        });

        return ReadResponse(response);
    }

    public async Task<IEnumerable<ResponseModel.PerformanceStatusModel>> GetPerformanceStatuses(IEnumerable<string> clientNames)
    {
        var client = new MonitorClient(channel);
        using var call = client.GetManyPerformanceStats();
        var responses = new List<ResponseModel.PerformanceStatusModel>();

        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                responses.Add(ReadResponse(response));
            }
        });

        foreach (var clientName in clientNames)
        {
            await call.RequestStream.WriteAsync(new PerformanceStatusRequest {
                ClientName = clientName
            });
        }

        return responses;
    }


    public void Dispose()
    {
        channel.Dispose();
    }
}

