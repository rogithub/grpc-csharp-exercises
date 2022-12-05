namespace ApiGateway;

public class DeviceDetails
{
    public DeviceDetails()
    {
        this.Name = string.Empty;
        this.Description = string.Empty;
    }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DeviceStatus Status { get; set; }

}

public enum DeviceStatus
{
    OFFLINE = 0,
    ONLINE = 1,
    BUSY = 2,
    ERRORED = 3
}