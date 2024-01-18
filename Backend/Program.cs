using System.Net;
using System.Security.Cryptography.X509Certificates;
using Backend.Hubs;
using Backend.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

// Reads configuration information from appsettings.json and appsettings.Production.json (or another environment file).
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// Configures the logger based on the previously loaded configuration.
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

// Add services to the container.
builder.Services.AddSingleton<ILogger>(logger);

builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ISpeechBubbleListService, SpeechBubbleListService>();

builder.Services.AddSingleton<IAvProcessingService, AvProcessingService>();

builder.Services.AddSingleton<IWordProcessingService, WordProcessingService>();

builder.Services.AddSingleton<IAvReceiverService, AvReceiverService>();

builder.Services.AddSingleton<ISubtitleExporterService, SubtitleExporterService>();

builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

builder.Services.AddSingleton<IFrontendCommunicationService, FrontendCommunicationService>();

builder.Services.AddSingleton<ISpeechmaticsConnectionService, SpeechmaticsConnectionService>();

builder.Services.AddSingleton<ISpeechmaticsSendService, SpeechmaticsSendService>();

builder.Services.AddSingleton<ISpeechmaticsReceiveService, SpeechmaticsReceiveService>();

builder.Services.AddHostedService<StartupService>();

builder.Services.AddHostedService<BufferTimeMonitor>();

var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:40110";
Console.WriteLine($"Expecting Frontend on: {frontendUrl}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend", policy =>
        {
            policy.WithOrigins(frontendUrl) // Replace with your Angular app URL
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Set Certificate
builder.Services.Configure<KestrelServerOptions>(
    options =>
    {
        var crt = "ssl/server.pfx";
        var cert = new X509Certificate2(crt);

        // http
        options.Listen(IPAddress.Any, 40114);

        // https
        options.Listen(
            IPAddress.Any,
            40115,
            listenOptions =>
            {
                listenOptions.UseHttps(cert);
            });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

app.UseRouting();

app.UseCors("AllowAngularFrontend");

app.MapHub<FrontendCommunicationHub>("/communicationHub");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

var backendUrl = Environment.GetEnvironmentVariable("BACKEND_URL") ?? "http://localhost:40114";
Console.WriteLine($"Starting Backend on: {backendUrl}");

app.Run(backendUrl);
