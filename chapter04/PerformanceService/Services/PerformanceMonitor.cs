using System;
using System.Threading.Tasks;
using Grpc.Core;
using Performance;
using Google.Protobuf;

namespace PerformanceService.Services;
public class PerformanceMonitor : Performance.Monitor.MonitorBase
{
    public override Task<PerformanceStatusResponse> GetPerformance(PerformanceStatusRequest request, ServerCallContext context)
    {
        var randomNumberGenerator = new Random();
        return Task.FromResult(GetPerformaceResponse());
        
    }


    public override async Task GetManyPerformanceStats(IAsyncStreamReader<PerformanceStatusRequest> requestStream,
                IServerStreamWriter<PerformanceStatusResponse> responseStream, ServerCallContext context)
    {
        while (await requestStream.MoveNext())
        {
            var randomNumberGenerator = new Random();

            await responseStream.WriteAsync(GetPerformaceResponse());
        }
    }


    private PerformanceStatusResponse GetPerformaceResponse()
    {
        var randomNumberGenerator = new Random();
        var dataLoad1 = new byte[100];
        var dataLoad2 = new byte[100];
        randomNumberGenerator.NextBytes(dataLoad1);
        randomNumberGenerator.NextBytes(dataLoad2);
        return new PerformanceStatusResponse
        {
            CpuPercentageUsage = randomNumberGenerator.NextDouble() * 100,
            MemoryUsage = randomNumberGenerator.NextDouble() * 100,
            ProcessesRunning = randomNumberGenerator.Next(),
            ActiveConnections = randomNumberGenerator.Next(),
            DataLoad1 = UnsafeByteOperations.UnsafeWrap(dataLoad1), // faster but keeps a reference, modifying original might corrupt result
            DataLoad2 = ByteString.CopyFrom(dataLoad2) // safer but slower
        };
    }            
}