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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var session = await _liveChatService.GetSessionByIdAsync(id);
        if (session == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi session'larına erişebilmeli veya agent olmalı
        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        return Ok(session);
    }

    [HttpGet("sessions/session-id/{sessionId}")]
    [Authorize]
    public async Task<ActionResult<LiveChatSessionDto>> GetSessionBySessionId(string sessionId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var session = await _liveChatService.GetSessionBySessionIdAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi session'larına erişebilmeli veya agent olmalı
        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi session'larını kapatabilmeli veya agent olmalı
        var session = await _liveChatService.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

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

        if (!TryGetUserId(out var senderId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi session'larına mesaj gönderebilmeli veya agent olmalı
        var session = await _liveChatService.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != senderId)
        {
            return Forbid();
        }

        var message = await _liveChatService.SendMessageAsync(sessionId, senderId, dto);
        return CreatedAtAction(nameof(GetSessionMessages), new { sessionId = sessionId }, message);
    }

    [HttpGet("sessions/{sessionId}/messages")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LiveChatMessageDto>>> GetSessionMessages(Guid sessionId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi session'larının mesajlarını görebilmeli veya agent olmalı
        var session = await _liveChatService.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var messages = await _liveChatService.GetSessionMessagesAsync(sessionId);
        return Ok(messages);
    }

    [HttpPost("sessions/{sessionId}/messages/mark-read")]
    [Authorize]
    public async Task<IActionResult> MarkMessagesAsRead(Guid sessionId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi session'larının mesajlarını okuyabilir olarak işaretleyebilmeli veya agent olmalı
        var session = await _liveChatService.GetSessionByIdAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

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

