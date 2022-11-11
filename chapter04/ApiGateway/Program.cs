using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Performance;
using MonitorClient = Performance.Monitor.MonitorClient;
using ApiGateway;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var serverUrl = builder.Configuration.GetSection("ServerUrl").Value;

builder.Services.AddControllers();
builder.Services.AddSingleton(builder.Configuration);
builder.Services.AddSingleton<IGrpcPerformanceClient>(p => new
    GrpcPerformanceClient(serverUrl));
builder.Services.AddGrpcClient<MonitorClient>(o =>
{
    o.Address = new Uri(serverUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
