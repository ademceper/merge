using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Payment;
using Merge.Application.Payment.Commands.CreatePayment;
using Merge.Application.Payment.Commands.ProcessPayment;
using Merge.Application.Payment.Commands.RefundPayment;
using Merge.Application.Payment.Queries.GetPaymentById;
using Merge.Application.Payment.Queries.GetPaymentByOrderId;
using Merge.Application.Payment.Queries.VerifyPayment;
using Merge.Application.Order.Queries.GetOrderById;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Payment;

/// <summary>
/// Payment API endpoints.
/// Ödeme işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/payments")]
[Authorize]
[Tags("Payments")]
public class PaymentsController(IMediator mediator) : BaseController
{
    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentDto>> GetByOrderId(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var query = new GetPaymentByOrderIdQuery(orderId);
        var payment = await mediator.Send(query, cancellationToken);
        if (payment == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(payment);
    }

    [HttpPost]
    [RateLimit(10, 3600)]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var orderQuery = new GetOrderByIdQuery(dto.OrderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var command = new CreatePaymentCommand(
            dto.OrderId,
            dto.PaymentMethod,
            dto.PaymentProvider,
            dto.Amount);
        var payment = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByOrderId), new { orderId = payment.OrderId }, payment);
    }

    [HttpPost("{paymentId}/process")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentDto>> ProcessPayment(Guid paymentId, [FromBody] ProcessPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var command = new ProcessPaymentCommand(
            paymentId,
            dto.TransactionId,
            dto.PaymentReference,
            dto.Metadata);
        var payment = await mediator.Send(command, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("{paymentId}/refund")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 3600)]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentDto>> RefundPayment(Guid paymentId, [FromBody] RefundPaymentDto? dto = null, CancellationToken cancellationToken = default)
    {
        var command = new RefundPaymentCommand(paymentId, dto?.Amount);
        var payment = await mediator.Send(command, cancellationToken);
        return Ok(payment);
    }

    /// <summary>
    /// Ödeme işlemini doğrular
    /// </summary>
    /// <param name="dto">Doğrulama parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Doğrulama sonucu</returns>
    /// <response code="200">Doğrulama başarılı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpPost("verify")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<bool>> VerifyPayment([FromBody] VerifyPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var query = new VerifyPaymentQuery(dto.TransactionId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
