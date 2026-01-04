using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Support;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.API.Middleware;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions")]
    [AllowAnonymous]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (spam koruması)
    [ProducesResponseType(typeof(LiveChatSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatSessionDto>> CreateSession(
        [FromBody] CreateLiveChatSessionDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
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
            dto?.Department,
            cancellationToken);
        return CreatedAtAction(nameof(GetSessionById), new { id = session.Id }, session);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("sessions/{id}")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(LiveChatSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatSessionDto>> GetSessionById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var session = await _liveChatService.GetSessionByIdAsync(id, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi session'larına erişebilmeli veya agent olmalı
        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        return Ok(session);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("sessions/session-id/{sessionId}")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(LiveChatSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatSessionDto>> GetSessionBySessionId(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var session = await _liveChatService.GetSessionBySessionIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi session'larına erişebilmeli veya agent olmalı
        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        return Ok(session);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("sessions/my-sessions")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<LiveChatSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetMySessions(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        var sessions = await _liveChatService.GetUserSessionsAsync(userId, cancellationToken);
        return Ok(sessions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("sessions/agent/my-sessions")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<LiveChatSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetAgentSessions(
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var agentId = GetUserId();
        var sessions = await _liveChatService.GetAgentSessionsAsync(agentId, status, cancellationToken);
        return Ok(sessions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("sessions/waiting")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<LiveChatSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetWaitingSessions(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var sessions = await _liveChatService.GetWaitingSessionsAsync(cancellationToken);
        return Ok(sessions);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions/{sessionId}/assign-agent")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AssignAgent(
        Guid sessionId,
        [FromBody] AssignAgentDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _liveChatService.AssignAgentAsync(sessionId, dto.AgentId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions/{sessionId}/close")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CloseSession(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi session'larını kapatabilmeli veya agent olmalı
        var session = await _liveChatService.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var result = await _liveChatService.CloseSessionAsync(sessionId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions/{sessionId}/messages")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (spam koruması)
    [ProducesResponseType(typeof(LiveChatMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatMessageDto>> SendMessage(
        Guid sessionId,
        [FromBody] CreateLiveChatMessageDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var senderId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi session'larına mesaj gönderebilmeli veya agent olmalı
        var session = await _liveChatService.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != senderId)
        {
            return Forbid();
        }

        var message = await _liveChatService.SendMessageAsync(sessionId, senderId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetSessionMessages), new { sessionId = sessionId }, message);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("sessions/{sessionId}/messages")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<LiveChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LiveChatMessageDto>>> GetSessionMessages(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi session'larının mesajlarını görebilmeli veya agent olmalı
        var session = await _liveChatService.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var messages = await _liveChatService.GetSessionMessagesAsync(sessionId, cancellationToken);
        return Ok(messages);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions/{sessionId}/messages/mark-read")]
    [Authorize]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkMessagesAsRead(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR koruması - Kullanıcı sadece kendi session'larının mesajlarını okuyabilir olarak işaretleyebilmeli veya agent olmalı
        var session = await _liveChatService.GetSessionByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var result = await _liveChatService.MarkMessagesAsReadAsync(sessionId, userId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(LiveChatStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatStatsDto>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _liveChatService.GetChatStatsAsync(startDate, endDate, cancellationToken);
        return Ok(stats);
    }
}

