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

// TODO manually kick off a transcription, for testing
var avp = app.Services.GetService<IAvProcessingService>();

if (avp is null)
    throw new InvalidOperationException($"Failed to find a registered {typeof(IAvProcessingService).Name} service");

var doShowcase = await avp.Init("SPEECHMATICS_API_KEY");

Console.WriteLine($"{(doShowcase ? "Doing" : "Not doing")} the Speechmatics API showcase");

// stressed and exhausted, the compiler is forcing my hand:
// errors on this variable being unset at the later await, even though it will definitely be set when it needs to await it
// thus initialise to null and cast away nullness during the await
Task<bool>? audioTranscription = null;
if (doShowcase)
    audioTranscription = avp.TranscribeAudio("./tagesschau_clip.aac");

app.Run();

if (doShowcase)
{
    var transcriptionSuccess = await audioTranscription!;
    Console.WriteLine($"Speechmatics communication was a {(transcriptionSuccess ? "success" : "failure")}");
}