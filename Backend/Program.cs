using System;
using Backend.Hubs;
using Backend.Services;
using Serilog;
using Serilog.Core;

var builder = WebApplication.CreateBuilder(args);

// If frontend URL is not specified, use default value (localhost:4200)
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";

// Reads configuration information from appsettings.json and appsettings.Production.json (or another environment file).
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
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

builder.Services.AddSingleton<ICustomDictionaryService, CustomDictionaryService>();

builder.Services.AddSingleton<FrontendAudioQueueService, FrontendAudioQueueService>();

builder.Services.AddHostedService<StartupService>();

builder.Services.AddHostedService<BufferTimeMonitor>();

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

app.MapHub<CommunicationHub>("/communicationHub");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
