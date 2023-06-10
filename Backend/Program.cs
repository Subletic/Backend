using Backend.Hubs;
using Backend.Services;
using Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddSingleton<ISpeechBubbleListService, SpeechBubbleListService>();

builder.Services.AddHostedService<BufferTimeMonitor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // Replace with your Angular app URL
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

AvProcessing avprocessing = await AvProcessing.Init ("SPEECHMATICS_API_KEY");
// test
string testAudioFile = "/home/puna/tmp/tagesschau_clip.aac";
Task<bool> audioTranscription = avprocessing.TranscribeAudio (testAudioFile);

app.Run();

await audioTranscription;
