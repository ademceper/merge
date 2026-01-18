using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Seller;
using Merge.Application.Exceptions;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.Application.Common;
using Merge.Application.Seller.Commands.SubmitSellerApplication;
using Merge.Application.Seller.Queries.GetUserSellerApplication;
using Merge.Application.Seller.Queries.GetSellerApplication;
using Merge.Application.Seller.Queries.GetAllSellerApplications;
using Merge.Application.Seller.Commands.ReviewSellerApplication;
using Merge.Application.Seller.Commands.ApproveSellerApplication;
using Merge.Application.Seller.Commands.RejectSellerApplication;
using Merge.Application.Seller.Queries.GetSellerOnboardingStats;
using Merge.Domain.Enums;

namespace Merge.API.Controllers.Seller;

/// <summary>
/// Seller Onboarding API endpoints.
/// Satıcı başvuru sürecini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/seller/onboarding")]
[Tags("SellerOnboarding")]
public class OnboardingController(IMediator mediator) : BaseController
{

    [HttpPost("apply")]
    [Authorize]
    [RateLimit(5, 60)]
    [ProducesResponseType(typeof(SellerApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerApplicationDto>> SubmitApplication(
        [FromBody] CreateSellerApplicationDto applicationDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var userId = GetUserId();
        var command = new SubmitSellerApplicationCommand(userId, applicationDto);
        var application = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMyApplication), new { version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" }, application);
    }

    [HttpGet("my-application")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerApplicationDto>> GetMyApplication(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserSellerApplicationQuery(userId);
        var application = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("SellerApplication", userId);
        return Ok(application);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerApplicationDto>> GetApplication(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSellerApplicationQuery(id);
        var application = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("SellerApplication", id);
        return Ok(application);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<SellerApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SellerApplicationDto>>> GetApplications(
        [FromQuery] SellerApplicationStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllSellerApplicationsQuery(status, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/review")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SellerApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerApplicationDto>> ReviewApplication(
        Guid id,
        [FromBody] ReviewSellerApplicationDto reviewDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var reviewerId = GetUserId();
        var command = new ReviewSellerApplicationCommand(
            id,
            reviewDto.Status,
            reviewDto.RejectionReason,
            reviewDto.AdditionalNotes,
            reviewerId);
        var application = await mediator.Send(command, cancellationToken)
            ?? throw new NotFoundException("SellerApplication", id);
        return Ok(application);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApproveApplication(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var reviewerId = GetUserId();
        var command = new ApproveSellerApplicationCommand(id, reviewerId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
            throw new NotFoundException("SellerApplication", id);
        return NoContent();
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RejectApplication(
        Guid id,
        [FromBody] RejectApplicationDto rejectDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var reviewerId = GetUserId();
        var command = new RejectSellerApplicationCommand(id, rejectDto.Reason, reviewerId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
            throw new NotFoundException("SellerApplication", id);
        return NoContent();
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(SellerOnboardingStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerOnboardingStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        var query = new GetSellerOnboardingStatsQuery();
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
