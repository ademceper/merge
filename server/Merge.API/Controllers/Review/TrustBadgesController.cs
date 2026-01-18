using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.Exceptions;
using Merge.Application.Review.Commands.CreateTrustBadge;
using Merge.Application.Review.Commands.UpdateTrustBadge;
using Merge.Application.Review.Commands.DeleteTrustBadge;
using Merge.Application.Review.Commands.AwardSellerBadge;
using Merge.Application.Review.Commands.RevokeSellerBadge;
using Merge.Application.Review.Commands.AwardProductBadge;
using Merge.Application.Review.Commands.RevokeProductBadge;
using Merge.Application.Review.Commands.EvaluateAndAwardBadges;
using Merge.Application.Review.Commands.EvaluateSellerBadges;
using Merge.Application.Review.Commands.EvaluateProductBadges;
using Merge.Application.Review.Queries.GetTrustBadgeById;
using Merge.Application.Review.Queries.GetTrustBadges;
using Merge.Application.Review.Queries.GetSellerBadges;
using Merge.Application.Review.Queries.GetProductBadges;
using Merge.Application.DTOs.Review;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Review;

/// <summary>
/// Trust Badges API endpoints.
/// Güven rozetlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/reviews/trust-badges")]
[Tags("TrustBadges")]
public class TrustBadgesController(IMediator mediator) : BaseController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(TrustBadgeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TrustBadgeDto>> CreateBadge(
        [FromBody] CreateTrustBadgeDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var command = new CreateTrustBadgeCommand(
            dto.Name,
            dto.Description,
            dto.IconUrl,
            dto.BadgeType,
            dto.Criteria,
            dto.IsActive,
            dto.DisplayOrder,
            dto.Color);
        var badge = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetBadge), new { id = badge.Id }, badge);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(TrustBadgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TrustBadgeDto>> GetBadge(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetTrustBadgeByIdQuery(id);
        var badge = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("TrustBadge", id);
        return Ok(badge);
    }

    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<TrustBadgeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<TrustBadgeDto>>> GetBadges(
        [FromQuery] string? badgeType = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTrustBadgesQuery(badgeType);
        var badges = await mediator.Send(query, cancellationToken);
        return Ok(badges);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(TrustBadgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TrustBadgeDto>> UpdateBadge(
        Guid id,
        [FromBody] UpdateTrustBadgeDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var command = new UpdateTrustBadgeCommand(
            id,
            dto.Name,
            dto.Description,
            dto.IconUrl,
            dto.BadgeType,
            dto.Criteria,
            dto.IsActive,
            dto.DisplayOrder,
            dto.Color);
        var badge = await mediator.Send(command, cancellationToken);
        return Ok(badge);
    }

    /// <summary>
    /// Güven rozetini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(TrustBadgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TrustBadgeDto>> PatchBadge(
        Guid id,
        [FromBody] PatchTrustBadgeDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var command = new UpdateTrustBadgeCommand(
            id,
            patchDto.Name,
            patchDto.Description,
            patchDto.IconUrl,
            patchDto.BadgeType,
            patchDto.Criteria,
            patchDto.IsActive,
            patchDto.DisplayOrder,
            patchDto.Color);
        var badge = await mediator.Send(command, cancellationToken);
        return Ok(badge);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteBadge(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteTrustBadgeCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("TrustBadge", id);

        return NoContent();
    }

    [HttpPost("seller/{sellerId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(SellerTrustBadgeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SellerTrustBadgeDto>> AwardSellerBadge(
        Guid sellerId,
        [FromBody] AwardBadgeDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var command = new AwardSellerBadgeCommand(sellerId, dto.BadgeId, dto.ExpiresAt, dto.AwardReason);
        var badge = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSellerBadges), new { sellerId = sellerId }, badge);
    }

    [HttpGet("seller/{sellerId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<SellerTrustBadgeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SellerTrustBadgeDto>>> GetSellerBadges(
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSellerBadgesQuery(sellerId);
        var badges = await mediator.Send(query, cancellationToken);
        return Ok(badges);
    }

    [HttpDelete("seller/{sellerId}/{badgeId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RevokeSellerBadge(
        Guid sellerId,
        Guid badgeId,
        CancellationToken cancellationToken = default)
    {
        var command = new RevokeSellerBadgeCommand(sellerId, badgeId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("SellerTrustBadge", badgeId);

        return NoContent();
    }

    [HttpPost("product/{productId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ProductTrustBadgeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductTrustBadgeDto>> AwardProductBadge(
        Guid productId,
        [FromBody] AwardBadgeDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var command = new AwardProductBadgeCommand(productId, dto.BadgeId, dto.ExpiresAt, dto.AwardReason);
        var badge = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProductBadges), new { productId = productId }, badge);
    }

    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductTrustBadgeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductTrustBadgeDto>>> GetProductBadges(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductBadgesQuery(productId);
        var badges = await mediator.Send(query, cancellationToken);
        return Ok(badges);
    }

    [HttpDelete("product/{productId}/{badgeId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RevokeProductBadge(
        Guid productId,
        Guid badgeId,
        CancellationToken cancellationToken = default)
    {
        var command = new RevokeProductBadgeCommand(productId, badgeId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("ProductTrustBadge", badgeId);

        return NoContent();
    }

    [HttpPost("evaluate")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EvaluateBadges(
        [FromQuery] Guid? sellerId = null,
        CancellationToken cancellationToken = default)
    {
        var command = new EvaluateAndAwardBadgesCommand(sellerId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("evaluate/seller/{sellerId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EvaluateSellerBadges(
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        var command = new EvaluateSellerBadgesCommand(sellerId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("evaluate/product/{productId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EvaluateProductBadges(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var command = new EvaluateProductBadgesCommand(productId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
