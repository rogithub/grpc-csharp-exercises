using System;
using System.Threading.Tasks;
using Grpc.Net.Client;


// See https://aka.ms/new-console-template for more information


// BEGIN WARNING! Do not use this in Prod.
// **************************************
var httpHandler = new HttpClientHandler();
// Return `true` to allow certificates that are untrusted/invalid
httpHandler.ServerCertificateCustomValidationCallback =
    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
// END WARNING! Do not use this in Prod.
// **************************************

Console.WriteLine("Please enter gRPC server address:");
var serverUrl = Console.ReadLine();
using var channel =
    GrpcChannel.ForAddress(serverUrl,
    new GrpcChannelOptions { HttpHandler = httpHandler });

var client = new Stats.V1.Status.StatusClient(channel);
Console.WriteLine("Please enter client name:");
var clientName = Console.ReadLine();
Console.WriteLine("Please enter client description:");
var clientDescription = Console.ReadLine();

var response = await client.GetStatusAsync(new Stats.V1.StatusRequest
{
    ClientName = clientName,
    ClientDescription = clientDescription,
    Allowed = true
});

Console.WriteLine($"Server name: {response.ServerName}");
Console.WriteLine($"Server description: {response.ServerDescription}");
Console.WriteLine($"Number of connections: {response.NumberOfConnections}");
Console.WriteLine($"CPU usage: {response.CpuUsage}");
Console.WriteLine($"Memory usage: {response.MemoryUsage}");
Console.WriteLine($"Errors logged: {response.ErrorsLogged}");
Console.WriteLine($"Catastrophic failures logged: {response.CatastrophicFailuresLogged}");
Console.WriteLine($"Active: {response.Active}");
Console.ReadKey();