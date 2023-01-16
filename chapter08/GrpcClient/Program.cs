using System;
using GrpcServiceApp;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;

// See https://aka.ms/new-console-template for more information


// BEGIN WARNING! Do not use this in Prod.
// **************************************
var httpHandler = new HttpClientHandler();
// Return `true` to allow certificates that are untrusted/invalid
httpHandler.ServerCertificateCustomValidationCallback =
    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
// END WARNING! Do not use this in Prod.
// **************************************

Console.WriteLine("Please enter the gRPC service URL.");
var url = Console.ReadLine();

using var channel =
    GrpcChannel.ForAddress(url,
    new GrpcChannelOptions { HttpHandler = httpHandler });
    

var client = new Greeter.GreeterClient(channel);

var proceed = true;
while (proceed)
{
    Console.WriteLine("Which acion you would like to take?");
    Console.WriteLine("1 - get a greeting.");
    Console.WriteLine("2 - Receive message count");
    Console.WriteLine("3 - Update message count");
    var action = Console.ReadLine();
    switch (action)
    {
        case "1":
        break;
        case "2":
            var couterResponse = await client.GetMessageProcessedCountAsync(new Empty());
            Console.WriteLine("Message processed count: " + couterResponse.Count);
        break;
        case "3":
            Console.WriteLine("Please type new message count:");
            var messageCount = Console.ReadLine();
            await client.SynchronizeMessageCountAsync(
                new MessageCount { Count = uint.Parse(messageCount)
            });
            Console.WriteLine("Message count successfully updated to " + messageCount);
        break;
        default:
            Console.WriteLine("Invalid selection option.");
        break;
    }

    Console.WriteLine("Please enter the name.");
    var name = Console.ReadLine();
    var reply = await client.SayHelloAsync(
        new HelloRequest { Name = name, RequestTimeUtc = Timestamp.FromDateTime(DateTime.UtcNow) }, 
        deadline: DateTime.UtcNow.AddMinutes(1));

    Console.WriteLine("Message: " + reply.Message);
    Console.WriteLine("Messages processed: " + reply.MessageProcessedCount);
    Console.WriteLine("Message length in bytes: " + reply.MessageLengthInBytes);
    Console.WriteLine("Message length in letters: " + reply.MessageLengthInLetters);
    Console.WriteLine("Milliseconds to deadline: " + reply.MillisecondsToDeadline);
    Console.WriteLine("Seconds to deadline: " + reply.SecondsToDeadline);
    Console.WriteLine("Minutes to deadline: " + reply.MinutesToDeadline);
    Console.WriteLine("Last name present: " + reply.LastNamePresent);
    Console.WriteLine("Message bytes: " + reply.MessageBytes);
    Console.WriteLine("Call processing duration: " + reply.CallProcessingDuration);
    Console.WriteLine("Response time UTC: " + reply.ResponseTimeUtc);
}
Console.WriteLine("Press any key to exit...");
Console.ReadKey();