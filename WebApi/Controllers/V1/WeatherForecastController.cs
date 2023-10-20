using Application.Weather.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Backend.WebApi.Controllers.V1;

/// <summary>
/// </summary>
[ApiController]
public class WeatherForecastController : V1ApiController
{
    private readonly ILogger<WeatherForecastController> _logger;

    /// <summary>
    /// </summary>
    /// <param name="logger"></param>
    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Gets the weather
    /// </summary>
    /// <param name="dto">The dto.</param>
    /// <returns></returns>
    [HttpGet(Name = "getWeather")]
    public async Task<List<string>> Get()
    {
        return await Mediator.Send(new GetWeatherQuery());
    }
}