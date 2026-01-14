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

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/loyalty")]
[Authorize]
public class LoyaltyController : BaseController
{
    private readonly IMediator _mediator;
    private readonly MarketingSettings _marketingSettings;

    public LoyaltyController(
        IMediator mediator,
        IOptions<MarketingSettings> marketingSettings)
    {
        _mediator = mediator;
        _marketingSettings = marketingSettings.Value;
    }

    /// <summary>
    /// Kullanıcının sadakat hesabını getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sadakat hesabı detayları</returns>
    /// <response code="200">Sadakat hesabı başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("account")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Kullanıcının sadakat hesabını getirir",
        Description = "Kullanıcının sadakat hesabını getirir. Hesap yoksa otomatik olarak oluşturulur.")]
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetLoyaltyAccountQuery(userId);
        var account = await _mediator.Send(query, cancellationToken);

        if (account == null)
        {
            // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
            var createCommand = new CreateLoyaltyAccountCommand(userId);
            account = await _mediator.Send(createCommand, cancellationToken);
        }

        return Ok(account);
    }

    /// <summary>
    /// Kullanıcının sadakat işlemlerini getirir (pagination ile)
    /// </summary>
    /// <param name="days">Kaç günlük işlemler getirilecek (varsayılan: 30, maksimum: 365)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sadakat işlemleri listesi</returns>
    /// <response code="200">Sadakat işlemleri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("transactions")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Kullanıcının sadakat işlemlerini getirir",
        Description = "Sayfalama ile kullanıcının sadakat işlemlerini getirir.")]
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

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (days > _marketingSettings.MaxTransactionDays) days = _marketingSettings.MaxTransactionDays;

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetLoyaltyTransactionsQuery(userId, days, PageNumber: page, PageSize: pageSize);
        var transactions = await _mediator.Send(query, cancellationToken);
        return Ok(transactions);
    }

    /// <summary>
    /// Sadakat puanlarını kullanır
    /// </summary>
    /// <param name="dto">Puan kullanma bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kullanma işlemi sonucu</returns>
    /// <response code="204">Puanlar başarıyla kullanıldı</response>
    /// <response code="400">Geçersiz istek veya yetersiz puan</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("redeem")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika (kritik işlem)
    [SwaggerOperation(
        Summary = "Sadakat puanlarını kullanır",
        Description = "Kullanıcının sadakat puanlarını kullanır (sipariş için indirim olarak).")]
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new RedeemPointsCommand(userId, dto.Points, dto.OrderId);
        var success = await _mediator.Send(command, cancellationToken);
        
        if (!success)
        {
            return BadRequest("Puan kullanılamadı.");
        }

        return NoContent();
    }

    /// <summary>
    /// Sadakat seviyelerini getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sadakat seviyeleri listesi</returns>
    /// <response code="200">Sadakat seviyeleri başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("tiers")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Sadakat seviyelerini getirir",
        Description = "Tüm aktif sadakat seviyelerini getirir. Herkese açık endpoint.")]
    [ProducesResponseType(typeof(IEnumerable<LoyaltyTierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LoyaltyTierDto>>> GetTiers(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetLoyaltyTiersQuery();
        var tiers = await _mediator.Send(query, cancellationToken);
        return Ok(tiers);
    }

    /// <summary>
    /// Sadakat istatistiklerini getirir (Admin, Manager)
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sadakat istatistikleri</returns>
    /// <response code="200">Sadakat istatistikleri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Sadakat istatistiklerini getirir",
        Description = "Genel sadakat istatistiklerini getirir. Sadece Admin ve Manager rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(typeof(LoyaltyStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoyaltyStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetLoyaltyStatsQuery();
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Satın alma tutarından kazanılacak puanları hesaplar
    /// </summary>
    /// <param name="amount">Sipariş tutarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kazanılacak puan miktarı</returns>
    /// <response code="200">Puan miktarı hesaplandı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("calculate-points")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Satın alma tutarından kazanılacak puanları hesaplar",
        Description = "Sipariş tutarına göre kazanılacak sadakat puanlarını hesaplar.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> CalculatePoints(
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new CalculatePointsFromPurchaseQuery(amount);
        var points = await _mediator.Send(query, cancellationToken);
        return Ok(new { amount, points });
    }

    /// <summary>
    /// Puanlardan indirim miktarını hesaplar
    /// </summary>
    /// <param name="points">Puan miktarı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İndirim miktarı</returns>
    /// <response code="200">İndirim miktarı hesaplandı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("calculate-discount")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Puanlardan indirim miktarını hesaplar",
        Description = "Puan miktarına göre indirim miktarını hesaplar.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateDiscount(
        [FromQuery] int points,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new CalculateDiscountFromPointsQuery(points);
        var discount = await _mediator.Send(query, cancellationToken);
        return Ok(new { points, discount });
    }
}
