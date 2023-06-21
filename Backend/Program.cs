using Backend.Hubs;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";

// Add services to the container.
// TODO this used to be Services.AddControllers(). is it bad to do this instead?
builder.Services.AddMvc().AddControllersAsServices();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ISpeechBubbleListService, SpeechBubbleListService>();

builder.Services.AddSingleton<IAvProcessingService, AvProcessingService>();

builder.Services.AddSingleton<IWordProcessingService, WordProcessingService>();

builder.Services.AddHostedService<BufferTimeMonitor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend",
        builder =>
        {
            builder.WithOrigins(frontendUrl) // Replace with your Angular app URL
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
