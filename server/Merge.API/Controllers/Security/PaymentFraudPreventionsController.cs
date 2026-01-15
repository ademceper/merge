using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Security.Commands.CreatePaymentFraudCheck;
using Merge.Application.Security.Commands.BlockPayment;
using Merge.Application.Security.Commands.UnblockPayment;
using Merge.Application.Security.Queries.GetCheckByPaymentId;
using Merge.Application.Security.Queries.GetBlockedPayments;
using Merge.Application.Security.Queries.GetAllChecks;

namespace Merge.API.Controllers.Security;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/security/payment-fraud-prevention")]
[Authorize]
public class PaymentFraudPreventionsController(IMediator mediator, IOptions<PaginationSettings> paginationSettings) : BaseController
{
    private readonly PaginationSettings _paginationSettings = paginationSettings.Value;

    [HttpPost("check")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(PaymentFraudPreventionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentFraudPreventionDto>> CheckPayment(
        [FromBody] CreatePaymentFraudCheckDto dto,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = string.IsNullOrEmpty(dto.IpAddress)
            ? HttpContext.Connection.RemoteIpAddress?.ToString()
            : dto.IpAddress;
        var userAgent = string.IsNullOrEmpty(dto.UserAgent)
            ? Request.Headers["User-Agent"].ToString()
            : dto.UserAgent;

        var command = new CreatePaymentFraudCheckCommand(
            dto.PaymentId,
            dto.CheckType,
            dto.DeviceFingerprint,
            ipAddress,
            userAgent);
        var check = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCheckByPaymentId), new { paymentId = dto.PaymentId }, check);
    }

    [HttpGet("payment/{paymentId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PaymentFraudPreventionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentFraudPreventionDto>> GetCheckByPaymentId(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCheckByPaymentIdQuery(paymentId);
        var check = await mediator.Send(query, cancellationToken);
        if (check == null)
        {
            return NotFound();
        }
        return Ok(check);
    }

    [HttpGet("blocked")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<PaymentFraudPreventionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PaymentFraudPreventionDto>>> GetBlockedPayments(
        CancellationToken cancellationToken = default)
    {
        var query = new GetBlockedPaymentsQuery();
        var checks = await mediator.Send(query, cancellationToken);
        return Ok(checks);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<PaymentFraudPreventionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PaymentFraudPreventionDto>>> GetAllChecks(
        [FromQuery] string? status = null,
        [FromQuery] bool? isBlocked = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetAllChecksQuery(status, isBlocked, page, pageSize);
        var checks = await mediator.Send(query, cancellationToken);
        return Ok(checks);
    }

    [HttpPost("{checkId}/block")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> BlockPayment(
        Guid checkId,
        [FromBody] BlockPaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new BlockPaymentCommand(checkId, dto.Reason);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{checkId}/unblock")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UnblockPayment(
        Guid checkId,
        CancellationToken cancellationToken = default)
    {
        var command = new UnblockPaymentCommand(checkId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
