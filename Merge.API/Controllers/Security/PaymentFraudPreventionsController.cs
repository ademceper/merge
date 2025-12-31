using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Security;
using Merge.Application.Interfaces.User;
using Merge.Application.DTOs.Security;

namespace Merge.API.Controllers.Security;

[ApiController]
[Route("api/security/payment-fraud-prevention")]
[Authorize]
public class PaymentFraudPreventionsController : BaseController
{
    private readonly IPaymentFraudPreventionService _paymentFraudPreventionService;
        public PaymentFraudPreventionsController(
        IPaymentFraudPreventionService paymentFraudPreventionService)
    {
        _paymentFraudPreventionService = paymentFraudPreventionService;
            }

    [HttpPost("check")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PaymentFraudPreventionDto>> CheckPayment([FromBody] CreatePaymentFraudCheckDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (string.IsNullOrEmpty(dto.IpAddress))
        {
            dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        if (string.IsNullOrEmpty(dto.UserAgent))
        {
            dto.UserAgent = Request.Headers["User-Agent"].ToString();
        }

        var check = await _paymentFraudPreventionService.CheckPaymentAsync(dto);
        return CreatedAtAction(nameof(GetCheckByPaymentId), new { paymentId = dto.PaymentId }, check);
    }

    [HttpGet("payment/{paymentId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PaymentFraudPreventionDto>> GetCheckByPaymentId(Guid paymentId)
    {
        var check = await _paymentFraudPreventionService.GetCheckByPaymentIdAsync(paymentId);
        if (check == null)
        {
            return NotFound();
        }
        return Ok(check);
    }

    [HttpGet("blocked")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<PaymentFraudPreventionDto>>> GetBlockedPayments()
    {
        var checks = await _paymentFraudPreventionService.GetBlockedPaymentsAsync();
        return Ok(checks);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<PaymentFraudPreventionDto>>> GetAllChecks(
        [FromQuery] string? status = null,
        [FromQuery] bool? isBlocked = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var checks = await _paymentFraudPreventionService.GetAllChecksAsync(status, isBlocked, page, pageSize);
        return Ok(checks);
    }

    [HttpPost("{checkId}/block")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> BlockPayment(Guid checkId, [FromBody] BlockPaymentDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _paymentFraudPreventionService.BlockPaymentAsync(checkId, dto.Reason);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{checkId}/unblock")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UnblockPayment(Guid checkId)
    {
        var result = await _paymentFraudPreventionService.UnblockPaymentAsync(checkId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

