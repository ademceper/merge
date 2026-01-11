using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.Review;

/// <summary>
/// Trust Badges Controller - Trust badge management endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews/trust-badges")]
public class TrustBadgesController : BaseController
{
    private readonly IMediator _mediator;

    public TrustBadgesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new trust badge (Admin only)
    /// </summary>
    /// <param name="dto">Trust badge creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created trust badge</returns>
    /// <response code="201">Trust badge created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="429">Too many requests</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (Spam koruması)
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
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new CreateTrustBadgeCommand(
            dto.Name,
            dto.Description,
            dto.IconUrl,
            dto.BadgeType,
            dto.Criteria,
            dto.IsActive,
            dto.DisplayOrder,
            dto.Color);

        var badge = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetBadge), new { id = badge.Id }, badge);
    }

    /// <summary>
    /// Get trust badge by ID
    /// </summary>
    /// <param name="id">Trust badge ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trust badge</returns>
    /// <response code="200">Returns the trust badge</response>
    /// <response code="404">Trust badge not found</response>
    /// <response code="429">Too many requests</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(TrustBadgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TrustBadgeDto>> GetBadge(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var query = new GetTrustBadgeByIdQuery(id);
        var badge = await _mediator.Send(query, cancellationToken);
        if (badge == null)
        {
            return NotFound();
        }
        return Ok(badge);
    }

    /// <summary>
    /// Get all trust badges
    /// </summary>
    /// <param name="badgeType">Filter by badge type (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trust badges</returns>
    /// <response code="200">Returns list of trust badges</response>
    /// <response code="429">Too many requests</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<TrustBadgeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<TrustBadgeDto>>> GetBadges(
        [FromQuery] string? badgeType = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var query = new GetTrustBadgesQuery(badgeType);
        var badges = await _mediator.Send(query, cancellationToken);
        return Ok(badges);
    }

    /// <summary>
    /// Update a trust badge (Admin only)
    /// </summary>
    /// <param name="id">Trust badge ID</param>
    /// <param name="dto">Trust badge update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated trust badge</returns>
    /// <response code="200">Trust badge updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Trust badge not found</response>
    /// <response code="429">Too many requests</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        // ✅ ARCHITECTURE: Exception handling - GlobalExceptionHandlerMiddleware otomatik olarak NotFoundException'ı yakalar ve 404 döner
        // Controller'da try-catch gereksiz, exception'ı GlobalExceptionHandlerMiddleware'e bırak
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

        var badge = await _mediator.Send(command, cancellationToken);
        return Ok(badge);
    }

    /// <summary>
    /// Delete a trust badge (Admin only)
    /// </summary>
    /// <param name="id">Trust badge ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Trust badge deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Trust badge not found</response>
    /// <response code="429">Too many requests</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteBadge(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new DeleteTrustBadgeCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Award a trust badge to a seller (Admin only)
    /// </summary>
    /// <param name="sellerId">Seller ID</param>
    /// <param name="dto">Badge award data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created seller trust badge</returns>
    /// <response code="201">Badge awarded successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("seller/{sellerId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new AwardSellerBadgeCommand(sellerId, dto.BadgeId, dto.ExpiresAt, dto.AwardReason);
        var badge = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSellerBadges), new { sellerId = sellerId }, badge);
    }

    /// <summary>
    /// Get seller trust badges
    /// </summary>
    /// <param name="sellerId">Seller ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of seller trust badges</returns>
    /// <response code="200">Returns list of seller trust badges</response>
    /// <response code="429">Too many requests</response>
    [HttpGet("seller/{sellerId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<SellerTrustBadgeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<SellerTrustBadgeDto>>> GetSellerBadges(
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var query = new GetSellerBadgesQuery(sellerId);
        var badges = await _mediator.Send(query, cancellationToken);
        return Ok(badges);
    }

    /// <summary>
    /// Revoke a seller trust badge (Admin only)
    /// </summary>
    /// <param name="sellerId">Seller ID</param>
    /// <param name="badgeId">Badge ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Badge revoked successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Badge not found</response>
    /// <response code="429">Too many requests</response>
    [HttpDelete("seller/{sellerId}/{badgeId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new RevokeSellerBadgeCommand(sellerId, badgeId);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Award a trust badge to a product (Admin only)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="dto">Badge award data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product trust badge</returns>
    /// <response code="201">Badge awarded successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("product/{productId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new AwardProductBadgeCommand(productId, dto.BadgeId, dto.ExpiresAt, dto.AwardReason);
        var badge = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProductBadges), new { productId = productId }, badge);
    }

    /// <summary>
    /// Get product trust badges
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of product trust badges</returns>
    /// <response code="200">Returns list of product trust badges</response>
    /// <response code="429">Too many requests</response>
    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductTrustBadgeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductTrustBadgeDto>>> GetProductBadges(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var query = new GetProductBadgesQuery(productId);
        var badges = await _mediator.Send(query, cancellationToken);
        return Ok(badges);
    }

    /// <summary>
    /// Revoke a product trust badge (Admin only)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="badgeId">Badge ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Badge revoked successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Badge not found</response>
    /// <response code="429">Too many requests</response>
    [HttpDelete("product/{productId}/{badgeId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new RevokeProductBadgeCommand(productId, badgeId);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Evaluate and award badges automatically (Admin only)
    /// </summary>
    /// <param name="sellerId">Optional seller ID to evaluate (if not provided, evaluates all sellers)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Evaluation completed successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("evaluate")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika (Expensive operation)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EvaluateBadges(
        [FromQuery] Guid? sellerId = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new EvaluateAndAwardBadgesCommand(sellerId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Evaluate and award badges for a specific seller (Admin only)
    /// </summary>
    /// <param name="sellerId">Seller ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Evaluation completed successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("evaluate/seller/{sellerId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika (Expensive operation)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EvaluateSellerBadges(
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new EvaluateSellerBadgesCommand(sellerId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Evaluate and award badges for a specific product (Admin only)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Evaluation completed successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("evaluate/product/{productId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika (Expensive operation)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EvaluateProductBadges(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern
        var command = new EvaluateProductBadgesCommand(productId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

