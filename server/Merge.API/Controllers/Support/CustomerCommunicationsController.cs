using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Exceptions;
using Merge.Application.Support.Commands.CreateCustomerCommunication;
using Merge.Application.Support.Commands.UpdateCustomerCommunicationStatus;
using Merge.Application.Support.Queries.GetCustomerCommunication;
using Merge.Application.Support.Queries.GetUserCustomerCommunications;
using Merge.Application.Support.Queries.GetUserCommunicationHistory;
using Merge.Application.Support.Queries.GetAllCustomerCommunications;
using Merge.Application.Support.Queries.GetCustomerCommunicationStats;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Support;

/// <summary>
/// Customer Communications API endpoints.
/// Müşteri iletişimlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/communications")]
[Authorize]
[Tags("CustomerCommunications")]
public class CustomerCommunicationsController(IMediator mediator, IOptions<SupportSettings> supportSettings) : BaseController
{
    private readonly SupportSettings _supportSettings = supportSettings.Value;

    [HttpGet("my-communications", Name = "GetMyCommunications")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(PagedResult<CustomerCommunicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CustomerCommunicationDto>>> GetMyCommunications(
        [FromQuery] string? communicationType = null,
        [FromQuery] string? channel = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
                                if (pageSize <= 0) pageSize = _supportSettings.DefaultPageSize;
        if (pageSize > _supportSettings.MaxPageSize) pageSize = _supportSettings.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
                var query = new GetUserCustomerCommunicationsQuery(userId, communicationType, channel, page, pageSize);
        var communications = await mediator.Send(query, cancellationToken);
        
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

    [HttpGet("my-history")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(CommunicationHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommunicationHistoryDto>> GetMyHistory(
        CancellationToken cancellationToken = default)
    {
                var userId = GetUserId();
                var query = new GetUserCommunicationHistoryQuery(userId);
        var history = await mediator.Send(query, cancellationToken);
        return Ok(history);
    }

    [HttpGet("{id}", Name = "GetCommunication")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(CustomerCommunicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerCommunicationDto>> GetCommunication(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var userId = !isAdmin ? GetUserIdOrNull() : null;

                var query = new GetCustomerCommunicationQuery(id, userId);
        var communication = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("CustomerCommunication", id);

                if (!isAdmin && communication.UserId != userId)
        {
            return Forbid();
        }

                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateCustomerCommunicationLinks(Url, communication.Id, version);
        communication = communication with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(communication);
    }

    // Admin endpoints
                [HttpPost]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(CustomerCommunicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CustomerCommunicationDto>> CreateCommunication(
        [FromBody] CreateCustomerCommunicationDto dto,
        CancellationToken cancellationToken = default)
    {
                var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var sentByUserId = GetUserIdOrNull();
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
        var communication = await mediator.Send(command, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateCustomerCommunicationLinks(Url, communication.Id, version);
        communication = communication with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetCommunication), new { version, id = communication.Id }, communication);
    }

    [HttpGet(Name = "GetAllCommunications")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(PagedResult<CustomerCommunicationDto>), StatusCodes.Status200OK)]
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
                                if (pageSize <= 0) pageSize = _supportSettings.DefaultPageSize;
        if (pageSize > _supportSettings.MaxPageSize) pageSize = _supportSettings.MaxPageSize;
        if (page < 1) page = 1;

                var query = new GetAllCustomerCommunicationsQuery(
            communicationType,
            channel,
            userId,
            startDate,
            endDate,
            page,
            pageSize);
        var communications = await mediator.Send(query, cancellationToken);
        
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

    [HttpGet("user/{userId}/history")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(CommunicationHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommunicationHistoryDto>> GetUserHistory(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetUserCommunicationHistoryQuery(userId);
        var history = await mediator.Send(query, cancellationToken);
        return Ok(history);
    }

    [HttpPut("{id}/status", Name = "UpdateStatus")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
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
                var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

                var command = new UpdateCustomerCommunicationStatusCommand(
            id,
            dto.Status,
            dto.DeliveredAt,
            dto.ReadAt);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("CustomerCommunication", id);

        return NoContent();
    }

    /// <summary>
    /// Müşteri iletişim durumunu kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchStatus(
        Guid id,
        [FromBody] PatchCommunicationStatusDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        // Status is required for UpdateCustomerCommunicationStatusCommand, but we can make it optional for PATCH
        // If status is not provided, we'll skip the update
        if (string.IsNullOrEmpty(patchDto.Status) && !patchDto.DeliveredAt.HasValue && !patchDto.ReadAt.HasValue)
        {
            return BadRequest("En az bir alan güncellenmelidir.");
        }

        var command = new UpdateCustomerCommunicationStatusCommand(
            id,
            patchDto.Status ?? "Sent", // Default status if not provided
            patchDto.DeliveredAt,
            patchDto.ReadAt);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("CustomerCommunication", id);

        return NoContent();
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetCustomerCommunicationStatsQuery(startDate, endDate);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
