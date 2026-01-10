using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetMyReferralCode;
using Merge.Application.Marketing.Queries.GetMyReferrals;
using Merge.Application.Marketing.Queries.GetReferralStats;
using Merge.Application.Marketing.Commands.ApplyReferralCode;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/referrals")]
[Authorize]
public class ReferralsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly MarketingSettings _marketingSettings;

    public ReferralsController(
        IMediator mediator,
        IOptions<MarketingSettings> marketingSettings)
    {
        _mediator = mediator;
        _marketingSettings = marketingSettings.Value;
    }

    /// <summary>
    /// Kullanıcının referans kodunu getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Referans kodu detayları</returns>
    /// <response code="200">Referans kodu başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("my-code")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Kullanıcının referans kodunu getirir",
        Description = "Kullanıcının referans kodunu getirir. Kod yoksa otomatik olarak oluşturulur.")]
    [ProducesResponseType(typeof(ReferralCodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReferralCodeDto>> GetMyReferralCode(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetMyReferralCodeQuery(userId);
        var code = await _mediator.Send(query, cancellationToken);
        return Ok(code);
    }

    /// <summary>
    /// Kullanıcının referanslarını getirir (pagination ile)
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Referans listesi</returns>
    /// <response code="200">Referanslar başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("my-referrals")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Kullanıcının referanslarını getirir",
        Description = "Sayfalama ile kullanıcının referanslarını getirir.")]
    [ProducesResponseType(typeof(PagedResult<ReferralDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ReferralDto>>> GetMyReferrals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetMyReferralsQuery(userId, PageNumber: page, PageSize: pageSize);
        var referrals = await _mediator.Send(query, cancellationToken);
        return Ok(referrals);
    }

    /// <summary>
    /// Referans kodunu uygular
    /// </summary>
    /// <param name="code">Referans kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Uygulama işlemi sonucu</returns>
    /// <response code="204">Referans kodu başarıyla uygulandı</response>
    /// <response code="400">Geçersiz kod veya kod zaten uygulanmış</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("apply")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (kritik işlem)
    [SwaggerOperation(
        Summary = "Referans kodunu uygular",
        Description = "Yeni kullanıcı kaydı sırasında referans kodunu uygular.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApplyReferralCode(
        [FromBody] string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest("Referans kodu boş olamaz.");
        }

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new ApplyReferralCodeCommand(userId, code);
        var success = await _mediator.Send(command, cancellationToken);
        
        return success ? NoContent() : BadRequest(new { message = "Geçersiz kod" });
    }

    /// <summary>
    /// Kullanıcının referans istatistiklerini getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Referans istatistikleri</returns>
    /// <response code="200">Referans istatistikleri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("stats")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Kullanıcının referans istatistiklerini getirir",
        Description = "Kullanıcının referans istatistiklerini getirir (toplam referans, tamamlanan referans, kazanılan puanlar vb.).")]
    [ProducesResponseType(typeof(ReferralStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ReferralStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetReferralStatsQuery(userId);
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
