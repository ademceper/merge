using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Payment;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Payment;


namespace Merge.API.Controllers.Payment;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : BaseController
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;

    public PaymentsController(IPaymentService paymentService, IOrderService orderService)
    {
        _paymentService = paymentService;
        _orderService = orderService;
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<PaymentDto>> GetByOrderId(Guid orderId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Önce Order'ın kullanıcıya ait olduğunu kontrol et
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var payment = await _paymentService.GetByOrderIdAsync(orderId);
        if (payment == null)
        {
            return NotFound();
        }
        
        return Ok(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var payment = await _paymentService.CreatePaymentAsync(dto);
        return CreatedAtAction(nameof(GetByOrderId), new { orderId = payment.OrderId }, payment);
    }

    [HttpPost("{paymentId}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaymentDto>> ProcessPayment(Guid paymentId, [FromBody] ProcessPaymentDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var payment = await _paymentService.ProcessPaymentAsync(paymentId, dto);
        if (payment == null)
        {
            return NotFound();
        }
        return Ok(payment);
    }

    [HttpPost("{paymentId}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaymentDto>> RefundPayment(Guid paymentId, [FromBody] RefundPaymentDto? dto = null)
    {
        var payment = await _paymentService.RefundPaymentAsync(paymentId, dto?.Amount);
        return Ok(payment);
    }

    [HttpPost("verify")]
    public async Task<ActionResult<bool>> VerifyPayment([FromBody] VerifyPaymentDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _paymentService.VerifyPaymentAsync(dto.TransactionId);
        return Ok(new { isValid = result });
    }
}

