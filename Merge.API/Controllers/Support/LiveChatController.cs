using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Support;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;


namespace Merge.API.Controllers.Support;

[ApiController]
[Route("api/support/live-chat")]
public class LiveChatController : BaseController
{
    private readonly ILiveChatService _liveChatService;

    public LiveChatController(ILiveChatService liveChatService)
    {
        _liveChatService = liveChatService;
    }

    [HttpPost("sessions")]
    [AllowAnonymous]
    public async Task<ActionResult<LiveChatSessionDto>> CreateSession([FromBody] CreateLiveChatSessionDto? dto = null)
    {
        if (dto != null)
        {
            var validationResult = ValidateModelState();
            if (validationResult != null) return validationResult;
        }

        var userId = GetUserIdOrNull();
        var session = await _liveChatService.CreateSessionAsync(
            userId,
            dto?.GuestName,
            dto?.GuestEmail,
            dto?.Department
        );
        return CreatedAtAction(nameof(GetSessionById), new { id = session.Id }, session);
    }

    [HttpGet("sessions/{id}")]
    [Authorize]
    public async Task<ActionResult<LiveChatSessionDto>> GetSessionById(Guid id)
    {
        var session = await _liveChatService.GetSessionByIdAsync(id);
        if (session == null)
        {
            return NotFound();
        }
        return Ok(session);
    }

    [HttpGet("sessions/session-id/{sessionId}")]
    [Authorize]
    public async Task<ActionResult<LiveChatSessionDto>> GetSessionBySessionId(string sessionId)
    {
        var session = await _liveChatService.GetSessionBySessionIdAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }
        return Ok(session);
    }

    [HttpGet("sessions/my-sessions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetMySessions()
    {
        var userId = GetUserId();
        var sessions = await _liveChatService.GetUserSessionsAsync(userId);
        return Ok(sessions);
    }

    [HttpGet("sessions/agent/my-sessions")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetAgentSessions([FromQuery] string? status = null)
    {
        var agentId = GetUserId();
        var sessions = await _liveChatService.GetAgentSessionsAsync(agentId, status);
        return Ok(sessions);
    }

    [HttpGet("sessions/waiting")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetWaitingSessions()
    {
        var sessions = await _liveChatService.GetWaitingSessionsAsync();
        return Ok(sessions);
    }

    [HttpPost("sessions/{sessionId}/assign-agent")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<IActionResult> AssignAgent(Guid sessionId, [FromBody] AssignAgentDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _liveChatService.AssignAgentAsync(sessionId, dto.AgentId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("sessions/{sessionId}/close")]
    [Authorize]
    public async Task<IActionResult> CloseSession(Guid sessionId)
    {
        var result = await _liveChatService.CloseSessionAsync(sessionId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("sessions/{sessionId}/messages")]
    [Authorize]
    public async Task<ActionResult<LiveChatMessageDto>> SendMessage(Guid sessionId, [FromBody] CreateLiveChatMessageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var senderId = GetUserId();
        var message = await _liveChatService.SendMessageAsync(sessionId, senderId, dto);
        return CreatedAtAction(nameof(GetSessionMessages), new { sessionId = sessionId }, message);
    }

    [HttpGet("sessions/{sessionId}/messages")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LiveChatMessageDto>>> GetSessionMessages(Guid sessionId)
    {
        var messages = await _liveChatService.GetSessionMessagesAsync(sessionId);
        return Ok(messages);
    }

    [HttpPost("sessions/{sessionId}/messages/mark-read")]
    [Authorize]
    public async Task<IActionResult> MarkMessagesAsRead(Guid sessionId)
    {
        var userId = GetUserId();
        var result = await _liveChatService.MarkMessagesAsReadAsync(sessionId, userId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<ActionResult<LiveChatStatsDto>> GetStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var stats = await _liveChatService.GetChatStatsAsync(startDate, endDate);
        return Ok(stats);
    }
}

