using SharpGateway.Models;
using Serilog;

// Load Env configuration
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Load from .env manually
builder.Configuration.AddEnvironmentVariables();

// Register Authorize.Net config for IOptions
builder.Services.Configure<AuthorizeNetConfig>(
    builder.Configuration.GetSection("AuthorizeNet")
);

// Configure Serilog before building
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/gateway-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog(); 

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
