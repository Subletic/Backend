using Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

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

var test = new CommunicationHub();

app.UseRouting();

app.MapGet("/test", () => test.SendMessage("test"));

app.UseAuthorization();

app.MapControllers();

app.Run();

