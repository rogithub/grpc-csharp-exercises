using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace DeviceManagerService.Services;


public class ManagerService : DeviceManagement.DeviceManager.DeviceManagerBase
{
    private readonly IDeviceStatusCache deviceStatusCache;
    public ManagerService(IDeviceStatusCache deviceStatusCache)
    {
        this.deviceStatusCache = deviceStatusCache;
    }

    public override Task<DeviceManagement.UpsertDeviceResponse> UpsertDeviceStatus
        (DeviceManagement.DeviceDetails request, ServerCallContext context)
    {
        deviceStatusCache.UpsertDeviceDetail(request);
        Console.WriteLine($"DeviceManagerService triggered. Peer: {context.Peer}. Host: {context.Host}.");
        Console.WriteLine($"Device id: {request.DeviceId}, Name: {request.Name}, Description: {request.Description}, Status {request.Status}.");
        return Task.FromResult(new DeviceManagement.UpsertDeviceResponse
        {
            Success = true
        });
    }

    public override async Task<DeviceManagement.UpsertDeviceResponse> UpsertDeviceStatuses
        (IAsyncStreamReader<DeviceManagement.DeviceDetails> requestStream, ServerCallContext context)
    {
        await foreach (var status in requestStream.ReadAllAsync())
        {
            deviceStatusCache.UpsertDeviceDetail(status);
            Console.WriteLine
                ($"Device id: {status.DeviceId},Name: {status.Name}, Description: {status.Description}, Status {status.Status}.");
        }
        return new DeviceManagement.UpsertDeviceResponse
        {
            Success = true
        };
    }

    public override async Task GetAllStatuses(Empty request, 
        IServerStreamWriter<DeviceManagement.DeviceDetails> responseStream, 
        ServerCallContext context)
    {
        foreach (var device in deviceStatusCache.GetAllDeviceDetails())
        {
            if (DateTime.UtcNow.AddSeconds(1) > context.Deadline)
                break;
            await responseStream.WriteAsync(device);
            await Task.Delay(500);
        }
    }
}
