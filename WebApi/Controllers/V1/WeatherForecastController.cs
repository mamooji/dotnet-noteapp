using Application.Weather.Queries;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.V1;

[ApiController]
public class WeatherForecastController : V1ApiController
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "getWeather")]
    public async Task<List<string>> Get()
    {
        
        return await Mediator.Send(new GetWeatherQuery());
    }
}