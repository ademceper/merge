using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Seller;
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

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seller/onboarding")]
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
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var command = new SubmitSellerApplicationCommand(userId, applicationDto);
        var application = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetMyApplication", new { version }, version);
        links["application"] = new LinkDto { Href = $"/api/v{version}/seller/onboarding/{application.Id}", Method = "GET" };
        return CreatedAtAction(nameof(GetMyApplication), new { version }, new { application, _links = links });
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
        var application = await mediator.Send(query, cancellationToken);

        if (application == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetMyApplication", new { version }, version);
        links["application"] = new LinkDto { Href = $"/api/v{version}/seller/onboarding/{application.Id}", Method = "GET" };
        return Ok(new { application, _links = links });
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
        var application = await mediator.Send(query, cancellationToken);

        if (application == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetApplication", new { version, id }, version);
        links["review"] = new LinkDto { Href = $"/api/v{version}/seller/onboarding/{id}/review", Method = "POST" };
        links["approve"] = new LinkDto { Href = $"/api/v{version}/seller/onboarding/{id}/approve", Method = "POST" };
        links["reject"] = new LinkDto { Href = $"/api/v{version}/seller/onboarding/{id}/reject", Method = "POST" };
        return Ok(new { application, _links = links });
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetApplications", page, pageSize, result.TotalPages, new { version, status }, version);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
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
        if (validationResult != null) return validationResult;

        var reviewerId = GetUserId();
        var command = new ReviewSellerApplicationCommand(
            id,
            reviewDto.Status,
            reviewDto.RejectionReason,
            reviewDto.AdditionalNotes,
            reviewerId);
        var application = await mediator.Send(command, cancellationToken);

        if (application == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetApplication", new { version, id }, version);
        return Ok(new { application, _links = links });
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
        {
            return NotFound();
        }
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
        if (validationResult != null) return validationResult;

        var reviewerId = GetUserId();
        var command = new RejectSellerApplicationCommand(id, rejectDto.Reason, reviewerId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound();
        }
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStats", new { version }, version);
        return Ok(new { stats, _links = links });
    }
}
