using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserInfoManager;

using UserInfoManager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<UserDataCache>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5002, o => o.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();



// Configure the HTTP request pipeline.
app.MapGrpcService<UserInfoService>();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Hello World");
app.MapControllers();
app.Run();


