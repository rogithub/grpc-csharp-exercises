using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserInfoManager;
using System.Security.Cryptography.X509Certificates;

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
    options.ConfigureHttpsDefaults(o =>
    {
        o.ServerCertificate = new X509Certificate2("UserInfoManager.pfx", "password");
    });
    options.ListenLocalhost(5002, o => o.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);

    options.ListenAnyIP(5001, o => o.UseHttps());
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
    options.HttpsPort = 5001;
});


var app = builder.Build();

app.UseHttpsRedirection();


// Configure the HTTP request pipeline.
app.MapGrpcService<UserInfoService>();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Hello World");
app.MapControllers();
app.Run();


