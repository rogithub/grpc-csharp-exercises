using Grpc.Net.Client;

// See https://aka.ms/new-console-template for more information



// The port number(5001) must match the port of
// the gRPC server.
using var channel =
    GrpcChannel.ForAddress("https://localhost:5001");

var client = new
    GreetingsManager.GreetingsManagerClient(channel);

var reply = await client.GenerateGreetingAsync(
    new GreetingRequest { Name = "BasicGrpcClient" }
);

Console.WriteLine("Greeting: " + reply.GreetingMessage);
Console.WriteLine("Press any key to exit...");

Console.ReadKey();