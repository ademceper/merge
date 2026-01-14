using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Support.Commands.CreateCustomerCommunication;
using Merge.Application.Support.Commands.UpdateCustomerCommunicationStatus;
using Merge.Application.Support.Queries.GetCustomerCommunication;
using Merge.Application.Support.Queries.GetUserCustomerCommunications;
using Merge.Application.Support.Queries.GetUserCommunicationHistory;
using Merge.Application.Support.Queries.GetAllCustomerCommunications;
using Merge.Application.Support.Queries.GetCustomerCommunicationStats;
using Merge.API.Middleware;
using Merge.API.Helpers;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
// ✅ BOLUM 4.1.3: HATEOAS (ZORUNLU)
namespace Merge.API.Controllers.Support;

/// <summary>
/// Customer Communications Controller - Manages customer communications
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/communications")]
[Authorize]
public class CustomerCommunicationsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly SupportSettings _settings;

    public CustomerCommunicationsController(
        IMediator mediator,
        IOptions<SupportSettings> settings)
    {
        _mediator = mediator;
        _settings = settings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("my-communications", Name = "GetMyCommunications")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<CustomerCommunicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CustomerCommunicationDto>>> GetMyCommunications(
        [FromQuery] string? communicationType = null,
        [FromQuery] string? channel = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        if (pageSize <= 0) pageSize = _settings.DefaultPageSize;
        if (pageSize > _settings.MaxPageSize) pageSize = _settings.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetUserCustomerCommunicationsQuery(userId, communicationType, channel, page, pageSize);
        var communications = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each communication and pagination links
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = communications.Items.Select(communication =>
        {
            var links = HateoasHelper.CreateCustomerCommunicationLinks(Url, communication.Id, version);
            return communication with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetMyCommunications",
            communications.Page,
            communications.PageSize,
            communications.TotalPages,
            new { communicationType, channel },
            version);
        communications.Items = updatedItems;
        communications.Links = paginationLinks.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return Ok(communications);
    }

    /// <summary>
    /// Gets communication history for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Communication history summary</returns>
    /// <response code="200">History retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("my-history")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(CommunicationHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommunicationHistoryDto>> GetMyHistory(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetUserCommunicationHistoryQuery(userId);
        var history = await _mediator.Send(query, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Gets a customer communication by ID
    /// </summary>
    /// <param name="id">Communication ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The communication</returns>
    /// <response code="200">Communication found</response>
    /// <response code="404">Communication not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to access this communication</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}", Name = "GetCommunication")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(CustomerCommunicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerCommunicationDto>> GetCommunication(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi communication'larına erişebilmeli veya admin olmalı
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var userId = !isAdmin ? GetUserIdOrNull() : null;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetCustomerCommunicationQuery(id, userId);
        var communication = await _mediator.Send(query, cancellationToken);
        if (communication == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR koruması - Double check (ekstra güvenlik)
        if (!isAdmin && communication.UserId != userId)
        {
            return Forbid();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateCustomerCommunicationLinks(Url, communication.Id, version);
        communication = communication with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(communication);
    }

    /// <summary>
    /// Creates a new customer communication (Admin/Manager/Support only)
    /// </summary>
    /// <param name="dto">Communication creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created communication</returns>
    /// <response code="201">Communication created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // Admin endpoints
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(CustomerCommunicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerCommunicationDto>> CreateCommunication(
        [FromBody] CreateCustomerCommunicationDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sentByUserId = GetUserIdOrNull();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CreateCustomerCommunicationCommand(
            dto.UserId,
            dto.CommunicationType,
            dto.Channel,
            dto.Subject,
            dto.Content,
            dto.Direction,
            dto.RelatedEntityId,
            dto.RelatedEntityType,
            sentByUserId,
            dto.RecipientEmail,
            dto.RecipientPhone,
            dto.Metadata);
        var communication = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateCustomerCommunicationLinks(Url, communication.Id, version);
        communication = communication with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetCommunication), new { version, id = communication.Id }, communication);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet(Name = "GetAllCommunications")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<CustomerCommunicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CustomerCommunicationDto>>> GetAllCommunications(
        [FromQuery] string? communicationType = null,
        [FromQuery] string? channel = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        if (pageSize <= 0) pageSize = _settings.DefaultPageSize;
        if (pageSize > _settings.MaxPageSize) pageSize = _settings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetAllCustomerCommunicationsQuery(
            communicationType,
            channel,
            userId,
            startDate,
            endDate,
            page,
            pageSize);
        var communications = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each communication and pagination links
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = communications.Items.Select(communication =>
        {
            var links = HateoasHelper.CreateCustomerCommunicationLinks(Url, communication.Id, version);
            return communication with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetAllCommunications",
            communications.Page,
            communications.PageSize,
            communications.TotalPages,
            new { communicationType, channel, userId, startDate, endDate },
            version);
        communications.Items = updatedItems;
        communications.Links = paginationLinks.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return Ok(communications);
    }

    /// <summary>
    /// Gets communication history for a specific user (Admin/Manager/Support only)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Communication history summary</returns>
    /// <response code="200">History retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("user/{userId}/history")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(CommunicationHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommunicationHistoryDto>> GetUserHistory(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetUserCommunicationHistoryQuery(userId);
        var history = await _mediator.Send(query, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Updates the status of a customer communication (Admin/Manager/Support only)
    /// </summary>
    /// <param name="id">Communication ID</param>
    /// <param name="dto">Status update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Status updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Communication not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut("{id}/status", Name = "UpdateStatus")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateCommunicationStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new UpdateCustomerCommunicationStatusCommand(
            id,
            dto.Status,
            dto.DeliveredAt,
            dto.ReadAt);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Gets customer communication statistics (Admin/Manager only)
    /// </summary>
    /// <param name="startDate">Start date for statistics (optional)</param>
    /// <param name="endDate">End date for statistics (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Communication statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetCustomerCommunicationStatsQuery(startDate, endDate);
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}

