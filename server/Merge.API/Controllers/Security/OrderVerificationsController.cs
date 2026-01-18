using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Security.Commands.CreateOrderVerification;
using Merge.Application.Security.Commands.VerifyOrder;
using Merge.Application.Security.Commands.RejectOrder;
using Merge.Application.Security.Queries.GetVerificationByOrderId;
using Merge.Application.Security.Queries.GetPendingVerifications;
using Merge.Application.Security.Queries.GetAllVerifications;

namespace Merge.API.Controllers.Security;

/// <summary>
/// Order Verification API endpoints.
/// Sipariş doğrulama işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/security/order-verifications")]
[Authorize]
[Tags("OrderVerifications")]
public class OrderVerificationsController(IMediator mediator, IOptions<PaginationSettings> paginationSettings) : BaseController
{
    private readonly PaginationSettings _paginationSettings = paginationSettings.Value;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(OrderVerificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderVerificationDto>> CreateVerification(
        [FromBody] CreateOrderVerificationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateOrderVerificationCommand(
            dto.OrderId,
            dto.VerificationType,
            dto.VerificationMethod,
            dto.VerificationNotes,
            dto.RequiresManualReview);
        var verification = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVerificationByOrderId), new { orderId = verification.OrderId }, verification);
    }

    [HttpGet("order/{orderId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(OrderVerificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderVerificationDto>> GetVerificationByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVerificationByOrderIdQuery(orderId);
        var verification = await mediator.Send(query, cancellationToken);
        if (verification == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(verification);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<OrderVerificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<OrderVerificationDto>>> GetPendingVerifications(
        CancellationToken cancellationToken = default)
    {
        var query = new GetPendingVerificationsQuery();
        var verifications = await mediator.Send(query, cancellationToken);
        return Ok(verifications);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<OrderVerificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<OrderVerificationDto>>> GetAllVerifications(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetAllVerificationsQuery(status, page, pageSize);
        var verifications = await mediator.Send(query, cancellationToken);
        return Ok(verifications);
    }

    [HttpPost("{verificationId}/verify")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyOrder(
        Guid verificationId,
        [FromBody] VerifyOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var verifiedByUserId = GetUserId();
        var command = new VerifyOrderCommand(verificationId, verifiedByUserId, dto.Notes);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("{verificationId}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RejectOrder(
        Guid verificationId,
        [FromBody] RejectOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var verifiedByUserId = GetUserId();
        var command = new RejectOrderCommand(verificationId, verifiedByUserId, dto.Reason);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }
}
