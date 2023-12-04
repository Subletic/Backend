using System;
using Backend.Hubs;
using Backend.Services;
using Serilog;
using Serilog.Core;

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
builder.Services.AddSingleton<Serilog.ILogger>(logger);

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

builder.Services.AddSingleton<IAvReceiverService, AvReceiverService>();

builder.Services.AddSingleton<IFrontendCommunicationService, FrontendCommunicationService>();

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

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

var backendUrl = Environment.GetEnvironmentVariable("BACKEND_URL") ?? "http://localhost:40114";
Console.WriteLine($"Starting Backend on: {backendUrl}");

app.Run(backendUrl);
