using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.Support.Commands.CreateLiveChatSession;
using Merge.Application.Support.Commands.AssignAgentToSession;
using Merge.Application.Support.Commands.CloseLiveChatSession;
using Merge.Application.Support.Commands.SendLiveChatMessage;
using Merge.Application.Support.Commands.MarkMessagesAsRead;
using Merge.Application.Support.Queries.GetLiveChatSession;
using Merge.Application.Support.Queries.GetLiveChatSessionBySessionId;
using Merge.Application.Support.Queries.GetUserLiveChatSessions;
using Merge.Application.Support.Queries.GetAgentLiveChatSessions;
using Merge.Application.Support.Queries.GetWaitingLiveChatSessions;
using Merge.Application.Support.Queries.GetLiveChatSessionMessages;
using Merge.Application.Support.Queries.GetLiveChatStats;
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
/// Live Chat Controller - Manages live chat sessions and messages
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/live-chat")]
public class LiveChatController : BaseController
{
    private readonly IMediator _mediator;

    public LiveChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new live chat session
    /// </summary>
    /// <param name="dto">Session creation data (optional - can be null for anonymous users)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created session</returns>
    /// <response code="201">Session created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="429">Rate limit exceeded</response>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CreateLiveChatSessionCommand(
            userId,
            dto?.GuestName,
            dto?.GuestEmail,
            dto?.Department);
        var session = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
        session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return CreatedAtAction(nameof(GetSessionById), new { version, id = session.Id }, session);
    }

    /// <summary>
    /// Gets a live chat session by ID
    /// </summary>
    /// <param name="id">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The session</returns>
    /// <response code="200">Session found</response>
    /// <response code="404">Session not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to access this session</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("sessions/{id}", Name = "GetSessionById")]
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetLiveChatSessionQuery(id);
        var session = await _mediator.Send(query, cancellationToken);
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

        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
        session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

        return Ok(session);
    }

    /// <summary>
    /// Gets a live chat session by session ID (string)
    /// </summary>
    /// <param name="sessionId">Session ID (string format)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The session</returns>
    /// <response code="200">Session found</response>
    /// <response code="404">Session not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to access this session</response>
    /// <response code="429">Rate limit exceeded</response>
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetLiveChatSessionBySessionIdQuery(sessionId);
        var session = await _mediator.Send(query, cancellationToken);
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

        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
        session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

        return Ok(session);
    }

    /// <summary>
    /// Gets all live chat sessions for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's sessions</returns>
    /// <response code="200">Sessions retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="429">Rate limit exceeded</response>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetUserLiveChatSessionsQuery(userId);
        var sessions = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each session
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        foreach (var session in sessions)
        {
            var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
            session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
        
        return Ok(sessions);
    }

    /// <summary>
    /// Gets all live chat sessions assigned to the authenticated agent (Admin/Manager/Support only)
    /// </summary>
    /// <param name="status">Filter by status (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of agent's sessions</returns>
    /// <response code="200">Sessions retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetAgentLiveChatSessionsQuery(agentId, status);
        var sessions = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each session
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        foreach (var session in sessions)
        {
            var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
            session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
        
        return Ok(sessions);
    }

    /// <summary>
    /// Gets all waiting live chat sessions (Admin/Manager/Support only)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of waiting sessions</returns>
    /// <response code="200">Waiting sessions retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetWaitingLiveChatSessionsQuery();
        var sessions = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each session
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        foreach (var session in sessions)
        {
            var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
            session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
        
        return Ok(sessions);
    }

    /// <summary>
    /// Assigns an agent to a live chat session (Admin/Manager/Support only)
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="dto">Assignment data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Agent assigned successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Session not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions/{sessionId}/assign-agent", Name = "AssignAgent")]
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new AssignAgentToSessionCommand(sessionId, dto.AgentId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Closes a live chat session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Session closed successfully</response>
    /// <response code="404">Session not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to close this session</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions/{sessionId}/close", Name = "CloseSession")]
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var sessionQuery = new GetLiveChatSessionQuery(sessionId);
        var session = await _mediator.Send(sessionQuery, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var command = new CloseLiveChatSessionCommand(sessionId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sends a message in a live chat session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="dto">Message data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created message</returns>
    /// <response code="201">Message sent successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to send message in this session</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions/{sessionId}/messages", Name = "SendMessage")]
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var sessionQuery = new GetLiveChatSessionQuery(sessionId);
        var session = await _mediator.Send(sessionQuery, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != senderId)
        {
            return Forbid();
        }

        var command = new SendLiveChatMessageCommand(
            sessionId,
            senderId,
            dto.Content,
            dto.MessageType,
            dto.FileUrl,
            dto.FileName,
            dto.IsInternal);
        var message = await _mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetSessionMessages), new { version, sessionId = sessionId }, message);
    }

    /// <summary>
    /// Gets all messages for a live chat session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of messages</returns>
    /// <response code="200">Messages retrieved successfully</response>
    /// <response code="404">Session not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to access this session's messages</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("sessions/{sessionId}/messages", Name = "GetSessionMessages")]
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var sessionQuery = new GetLiveChatSessionQuery(sessionId);
        var session = await _mediator.Send(sessionQuery, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var query = new GetLiveChatSessionMessagesQuery(sessionId);
        var messages = await _mediator.Send(query, cancellationToken);
        return Ok(messages);
    }

    /// <summary>
    /// Marks all messages in a live chat session as read
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Messages marked as read successfully</response>
    /// <response code="404">Session not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized to mark messages in this session</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("sessions/{sessionId}/messages/mark-read", Name = "MarkMessagesAsRead")]
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var sessionQuery = new GetLiveChatSessionQuery(sessionId);
        var session = await _mediator.Send(sessionQuery, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var command = new MarkMessagesAsReadCommand(sessionId, userId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Gets live chat statistics (Admin/Manager/Support only)
    /// </summary>
    /// <param name="startDate">Start date for statistics (optional)</param>
    /// <param name="endDate">End date for statistics (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Live chat statistics</returns>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetLiveChatStatsQuery(startDate, endDate);
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}

