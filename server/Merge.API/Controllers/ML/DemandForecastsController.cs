using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.Application.ML.Commands.ForecastDemand;
using Merge.Application.ML.Queries.ForecastDemandForCategory;
using Merge.Application.ML.Queries.GetForecastStats;
using Merge.API.Middleware;

namespace Merge.API.Controllers.ML;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/ml/demand-forecasting")]
[Authorize(Roles = "Admin,Manager")]
public class DemandForecastsController(IMediator mediator) : BaseController
{
    [HttpPost("products/{productId}")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(DemandForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DemandForecastDto>> ForecastDemand(
        Guid productId,
        [FromQuery] int forecastDays = 30,
        CancellationToken cancellationToken = default)
    {
        var command = new ForecastDemandCommand(productId, forecastDays);
        var forecast = await mediator.Send(command, cancellationToken);
        return Ok(forecast);
    }

    [HttpPost("categories/{categoryId}")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(PagedResult<DemandForecastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<DemandForecastDto>>> ForecastDemandForCategory(
        Guid categoryId,
        [FromQuery] int forecastDays = 30,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new ForecastDemandForCategoryQuery(categoryId, forecastDays, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("stats")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(DemandForecastStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DemandForecastStatsDto>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetForecastStatsQuery(startDate, endDate);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
