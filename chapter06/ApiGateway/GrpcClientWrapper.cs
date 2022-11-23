using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataProcessor;
using Grpc.Net.Client;

namespace ApiGateway;
public interface IGrpcClientWrapper
{
    Task<int> SendDataViaStandardClient(int requestCount);
    Task<int> SendDataViaLoadBalancer(int requestCount);
}

internal class GrpcClientWrapper : IGrpcClientWrapper, IDisposable
{
    private int currentChannelIndex = 0;
    private readonly GrpcChannel standardChannel;
    private readonly List<GrpcChannel> roundRobinChannels;
    public GrpcClientWrapper(List<string> addresses)
    {
        
// BEGIN WARNING! Do not use this in Prod.
// **************************************
var httpHandler = new HttpClientHandler();
// Return `true` to allow certificates that are untrusted/invalid
httpHandler.ServerCertificateCustomValidationCallback =
    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
var opts = new GrpcChannelOptions { HttpHandler = httpHandler };
// END WARNING! Do not use this in Prod.
// **************************************

        roundRobinChannels = new List<GrpcChannel>();
        standardChannel = GrpcChannel.ForAddress(addresses[0], opts);
        foreach (var address in addresses)
        {
            roundRobinChannels.Add(GrpcChannel.ForAddress(address, opts));
        }
    }

    public async Task<int> SendDataViaStandardClient(int requestCount)
    {
        var count = 0;
        for (var i = 0; i < requestCount; i++)
        {
            var client = new Ingestor.IngestorClient(standardChannel);
            await client.ProcessDataAsync(GenerateDataRequest(i));
            count++;
        }

        return count;
    }

    public async Task<int> SendDataViaLoadBalancer(int requestCount)
    {
        var count = 0;
        for (var i = 0; i < requestCount; i++)
        {
            var client = new Ingestor.IngestorClient(roundRobinChannels[GetCurrentChannelIndex()]);
            await client.ProcessDataAsync(GenerateDataRequest(i));
            count++;
        }
        return count;
    }


    private int GetCurrentChannelIndex()
    {
        if (currentChannelIndex == roundRobinChannels.Count - 1)
          currentChannelIndex = 0;
        else
            currentChannelIndex++;
        return currentChannelIndex;
    }

    private DataRequest GenerateDataRequest(int index)
    {
        return new DataRequest{
            Id = index,
            Name = $"Object {index}",
            Description = $"This is an object with the index of {index}."
        };
    }

    public void Dispose()
    {
        standardChannel.Dispose();
        foreach (var channel in roundRobinChannels)
        {
            channel.Dispose();
        }
    }

}