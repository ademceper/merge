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

namespace Merge.API.Controllers.Support;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/tickets")]
public class SupportTicketsController(IMediator mediator, IOptions<SupportSettings> supportSettings) : BaseController
{
    private readonly SupportSettings _supportSettings = supportSettings.Value;

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
                var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                var command = new CreateTicketCommand(
            userId,
            dto.Category,
            dto.Priority,
            dto.Subject,
            dto.Description,
            dto.OrderId,
            dto.ProductId);
        var ticket = await mediator.Send(command, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
        ticket = ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetTicket), new { version, id = ticket.Id }, ticket);
    }

    [HttpGet("{id}", Name = "GetTicket")]
    [Authorize]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(SupportTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SupportTicketDto>> GetTicket(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var userId = !isAdmin ? GetUserIdOrNull() : null;

                var query = new GetTicketQuery(id, userId);
        var ticket = await mediator.Send(query, cancellationToken);
        if (ticket == null)
        {
            return NotFound();
        }

                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
        ticket = ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(ticket);
    }

    [HttpGet("number/{ticketNumber}")]
    [Authorize]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(SupportTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SupportTicketDto>> GetTicketByNumber(
        string ticketNumber,
        CancellationToken cancellationToken = default)
    {
                        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var userId = !isAdmin ? GetUserIdOrNull() : null;

                var query = new GetTicketByNumberQuery(ticketNumber, userId);
        var ticket = await mediator.Send(query, cancellationToken);
        if (ticket == null)
        {
            return NotFound();
        }

                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
        ticket = ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(ticket);
    }

    [HttpGet("my-tickets", Name = "GetMyTickets")]
    [Authorize]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(PagedResult<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SupportTicketDto>>> GetMyTickets(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
                                if (pageSize <= 0) pageSize = _supportSettings.DefaultPageSize;
        if (pageSize > _supportSettings.MaxPageSize) pageSize = _supportSettings.MaxPageSize;
        if (page < 1) page = 1;
        
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                var query = new GetUserTicketsQuery(userId, status, page, pageSize);
        var tickets = await mediator.Send(query, cancellationToken);
        
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

    [HttpGet(Name = "GetAllTickets")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(PagedResult<SupportTicketDto>), StatusCodes.Status200OK)]
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
                                if (pageSize <= 0) pageSize = _supportSettings.DefaultPageSize;
        if (pageSize > _supportSettings.MaxPageSize) pageSize = _supportSettings.MaxPageSize;
        if (page < 1) page = 1;

                var query = new GetAllTicketsQuery(status, category, assignedToId, page, pageSize);
        var tickets = await mediator.Send(query, cancellationToken);
        
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

    [HttpPut("{id}", Name = "UpdateTicket")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
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
                var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

                var command = new UpdateTicketCommand(
            id,
            dto.Subject,
            dto.Description,
            dto.Category,
            dto.Priority,
            dto.Status,
            dto.AssignedToId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/assign", Name = "AssignTicket")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
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
                var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

                var command = new AssignTicketCommand(id, dto.AssignedToId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/close", Name = "CloseTicket")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CloseTicket(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var command = new CloseTicketCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/reopen", Name = "ReopenTicket")]
    [Authorize]
    [RateLimit(10, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReopenTicket(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var command = new ReopenTicketCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("messages", Name = "AddMessage")]
    [Authorize]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(TicketMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TicketMessageDto>> AddMessage(
        [FromBody] CreateTicketMessageDto dto,
        CancellationToken cancellationToken = default)
    {
                var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var isStaffResponse = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
                var command = new AddMessageCommand(userId, dto.TicketId, dto.Message, isStaffResponse, dto.IsInternal);
        var message = await mediator.Send(command, cancellationToken);

        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetTicketMessages), new { version, id = dto.TicketId }, message);
    }

    [HttpGet("{id}/messages", Name = "GetTicketMessages")]
    [Authorize]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<TicketMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<TicketMessageDto>>> GetTicketMessages(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                var isStaff = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
                var query = new GetTicketMessagesQuery(id, isStaff);
        var messages = await mediator.Send(query, cancellationToken);

        return Ok(messages);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(TicketStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TicketStatsDto>> GetTicketStats(
        CancellationToken cancellationToken = default)
    {
                        var query = new GetTicketStatsQuery();
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    [HttpGet("unassigned")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetUnassignedTickets(
        CancellationToken cancellationToken = default)
    {
                        var query = new GetUnassignedTicketsQuery();
        var tickets = await mediator.Send(query, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        tickets = tickets.Select(ticket =>
        {
            var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
            return ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        return Ok(tickets);
    }

    [HttpGet("my-assigned")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<SupportTicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetMyAssignedTickets(
        CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                var query = new GetMyAssignedTicketsQuery(userId);
        var tickets = await mediator.Send(query, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        tickets = tickets.Select(ticket =>
        {
            var links = HateoasHelper.CreateSupportTicketLinks(Url, ticket.Id, version);
            return ticket with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        return Ok(tickets);
    }

    [HttpGet("agent/dashboard")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(SupportAgentDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SupportAgentDashboardDto>> GetAgentDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var agentId))
        {
            return Unauthorized();
        }

                var query = new GetAgentDashboardQuery(agentId, startDate, endDate);
        var dashboard = await mediator.Send(query, cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("agent/categories")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(List<CategoryTicketCountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<CategoryTicketCountDto>>> GetTicketsByCategory(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetTicketsByCategoryQuery(agentId, startDate, endDate);
        var categories = await mediator.Send(query, cancellationToken);
        return Ok(categories);
    }

    [HttpGet("agent/priorities")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(List<PriorityTicketCountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<PriorityTicketCountDto>>> GetTicketsByPriority(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetTicketsByPriorityQuery(agentId, startDate, endDate);
        var priorities = await mediator.Send(query, cancellationToken);
        return Ok(priorities);
    }

    [HttpGet("agent/trends")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(List<TicketTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<TicketTrendDto>>> GetTicketTrends(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetTicketTrendsQuery(agentId, startDate, endDate);
        var trends = await mediator.Send(query, cancellationToken);
        return Ok(trends);
    }
}
