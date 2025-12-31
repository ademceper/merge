using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;

namespace Merge.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/loyalty")]
[Authorize]
public class LoyaltyController : BaseController
{
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    [HttpGet("account")]
    public async Task<ActionResult<LoyaltyAccountDto>> GetAccount()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var account = await _loyaltyService.GetLoyaltyAccountAsync(userId);

        if (account == null)
        {
            account = await _loyaltyService.CreateLoyaltyAccountAsync(userId);
        }

        return Ok(account);
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<LoyaltyTransactionDto>>> GetTransactions([FromQuery] int days = 30)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var transactions = await _loyaltyService.GetTransactionsAsync(userId, days);
        return Ok(transactions);
    }

    [HttpPost("redeem")]
    public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var success = await _loyaltyService.RedeemPointsAsync(userId, dto.Points, dto.OrderId);
        if (!success)
        {
            return BadRequest("Puan kullanılamadı.");
        }

        return NoContent();
    }

    [HttpGet("tiers")]
    public async Task<ActionResult<IEnumerable<LoyaltyTierDto>>> GetTiers()
    {
        var tiers = await _loyaltyService.GetTiersAsync();
        return Ok(tiers);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<LoyaltyStatsDto>> GetStats()
    {
        var stats = await _loyaltyService.GetLoyaltyStatsAsync();
        return Ok(stats);
    }

    [HttpGet("calculate-points")]
    public async Task<ActionResult<int>> CalculatePoints([FromQuery] decimal amount)
    {
        var points = await _loyaltyService.CalculatePointsFromPurchaseAsync(amount);
        return Ok(new { amount, points });
    }

    [HttpGet("calculate-discount")]
    public async Task<ActionResult<decimal>> CalculateDiscount([FromQuery] int points)
    {
        var discount = await _loyaltyService.CalculateDiscountFromPointsAsync(points);
        return Ok(new { points, discount });
    }
}
