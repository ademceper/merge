using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Support.Commands.CreateTicket;
using Merge.Application.Support.Commands.UpdateTicket;
using Merge.Application.Support.Commands.AssignTicket;
using Merge.Application.Support.Commands.CloseTicket;
using Merge.Application.Support.Commands.ReopenTicket;
using Merge.Application.Support.Commands.AddMessage;
using Merge.Application.Support.Queries.GetTicket;
using Merge.Application.Support.Queries.GetTicketByNumber;
using Merge.Application.Support.Queries.GetUserTickets;
using Merge.Application.Support.Queries.GetAllTickets;
using Merge.Application.Support.Queries.GetTicketMessages;
using Merge.Application.Support.Queries.GetTicketStats;
using Merge.Application.Support.Queries.GetUnassignedTickets;
using Merge.Application.Support.Queries.GetMyAssignedTickets;
using Merge.Application.Support.Queries.GetAgentDashboard;
using Merge.Application.Support.Queries.GetTicketsByCategory;
using Merge.Application.Support.Queries.GetTicketsByPriority;
using Merge.Application.Support.Queries.GetTicketTrends;
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
/// Support Tickets Controller - Manages support ticket operations
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/tickets")]
public class SupportTicketsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly SupportSettings _settings;

    public SupportTicketsController(
        IMediator mediator,
        IOptions<SupportSettings> settings)
    {
        _mediator = mediator;
        _settings = settings.Value;
    }

    /// <summary>
    /// Creates a new support ticket
    /// </summary>
    /// <param name="dto">Ticket creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created ticket</returns>
    /// <response code="201">Ticket created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU) - 10 destek talebi / saat (spam koruması)
    [HttpPost]
    [Authorize]
    [RateLimit(10, 3600)]
    [ProducesResponseType(typeof(SupportTicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SupportTicketDto>> CreateTicket(
        [FromBody] CreateSupportTicketDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CreateTicketCommand(
            userId,
            dto.Category,
            dto.Priority,
            dto.Subject,
            dto.Description,
            dto.OrderId,
            dto.ProductId);
        var ticket = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
        ticket = ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetTicket), new { version, id = ticket.Id }, ticket);
    }

    /// <summary>
    /// Gets a support ticket by ID
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ticket</returns>
    /// <response code="200">Ticket found</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to access this ticket</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}", Name = "GetTicket")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(SupportTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SupportTicketDto>> GetTicket(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR koruması
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var userId = !isAdmin ? GetUserIdOrNull() : null;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetTicketQuery(id, userId);
        var ticket = await _mediator.Send(query, cancellationToken);
        if (ticket == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
        ticket = ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(ticket);
    }

    /// <summary>
    /// Gets a support ticket by ticket number
    /// </summary>
    /// <param name="ticketNumber">Ticket number (e.g., TKT-000001)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ticket</returns>
    /// <response code="200">Ticket found</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to access this ticket</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("number/{ticketNumber}")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(SupportTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SupportTicketDto>> GetTicketByNumber(
        string ticketNumber,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR koruması
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var userId = !isAdmin ? GetUserIdOrNull() : null;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetTicketByNumberQuery(ticketNumber, userId);
        var ticket = await _mediator.Send(query, cancellationToken);
        if (ticket == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
        ticket = ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(ticket);
    }

    /// <summary>
    /// Gets the authenticated user's support tickets
    /// </summary>
    /// <param name="status">Filter by status (Open, InProgress, Resolved, Closed, etc.)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of tickets</returns>
    /// <response code="200">Tickets retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="400">Invalid pagination parameters</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("my-tickets", Name = "GetMyTickets")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SupportTicketDto>>> GetMyTickets(
        [FromQuery] string? status = null,
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
        
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetUserTicketsQuery(userId, status, page, pageSize);
        var tickets = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each ticket and pagination links
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = tickets.Items.Select(ticket =>
        {
            var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
            return ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetMyTickets",
            tickets.Page,
            tickets.PageSize,
            tickets.TotalPages,
            new { status },
            version);
        tickets.Items = updatedItems;
        tickets.Links = paginationLinks.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return Ok(tickets);
    }

    /// <summary>
    /// Gets all support tickets (Admin/Manager/Support only)
    /// </summary>
    /// <param name="status">Filter by status</param>
    /// <param name="category">Filter by category</param>
    /// <param name="assignedToId">Filter by assigned agent ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of tickets</returns>
    /// <response code="200">Tickets retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet(Name = "GetAllTickets")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SupportTicketDto>>> GetAllTickets(
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? assignedToId = null,
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
        var query = new GetAllTicketsQuery(status, category, assignedToId, page, pageSize);
        var tickets = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each ticket and pagination links
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = tickets.Items.Select(ticket =>
        {
            var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
            return ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetAllTickets",
            tickets.Page,
            tickets.PageSize,
            tickets.TotalPages,
            new { status, category, assignedToId },
            version);
        tickets.Links = paginationLinks.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return Ok(tickets);
    }

    /// <summary>
    /// Updates a support ticket
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <param name="dto">Ticket update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Ticket updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut("{id}", Name = "UpdateTicket")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateTicket(
        Guid id,
        [FromBody] UpdateSupportTicketDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new UpdateTicketCommand(
            id,
            dto.Subject,
            dto.Description,
            dto.Category,
            dto.Priority,
            dto.Status,
            dto.AssignedToId);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Assigns a support ticket to an agent
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <param name="dto">Assignment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Ticket assigned successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/assign", Name = "AssignTicket")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AssignTicket(
        Guid id,
        [FromBody] AssignTicketDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new AssignTicketCommand(id, dto.AssignedToId);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Closes a support ticket
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Ticket closed successfully</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/close", Name = "CloseTicket")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CloseTicket(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CloseTicketCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Reopens a closed or resolved support ticket
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Ticket reopened successfully</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/reopen", Name = "ReopenTicket")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReopenTicket(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new ReopenTicketCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Adds a message to a support ticket
    /// </summary>
    /// <param name="dto">Message data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created message</returns>
    /// <response code="201">Message added successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("messages", Name = "AddMessage")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (spam koruması)
    [ProducesResponseType(typeof(TicketMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TicketMessageDto>> AddMessage(
        [FromBody] CreateTicketMessageDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var isStaffResponse = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new AddMessageCommand(userId, dto.TicketId, dto.Message, isStaffResponse, dto.IsInternal);
        var message = await _mediator.Send(command, cancellationToken);

        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetTicketMessages), new { version, id = dto.TicketId }, message);
    }

    /// <summary>
    /// Gets all messages for a support ticket
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of messages</returns>
    /// <response code="200">Messages retrieved successfully</response>
    /// <response code="404">Ticket not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}/messages", Name = "GetTicketMessages")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<TicketMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<TicketMessageDto>>> GetTicketMessages(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetTicketMessagesQuery(id, isStaff);
        var messages = await _mediator.Send(query, cancellationToken);

        return Ok(messages);
    }

    /// <summary>
    /// Gets support ticket statistics (Admin/Manager/Support only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Ticket statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(TicketStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TicketStatsDto>> GetTicketStats(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetTicketStatsQuery();
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Gets all unassigned support tickets (Admin/Manager/Support only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of unassigned tickets</returns>
    /// <response code="200">Unassigned tickets retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("unassigned")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetUnassignedTickets(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetUnassignedTicketsQuery();
        var tickets = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each ticket
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        tickets = tickets.Select(ticket =>
        {
            var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
            return ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        return Ok(tickets);
    }

    /// <summary>
    /// Gets all tickets assigned to the authenticated agent (Admin/Manager/Support only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of assigned tickets</returns>
    /// <response code="200">Assigned tickets retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("my-assigned")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetMyAssignedTickets(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetMyAssignedTicketsQuery(userId);
        var tickets = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each ticket
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        tickets = tickets.Select(ticket =>
        {
            var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
            return ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        return Ok(tickets);
    }

    /// <summary>
    /// Gets agent dashboard statistics and data (Admin/Manager/Support only)
    /// </summary>
    /// <param name="startDate">Start date for statistics (optional)</param>
    /// <param name="endDate">End date for statistics (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent dashboard data</returns>
    /// <response code="200">Dashboard data retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("agent/dashboard")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(SupportAgentDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SupportAgentDashboardDto>> GetAgentDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var agentId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetAgentDashboardQuery(agentId, startDate, endDate);
        var dashboard = await _mediator.Send(query, cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Gets ticket counts grouped by category (Admin/Manager/Support only)
    /// </summary>
    /// <param name="agentId">Filter by agent ID (optional)</param>
    /// <param name="startDate">Start date for filtering (optional)</param>
    /// <param name="endDate">End date for filtering (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of category ticket counts</returns>
    /// <response code="200">Category counts retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("agent/categories")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(List<CategoryTicketCountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CategoryTicketCountDto>>> GetTicketsByCategory(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetTicketsByCategoryQuery(agentId, startDate, endDate);
        var categories = await _mediator.Send(query, cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Gets ticket counts grouped by priority (Admin/Manager/Support only)
    /// </summary>
    /// <param name="agentId">Filter by agent ID (optional)</param>
    /// <param name="startDate">Start date for filtering (optional)</param>
    /// <param name="endDate">End date for filtering (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of priority ticket counts</returns>
    /// <response code="200">Priority counts retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("agent/priorities")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(List<PriorityTicketCountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<PriorityTicketCountDto>>> GetTicketsByPriority(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetTicketsByPriorityQuery(agentId, startDate, endDate);
        var priorities = await _mediator.Send(query, cancellationToken);
        return Ok(priorities);
    }

    /// <summary>
    /// Gets ticket trends over time (Admin/Manager/Support only)
    /// </summary>
    /// <param name="agentId">Filter by agent ID (optional)</param>
    /// <param name="startDate">Start date for trends (optional)</param>
    /// <param name="endDate">End date for trends (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of ticket trends</returns>
    /// <response code="200">Trends retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("agent/trends")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(List<TicketTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TicketTrendDto>>> GetTicketTrends(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetTicketTrendsQuery(agentId, startDate, endDate);
        var trends = await _mediator.Send(query, cancellationToken);
        return Ok(trends);
    }
}
