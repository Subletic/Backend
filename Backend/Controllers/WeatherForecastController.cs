using Backend.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Controllers;

[ApiController]
[Route("/Controller")]
public class WeatherForecastController : ControllerBase
{
    private readonly IHubContext<CommunicationHub> _hubContext;
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IHubContext<CommunicationHub> hubContext)
    {
        _hubContext = hubContext;
        _logger = logger;
        Console.Out.WriteLine("hallo");
        Send();
    }
    
    public async Task Send()
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "hallo angular");
    }


    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}
