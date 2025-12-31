using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Services;
using Merge.Application.Interfaces.ML;
using Merge.Application.DTOs.Analytics;


namespace Merge.API.Controllers.ML;

[ApiController]
[Route("api/ml/demand-forecasting")]
[Authorize(Roles = "Admin,Manager")]
public class DemandForecastsController : BaseController
{
    private readonly IDemandForecastingService _demandForecastingService;
        public DemandForecastsController(IDemandForecastingService demandForecastingService)
    {
        _demandForecastingService = demandForecastingService;
            }

    [HttpPost("products/{productId}")]
    public async Task<ActionResult<DemandForecastDto>> ForecastDemand(Guid productId, [FromQuery] int forecastDays = 30)
    {
        var forecast = await _demandForecastingService.ForecastDemandAsync(productId, forecastDays);
        return Ok(forecast);
    }

    [HttpPost("categories/{categoryId}")]
    public async Task<ActionResult<IEnumerable<DemandForecastDto>>> ForecastDemandForCategory(Guid categoryId, [FromQuery] int forecastDays = 30)
    {
        var forecasts = await _demandForecastingService.ForecastDemandForCategoryAsync(categoryId, forecastDays);
        return Ok(forecasts);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DemandForecastStatsDto>> GetStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var stats = await _demandForecastingService.GetForecastStatsAsync(startDate, endDate);
        return Ok(stats);
    }
}

