using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;
using Merge.API.Middleware;

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

    /// <summary>
    /// Kullanıcının sadakat hesabını getirir
    /// </summary>
    [HttpGet("account")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(LoyaltyAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoyaltyAccountDto>> GetAccount(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var account = await _loyaltyService.GetLoyaltyAccountAsync(userId, cancellationToken);

        if (account == null)
        {
            account = await _loyaltyService.CreateLoyaltyAccountAsync(userId, cancellationToken);
        }

        return Ok(account);
    }

    /// <summary>
    /// Kullanıcının sadakat işlemlerini getirir
    /// </summary>
    [HttpGet("transactions")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<LoyaltyTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LoyaltyTransactionDto>>> GetTransactions(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        if (days > 365) days = 365; // Max 1 yıl

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var transactions = await _loyaltyService.GetTransactionsAsync(userId, days, cancellationToken);
        return Ok(transactions);
    }

    /// <summary>
    /// Sadakat puanlarını kullanır
    /// </summary>
    [HttpPost("redeem")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika (kritik işlem)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RedeemPoints(
        [FromBody] RedeemPointsDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _loyaltyService.RedeemPointsAsync(userId, dto.Points, dto.OrderId, cancellationToken);
        if (!success)
        {
            return BadRequest("Puan kullanılamadı.");
        }

        return NoContent();
    }

    /// <summary>
    /// Sadakat seviyelerini getirir
    /// </summary>
    [HttpGet("tiers")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<LoyaltyTierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LoyaltyTierDto>>> GetTiers(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var tiers = await _loyaltyService.GetTiersAsync(cancellationToken);
        return Ok(tiers);
    }

    /// <summary>
    /// Sadakat istatistiklerini getirir (Admin, Manager)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(LoyaltyStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoyaltyStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _loyaltyService.GetLoyaltyStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Satın alma tutarından kazanılacak puanları hesaplar
    /// </summary>
    [HttpGet("calculate-points")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> CalculatePoints(
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var points = await _loyaltyService.CalculatePointsFromPurchaseAsync(amount, cancellationToken);
        return Ok(new { amount, points });
    }

    /// <summary>
    /// Puanlardan indirim miktarını hesaplar
    /// </summary>
    [HttpGet("calculate-discount")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateDiscount(
        [FromQuery] int points,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var discount = await _loyaltyService.CalculateDiscountFromPointsAsync(points, cancellationToken);
        return Ok(new { points, discount });
    }
}
