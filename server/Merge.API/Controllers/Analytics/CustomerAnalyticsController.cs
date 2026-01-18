using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Analytics.Queries.GetCustomerAnalytics;
using Merge.Application.Analytics.Queries.GetCustomerLifetimeValue;
using Merge.Application.Analytics.Queries.GetCustomerSegments;
using Merge.Application.Analytics.Queries.GetTopCustomers;

namespace Merge.API.Controllers.Analytics.Customer;

/// <summary>
/// Customer Analytics API endpoints.
/// Müşteri analitiklerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/analytics/customers")]
[Authorize(Roles = "Admin,Manager")]
[Tags("CustomerAnalytics")]
public class CustomerAnalyticsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Müşteri analitiklerini getirir
    /// </summary>
    [HttpGet("customers")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(CustomerAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerAnalyticsDto>> GetCustomerAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerAnalyticsQuery(startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// En çok harcama yapan müşterileri getirir
    /// </summary>
    [HttpGet("customers/top")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(List<TopCustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TopCustomerDto>>> GetTopCustomers(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit > paginationSettings.Value.MaxPageSize) limit = paginationSettings.Value.MaxPageSize;
        if (limit < 1) limit = 1;

        var query = new GetTopCustomersQuery(limit);
        var customers = await mediator.Send(query, cancellationToken);
        return Ok(customers);
    }

    /// <summary>
    /// Müşteri segmentlerini getirir
    /// </summary>
    [HttpGet("customers/segments")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (ağır işlem)
    [ProducesResponseType(typeof(List<CustomerSegmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CustomerSegmentDto>>> GetCustomerSegments(
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerSegmentsQuery();
        var segments = await mediator.Send(query, cancellationToken);
        return Ok(segments);
    }

    /// <summary>
    /// Müşteri yaşam boyu değerini getirir
    /// </summary>
    [HttpGet("customers/{customerId}/lifetime-value")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(CustomerLifetimeValueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerLifetimeValueDto>> GetCustomerLifetimeValue(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            var currentUserId = GetUserId();
            if (customerId != currentUserId)
            {
                return Forbid();
            }
        }

        var query = new GetCustomerLifetimeValueQuery(customerId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    
}
