using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.DTOs.Seller;


namespace Merge.API.Controllers.Seller;

[ApiController]
[Route("api/seller/commissions")]
public class CommissionsController : BaseController
{
    private readonly ISellerCommissionService _commissionService;

    public CommissionsController(ISellerCommissionService commissionService)
    {
        _commissionService = commissionService;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<SellerCommissionDto>> GetCommission(Guid id)
    {
        var commission = await _commissionService.GetCommissionAsync(id);

        if (commission == null)
        {
            return NotFound();
        }

        return Ok(commission);
    }

    [HttpGet("seller/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    public async Task<ActionResult<IEnumerable<SellerCommissionDto>>> GetSellerCommissions(
        Guid sellerId,
        [FromQuery] string? status = null)
    {
        var isSeller = User.IsInRole("Seller");

        // Sellers can only view their own commissions
        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }

        var commissions = await _commissionService.GetSellerCommissionsAsync(sellerId, status);
        return Ok(commissions);
    }

    [HttpGet("my-commissions")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<IEnumerable<SellerCommissionDto>>> GetMyCommissions([FromQuery] string? status = null)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var commissions = await _commissionService.GetSellerCommissionsAsync(userId, status);
        return Ok(commissions);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<SellerCommissionDto>>> GetAllCommissions(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var commissions = await _commissionService.GetAllCommissionsAsync(status, page, pageSize);
        return Ok(commissions);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApproveCommission(Guid id)
    {
        var success = await _commissionService.ApproveCommissionAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CancelCommission(Guid id)
    {
        var success = await _commissionService.CancelCommissionAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    // Commission Tiers
    [HttpPost("tiers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CommissionTierDto>> CreateTier([FromBody] CreateCommissionTierDto dto)
    {
        var tier = await _commissionService.CreateTierAsync(dto);
        return Ok(tier);
    }

    [HttpGet("tiers")]
    public async Task<ActionResult<IEnumerable<CommissionTierDto>>> GetAllTiers()
    {
        var tiers = await _commissionService.GetAllTiersAsync();
        return Ok(tiers);
    }

    [HttpPut("tiers/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTier(Guid id, [FromBody] CreateCommissionTierDto dto)
    {
        var success = await _commissionService.UpdateTierAsync(id, dto);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpDelete("tiers/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTier(Guid id)
    {
        var success = await _commissionService.DeleteTierAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    // Seller Settings
    [HttpGet("settings/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    public async Task<ActionResult<SellerCommissionSettingsDto>> GetSellerSettings(Guid sellerId)
    {
        var isSeller = User.IsInRole("Seller");

        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }

        var settings = await _commissionService.GetSellerSettingsAsync(sellerId);

        if (settings == null)
        {
            return NotFound();
        }

        return Ok(settings);
    }

    [HttpPut("settings/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    public async Task<ActionResult<SellerCommissionSettingsDto>> UpdateSellerSettings(
        Guid sellerId,
        [FromBody] UpdateCommissionSettingsDto dto)
    {
        var isSeller = User.IsInRole("Seller");

        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }

        var settings = await _commissionService.UpdateSellerSettingsAsync(sellerId, dto);
        return Ok(settings);
    }

    // Payouts
    [HttpPost("payouts")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<CommissionPayoutDto>> RequestPayout([FromBody] RequestPayoutDto dto)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var payout = await _commissionService.RequestPayoutAsync(userId, dto);
        return Ok(payout);
    }

    [HttpGet("payouts/{id}")]
    [Authorize]
    public async Task<ActionResult<CommissionPayoutDto>> GetPayout(Guid id)
    {
        var payout = await _commissionService.GetPayoutAsync(id);

        if (payout == null)
        {
            return NotFound();
        }

        return Ok(payout);
    }

    [HttpGet("payouts/seller/{sellerId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    public async Task<ActionResult<IEnumerable<CommissionPayoutDto>>> GetSellerPayouts(Guid sellerId)
    {
        var isSeller = User.IsInRole("Seller");

        if (isSeller && TryGetUserId(out var userId) && userId != sellerId)
        {
            return Forbid();
        }

        var payouts = await _commissionService.GetSellerPayoutsAsync(sellerId);
        return Ok(payouts);
    }

    [HttpGet("my-payouts")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<IEnumerable<CommissionPayoutDto>>> GetMyPayouts()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var payouts = await _commissionService.GetSellerPayoutsAsync(userId);
        return Ok(payouts);
    }

    [HttpGet("payouts")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<CommissionPayoutDto>>> GetAllPayouts(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var payouts = await _commissionService.GetAllPayoutsAsync(status, page, pageSize);
        return Ok(payouts);
    }

    [HttpPost("payouts/{id}/process")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ProcessPayout(Guid id, [FromBody] ProcessPayoutDto dto)
    {
        var success = await _commissionService.ProcessPayoutAsync(id, dto.TransactionReference);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost("payouts/{id}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CompletePayout(Guid id)
    {
        var success = await _commissionService.CompletePayoutAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPost("payouts/{id}/fail")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> FailPayout(Guid id, [FromBody] FailPayoutDto dto)
    {
        var success = await _commissionService.FailPayoutAsync(id, dto.Reason);

        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }

    // Stats
    [HttpGet("stats")]
    [Authorize]
    public async Task<ActionResult<CommissionStatsDto>> GetCommissionStats([FromQuery] Guid? sellerId = null)
    {
        var isSeller = User.IsInRole("Seller");

        // Sellers can only view their own stats
        if (isSeller && TryGetUserId(out var userId))
        {
            sellerId = userId;
        }

        var stats = await _commissionService.GetCommissionStatsAsync(sellerId);
        return Ok(stats);
    }

    [HttpGet("available-payout")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<decimal>> GetAvailablePayoutAmount()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var amount = await _commissionService.GetAvailablePayoutAmountAsync(userId);
        return Ok(new { availableAmount = amount });
    }
}
