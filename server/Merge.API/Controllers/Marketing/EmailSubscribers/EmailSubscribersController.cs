using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Commands.SubscribeEmail;
using Merge.Application.Marketing.Commands.UnsubscribeEmail;
using Merge.Application.Marketing.Queries.GetEmailSubscriberById;
using Merge.Application.Marketing.Queries.GetEmailSubscriberByEmail;
using Merge.Application.Marketing.Queries.GetAllEmailSubscribers;
using Merge.Application.Marketing.Commands.UpdateEmailSubscriber;
using Merge.Application.Marketing.Commands.BulkImportEmailSubscribers;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Application.Exceptions;

namespace Merge.API.Controllers.Marketing.EmailSubscribers;

/// <summary>
/// Email Subscribers API endpoints.
/// E-posta abonelerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/email-subscribers")]
[Authorize]
[Tags("EmailSubscribers")]
public class EmailSubscribersController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpPost]
    [AllowAnonymous]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> Subscribe(
        [FromBody] CreateEmailSubscriberDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new SubscribeEmailCommand(
            dto.Email,
            dto.FirstName,
            dto.LastName,
            dto.Source,
            dto.Tags,
            dto.CustomFields);

        var subscriber = await mediator.Send(command, cancellationToken);
        return Ok(subscriber);
    }

    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Unsubscribe(
        [FromBody] string email,
        CancellationToken cancellationToken = default)
    {
        var command = new UnsubscribeEmailCommand(email);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("EmailSubscriber", email);

        return NoContent();
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> GetSubscriber(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmailSubscriberByIdQuery(id);
        var subscriber = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("EmailSubscriber", id);

        return Ok(subscriber);
    }

    [HttpGet("by-email/{email}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> GetSubscriberByEmail(
        string email,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmailSubscriberByEmailQuery(email);
        var subscriber = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("EmailSubscriber", email);

        return Ok(subscriber);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<EmailSubscriberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<EmailSubscriberDto>>> GetSubscribers(
        [FromQuery] bool? isSubscribed = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetAllEmailSubscribersQuery(isSubscribed, PageNumber: page, PageSize: pageSize);
        var subscribers = await mediator.Send(query, cancellationToken);
        return Ok(subscribers);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> UpdateSubscriber(
        Guid id,
        [FromBody] UpdateEmailSubscriberDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmailSubscriberCommand(
            id,
            dto.FirstName,
            dto.LastName,
            dto.Source,
            dto.Tags,
            dto.CustomFields,
            dto.IsSubscribed);

        var subscriber = await mediator.Send(command, cancellationToken);
        return Ok(subscriber);
    }

    /// <summary>
    /// Email abonesini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(EmailSubscriberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<EmailSubscriberDto>> PatchSubscriber(
        Guid id,
        [FromBody] PatchEmailSubscriberDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmailSubscriberCommand(
            id,
            patchDto.FirstName,
            patchDto.LastName,
            patchDto.Source,
            patchDto.Tags,
            patchDto.CustomFields,
            patchDto.IsSubscribed);

        var subscriber = await mediator.Send(command, cancellationToken);
        return Ok(subscriber);
    }

    [HttpPost("bulk-import")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> BulkImportSubscribers(
        [FromBody] BulkImportSubscribersDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new BulkImportEmailSubscribersCommand(dto.Subscribers);
        var count = await mediator.Send(command, cancellationToken);
        return Ok(count);
    }
}
