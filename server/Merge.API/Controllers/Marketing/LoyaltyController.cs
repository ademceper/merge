using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetLoyaltyAccount;
using Merge.Application.Marketing.Queries.GetLoyaltyTransactions;
using Merge.Application.Marketing.Queries.GetLoyaltyTiers;
using Merge.Application.Marketing.Queries.GetLoyaltyStats;
using Merge.Application.Marketing.Queries.CalculatePointsFromPurchase;
using Merge.Application.Marketing.Queries.CalculateDiscountFromPoints;
using Merge.Application.Marketing.Commands.CreateLoyaltyAccount;
using Merge.Application.Marketing.Commands.RedeemPoints;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

/// <summary>
/// Loyalty API endpoints.
/// Sadakat programı işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/loyalty")]
[Authorize]
[Tags("Loyalty")]
public class LoyaltyController(
    IMediator mediator,
    IOptions<MarketingSettings> marketingSettings) : BaseController
{
    private readonly MarketingSettings _marketingSettings = marketingSettings.Value;

    [HttpGet("account")]
    [RateLimit(60, 60)]
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

        var query = new GetLoyaltyAccountQuery(userId);
        var account = await mediator.Send(query, cancellationToken);

        if (account == null)
        {
            var createCommand = new CreateLoyaltyAccountCommand(userId);
            account = await mediator.Send(createCommand, cancellationToken);
        }

        return Ok(account);
    }

    [HttpGet("transactions")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<LoyaltyTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<LoyaltyTransactionDto>>> GetTransactions(
        [FromQuery] int days = 30,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        if (days > _marketingSettings.MaxTransactionDays) days = _marketingSettings.MaxTransactionDays;

        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        var query = new GetLoyaltyTransactionsQuery(userId, days, PageNumber: page, PageSize: pageSize);
        var transactions = await mediator.Send(query, cancellationToken);
        return Ok(transactions);
    }

    [HttpPost("redeem")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RedeemPoints(
        [FromBody] RedeemPointsDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new RedeemPointsCommand(userId, dto.Points, dto.OrderId);
        var success = await mediator.Send(command, cancellationToken);
        
        if (!success)
        {
            return BadRequest("Puan kullanılamadı.");
        }

        return NoContent();
    }

    [HttpGet("tiers")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<LoyaltyTierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LoyaltyTierDto>>> GetTiers(
        CancellationToken cancellationToken = default)
    {
        var query = new GetLoyaltyTiersQuery();
        var tiers = await mediator.Send(query, cancellationToken);
        return Ok(tiers);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(LoyaltyStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoyaltyStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        var query = new GetLoyaltyStatsQuery();
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Satın alma tutarından kazanılacak puanları hesaplar
    /// </summary>
    /// <param name="amount">Satın alma tutarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kazanılacak puan miktarı</returns>
    /// <response code="200">Puan başarıyla hesaplandı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("calculate-points")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> CalculatePoints(
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        var query = new CalculatePointsFromPurchaseQuery(amount);
        var points = await mediator.Send(query, cancellationToken);
        return Ok(points);
    }

    /// <summary>
    /// Puanlardan hesaplanacak indirim tutarını hesaplar
    /// </summary>
    /// <param name="points">Kullanılacak puan miktarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İndirim tutarı</returns>
    /// <response code="200">İndirim başarıyla hesaplandı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("calculate-discount")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateDiscount(
        [FromQuery] int points,
        CancellationToken cancellationToken = default)
    {
        var query = new CalculateDiscountFromPointsQuery(points);
        var discount = await mediator.Send(query, cancellationToken);
        return Ok(discount);
    }
}
