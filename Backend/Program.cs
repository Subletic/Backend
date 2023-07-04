using Backend.Hubs;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// If frontend URL is not specified, use default value (localhost:4200)
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ISpeechBubbleListService, SpeechBubbleListService>();

builder.Services.AddSingleton<IAvProcessingService, AvProcessingService>();

builder.Services.AddSingleton<IWordProcessingService, WordProcessingService>();

builder.Services.AddSingleton<FrontendAudioQueueService, FrontendAudioQueueService>();

builder.Services.AddSingleton<WebVttExporter>();

builder.Services.AddSingleton<Stream>(new MemoryStream());

builder.Services.AddHostedService<BufferTimeMonitor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend",
        policy =>
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

app.UseRouting();

app.UseCors("AllowAngularFrontend");

app.MapHub<CommunicationHub>("/communicationHub");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();