using GrpcServerCommon;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<IngestorService>();

app.MapGet("/", () => "Hello World!");

app.Run();
