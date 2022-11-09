using Grpc.Core;
using IndependentProtobuf;

namespace IndependentProtobuf.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var message = new HelloReply
        {
            Message = "Hello " + request.Name,
            NestedMessageField = new HelloReply.Types.NestedMessage()
        };

        message.NestedMessageField.StringCollection.Add("entry 1");
        message.NestedMessageField.StringCollection.Add(new List<string> { "entry 2", "entry 3" });

        return Task.FromResult(message);

    }
}
