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

[ApiController]
[Route("api/v{version:apiVersion}/payments")]
[Authorize]
public class PaymentsController : BaseController
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentDto>> GetByOrderId(Guid orderId, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Önce Order'ın kullanıcıya ait olduğunu kontrol et
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPaymentByOrderIdQuery(orderId);
        var payment = await _mediator.Send(query, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }
        
        return Ok(payment);
    }

    // ✅ SECURITY: Rate limiting - 10 ödeme oluşturma / saat (fraud koruması)
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişleri için ödeme oluşturabilmeli
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var orderQuery = new GetOrderByIdQuery(dto.OrderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new CreatePaymentCommand(
            dto.OrderId,
            dto.PaymentMethod,
            dto.PaymentProvider,
            dto.Amount);
        
        var payment = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByOrderId), new { orderId = payment.OrderId }, payment);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{paymentId}/process")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentDto>> ProcessPayment(Guid paymentId, [FromBody] ProcessPaymentDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new ProcessPaymentCommand(
            paymentId,
            dto.TransactionId,
            dto.PaymentReference,
            dto.Metadata);
        
        var payment = await _mediator.Send(command, cancellationToken);
        return Ok(payment);
    }

    // ✅ SECURITY: Rate limiting - 5 iade işlemi / saat
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new RefundPaymentCommand(paymentId, dto?.Amount);
        var payment = await _mediator.Send(command, cancellationToken);
        return Ok(payment);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("verify")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<bool>> VerifyPayment([FromBody] VerifyPaymentDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new VerifyPaymentQuery(dto.TransactionId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(new { isValid = result });
    }
}

