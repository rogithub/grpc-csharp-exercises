using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserInfoManager;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UserInfoManager.Services;

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
        o.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
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

builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = context =>
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name,
                    context.ClientCertificate.Subject,
                    ClaimValueTypes.String,
                    context.Options.ClaimsIssuer)
                };
                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                Console.WriteLine($"Client certificate thumbprint {context.ClientCertificate.Thumbprint}");
                Console.WriteLine($"Client certificate subject: {context.ClientCertificate.Subject}");
                context.Success();
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                context.Response.StatusCode = 403;
                context.Response.ContentType = "text/plain";
                context.Response.WriteAsync(context.Exception.ToString()).Wait();
                return Task.CompletedTask;
            },
        };
    })
    .AddCertificateCache();


app.UseAuthentication();

var app = builder.Build();

app.UseHttpsRedirection();


// Configure the HTTP request pipeline.
app.MapGrpcService<UserInfoService>();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Hello World");
app.MapControllers();
app.Run();


