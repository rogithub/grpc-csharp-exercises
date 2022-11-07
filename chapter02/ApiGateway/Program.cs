using ApiGateway;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var serverUrl = builder.Configuration.GetSection("ServerUrl").Value;
builder.Services.AddSingleton<IGrpcStatusClient>(p => new GrpcStatusClient(serverUrl));
builder.Services.AddSingleton<IGrpcJobsClient>(p => new GrpcJobsClient(serverUrl));

// Register the Swagger services
builder.Services.AddSwaggerDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseOpenApi();
    app.UseSwaggerUi3();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
