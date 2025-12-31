using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.DTOs.Support;


namespace Merge.API.Controllers.Support;

[ApiController]
[Route("api/support/tickets")]
public class SupportTicketsController : BaseController
{
    private readonly ISupportTicketService _supportTicketService;

    public SupportTicketsController(ISupportTicketService supportTicketService)
    {
        _supportTicketService = supportTicketService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SupportTicketDto>> CreateTicket([FromBody] CreateSupportTicketDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var ticket = await _supportTicketService.CreateTicketAsync(userId, dto);
        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<SupportTicketDto>> GetTicket(Guid id)
    {
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var userId = !isAdmin ? GetUserIdOrNull() : null;

        var ticket = await _supportTicketService.GetTicketAsync(id, userId);
        if (ticket == null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpGet("number/{ticketNumber}")]
    [Authorize]
    public async Task<ActionResult<SupportTicketDto>> GetTicketByNumber(string ticketNumber)
    {
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var userId = !isAdmin ? GetUserIdOrNull() : null;

        var ticket = await _supportTicketService.GetTicketByNumberAsync(ticketNumber, userId);
        if (ticket == null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpGet("my-tickets")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetMyTickets([FromQuery] string? status = null)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var tickets = await _supportTicketService.GetUserTicketsAsync(userId, status);
        return Ok(tickets);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetAllTickets(
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var tickets = await _supportTicketService.GetAllTicketsAsync(status, category, assignedToId, page, pageSize);
        return Ok(tickets);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] UpdateSupportTicketDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _supportTicketService.UpdateTicketAsync(id, dto);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<IActionResult> AssignTicket(Guid id, [FromBody] AssignTicketDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _supportTicketService.AssignTicketAsync(id, dto.AssignedToId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/close")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<IActionResult> CloseTicket(Guid id)
    {
        var success = await _supportTicketService.CloseTicketAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/reopen")]
    [Authorize]
    public async Task<IActionResult> ReopenTicket(Guid id)
    {
        var success = await _supportTicketService.ReopenTicketAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("messages")]
    [Authorize]
    public async Task<ActionResult<TicketMessageDto>> AddMessage([FromBody] CreateTicketMessageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var isStaffResponse = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var message = await _supportTicketService.AddMessageAsync(userId, dto, isStaffResponse);

        return CreatedAtAction(nameof(GetTicketMessages), new { id = dto.TicketId }, message);
    }

    [HttpGet("{id}/messages")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TicketMessageDto>>> GetTicketMessages(Guid id)
    {
        var isStaff = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        var messages = await _supportTicketService.GetTicketMessagesAsync(id, isStaff);

        return Ok(messages);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<TicketStatsDto>> GetTicketStats()
    {
        var stats = await _supportTicketService.GetTicketStatsAsync();
        return Ok(stats);
    }

    [HttpGet("unassigned")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetUnassignedTickets()
    {
        var tickets = await _supportTicketService.GetUnassignedTicketsAsync();
        return Ok(tickets);
    }

    [HttpGet("my-assigned")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<IEnumerable<SupportTicketDto>>> GetMyAssignedTickets()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var tickets = await _supportTicketService.GetMyAssignedTicketsAsync(userId);
        return Ok(tickets);
    }

    [HttpGet("agent/dashboard")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<SupportAgentDashboardDto>> GetAgentDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        if (!TryGetUserId(out var agentId))
        {
            return Unauthorized();
        }

        var dashboard = await _supportTicketService.GetAgentDashboardAsync(agentId, startDate, endDate);
        return Ok(dashboard);
    }

    [HttpGet("agent/categories")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<List<CategoryTicketCountDto>>> GetTicketsByCategory(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var categories = await _supportTicketService.GetTicketsByCategoryAsync(agentId, startDate, endDate);
        return Ok(categories);
    }

    [HttpGet("agent/priorities")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<List<PriorityTicketCountDto>>> GetTicketsByPriority(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var priorities = await _supportTicketService.GetTicketsByPriorityAsync(agentId, startDate, endDate);
        return Ok(priorities);
    }

    [HttpGet("agent/trends")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<List<TicketTrendDto>>> GetTicketTrends(
        [FromQuery] Guid? agentId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var trends = await _supportTicketService.GetTicketTrendsAsync(agentId, startDate, endDate);
        return Ok(trends);
    }
}
