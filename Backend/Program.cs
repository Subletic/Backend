using System;
using System.IO;

using Backend.Hubs;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);
var frontend_url = Environment.GetEnvironmentVariable("FRONTEND_URL");

if (frontend_url == null)
{
    frontend_url = "http://localhost:4200";
}

// Add services to the container.

// TODO this used to be Services.AddControllers(). is it bad to do this instead?
builder.Services.AddMvc().AddControllersAsServices();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ISpeechBubbleListService, SpeechBubbleListService>();

builder.Services.AddSingleton<IAvProcessingService, AvProcessingService>();

builder.Services.AddHostedService<BufferTimeMonitor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend",
        builder =>
        {
            builder.WithOrigins(frontend_url) // Replace with your Angular app URL
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

/*
AvProcessing avprocessing = await AvProcessing.Init ("SPEECHMATICS_API_KEY");
// test
string testAudioFile = "./tagesschau_clip.aac";
Task<bool> audioTranscription = avprocessing.TranscribeAudio (testAudioFile);
*/

// TODO manually kick off a transcription, for testing
IAvProcessingService? avp = app.Services.GetService<IAvProcessingService>();
if (avp is null)
    throw new InvalidOperationException ($"Failed to find a registered {typeof (IAvProcessingService).Name} service");
bool doShowcase = await avp!.Init ("SPEECHMATICS_API_KEY");
Console.WriteLine (String.Format (
    "{0} the Speechmatics API showcase", doShowcase ? "Doing" : "Not doing"));

// stressed and exhausted, the compiler is forcing my hand:
// errors on this variable being unset at the later await, even though it will definitely be set when it needs to await it
// thus initialise to null and cast away nullness during the await
Task<bool>? audioTranscription = null;
if (doShowcase)
    audioTranscription = avp.TranscribeAudio ("./tagesschau_clip.aac");

app.Run();

if (doShowcase)
{
    bool transcriptionSuccess = await audioTranscription!;
    Console.WriteLine (String.Format ("Speechmatics communication was a {0}", transcriptionSuccess ? "success" : "failure"));
}
