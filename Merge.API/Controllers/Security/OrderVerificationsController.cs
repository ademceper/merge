using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Security;
using Merge.Application.DTOs.Security;

namespace Merge.API.Controllers.Security;

[ApiController]
[Route("api/security/order-verifications")]
[Authorize]
public class OrderVerificationsController : BaseController
{
    private readonly IOrderVerificationService _orderVerificationService;
        public OrderVerificationsController(
        IOrderVerificationService orderVerificationService)
    {
        _orderVerificationService = orderVerificationService;
            }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderVerificationDto>> CreateVerification([FromBody] CreateOrderVerificationDto dto)
    {
        var verification = await _orderVerificationService.CreateVerificationAsync(dto);
        return CreatedAtAction(nameof(GetVerificationByOrderId), new { orderId = verification.OrderId }, verification);
    }

    [HttpGet("order/{orderId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderVerificationDto>> GetVerificationByOrderId(Guid orderId)
    {
        var verification = await _orderVerificationService.GetVerificationByOrderIdAsync(orderId);
        if (verification == null)
        {
            return NotFound();
        }
        return Ok(verification);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<OrderVerificationDto>>> GetPendingVerifications()
    {
        var verifications = await _orderVerificationService.GetPendingVerificationsAsync();
        return Ok(verifications);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<OrderVerificationDto>>> GetAllVerifications(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var verifications = await _orderVerificationService.GetAllVerificationsAsync(status, page, pageSize);
        return Ok(verifications);
    }

    [HttpPost("{verificationId}/verify")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> VerifyOrder(Guid verificationId, [FromBody] VerifyOrderDto dto)
    {
        var verifiedByUserId = GetUserId();
        var result = await _orderVerificationService.VerifyOrderAsync(verificationId, verifiedByUserId, dto.Notes);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{verificationId}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RejectOrder(Guid verificationId, [FromBody] RejectOrderDto dto)
    {
        var verifiedByUserId = GetUserId();
        var result = await _orderVerificationService.RejectOrderAsync(verificationId, verifiedByUserId, dto.Reason);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

