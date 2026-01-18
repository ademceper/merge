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
using Merge.Application.Exceptions;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Support;

/// <summary>
/// Live Chat API endpoints.
/// Canlı destek sohbet işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/live-chat")]
[Tags("LiveChat")]
public class LiveChatController(IMediator mediator) : BaseController
{

    [HttpPost("sessions")]
    [AllowAnonymous]
    [RateLimit(10, 60)]     [ProducesResponseType(typeof(LiveChatSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatSessionDto>> CreateSession(
        [FromBody] CreateLiveChatSessionDto? dto = null,
        CancellationToken cancellationToken = default)
    {
                if (dto is not null)
        {
            var validationResult = ValidateModelState();
            if (validationResult is not null) return validationResult;
        }

        var userId = GetUserIdOrNull();
                var command = new CreateLiveChatSessionCommand(
            userId,
            dto?.GuestName,
            dto?.GuestEmail,
            dto?.Department);
        var session = await mediator.Send(command, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
        session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return CreatedAtAction(nameof(GetSessionById), new { version, id = session.Id }, session);
    }

    [HttpGet("sessions/{id}", Name = "GetSessionById")]
    [Authorize]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(LiveChatSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatSessionDto>> GetSessionById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                var query = new GetLiveChatSessionQuery(id);
        var session = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("LiveChatSession", id);

                var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
        session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

        return Ok(session);
    }

    [HttpGet("sessions/session-id/{sessionId}")]
    [Authorize]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(LiveChatSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatSessionDto>> GetSessionBySessionId(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                var query = new GetLiveChatSessionBySessionIdQuery(sessionId);
        var session = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("LiveChatSession", sessionId);

                var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
        session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

        return Ok(session);
    }

    [HttpGet("sessions/my-sessions")]
    [Authorize]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<LiveChatSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetMySessions(
        CancellationToken cancellationToken = default)
    {
                var userId = GetUserId();
                var query = new GetUserLiveChatSessionsQuery(userId);
        var sessions = await mediator.Send(query, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        foreach (var session in sessions)
        {
            var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
            session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
        
        return Ok(sessions);
    }

    [HttpGet("sessions/agent/my-sessions")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<LiveChatSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetAgentSessions(
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
                var agentId = GetUserId();
                var query = new GetAgentLiveChatSessionsQuery(agentId, status);
        var sessions = await mediator.Send(query, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        foreach (var session in sessions)
        {
            var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
            session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
        
        return Ok(sessions);
    }

    [HttpGet("sessions/waiting")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<LiveChatSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LiveChatSessionDto>>> GetWaitingSessions(
        CancellationToken cancellationToken = default)
    {
                        var query = new GetWaitingLiveChatSessionsQuery();
        var sessions = await mediator.Send(query, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        foreach (var session in sessions)
        {
            var links = HateoasHelper.CreateLiveChatSessionLinks(Url, session.Id, version);
            session.Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }
        
        return Ok(sessions);
    }

    [HttpPost("sessions/{sessionId}/assign-agent", Name = "AssignAgent")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
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
                var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

                var command = new AssignAgentToSessionCommand(sessionId, dto.AgentId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("LiveChatSession", sessionId);

        return NoContent();
    }

    [HttpPost("sessions/{sessionId}/close", Name = "CloseSession")]
    [Authorize]
    [RateLimit(10, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CloseSession(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                        var sessionQuery = new GetLiveChatSessionQuery(sessionId);
        var session = await mediator.Send(sessionQuery, cancellationToken)
            ?? throw new NotFoundException("LiveChatSession", sessionId);

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var command = new CloseLiveChatSessionCommand(sessionId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("LiveChatSession", sessionId);

        return NoContent();
    }

    [HttpPost("sessions/{sessionId}/messages", Name = "SendMessage")]
    [Authorize]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(LiveChatMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatMessageDto>> SendMessage(
        Guid sessionId,
        [FromBody] CreateLiveChatMessageDto dto,
        CancellationToken cancellationToken = default)
    {
                var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        if (!TryGetUserId(out var senderId))
        {
            return Unauthorized();
        }

                        var sessionQuery = new GetLiveChatSessionQuery(sessionId);
        var session = await mediator.Send(sessionQuery, cancellationToken)
            ?? throw new NotFoundException("LiveChatSession", sessionId);

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
        var message = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        return CreatedAtAction(nameof(GetSessionMessages), new { version, sessionId = sessionId }, message);
    }

    [HttpGet("sessions/{sessionId}/messages", Name = "GetSessionMessages")]
    [Authorize]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<LiveChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LiveChatMessageDto>>> GetSessionMessages(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                        var sessionQuery = new GetLiveChatSessionQuery(sessionId);
        var session = await mediator.Send(sessionQuery, cancellationToken)
            ?? throw new NotFoundException("LiveChatSession", sessionId);

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var query = new GetLiveChatSessionMessagesQuery(sessionId);
        var messages = await mediator.Send(query, cancellationToken);
        return Ok(messages);
    }

    [HttpPost("sessions/{sessionId}/messages/mark-read", Name = "MarkMessagesAsRead")]
    [Authorize]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkMessagesAsRead(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
                if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

                        var sessionQuery = new GetLiveChatSessionQuery(sessionId);
        var session = await mediator.Send(sessionQuery, cancellationToken)
            ?? throw new NotFoundException("LiveChatSession", sessionId);

        var isAgent = User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Support");
        if (!isAgent && session.UserId != userId)
        {
            return Forbid();
        }

        var command = new MarkMessagesAsReadCommand(sessionId, userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("LiveChatSession", sessionId);

        return NoContent();
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager,Support")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(LiveChatStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LiveChatStatsDto>> GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetLiveChatStatsQuery(startDate, endDate);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
