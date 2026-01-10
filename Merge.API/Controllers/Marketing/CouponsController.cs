using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetAllCoupons;
using Merge.Application.Marketing.Queries.GetCouponByCode;
using Merge.Application.Marketing.Queries.GetCouponById;
using Merge.Application.Marketing.Commands.CreateCoupon;
using Merge.Application.Marketing.Commands.UpdateCoupon;
using Merge.Application.Marketing.Commands.DeleteCoupon;
using Merge.Application.Marketing.Commands.ValidateCoupon;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/coupons")]
public class CouponsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly MarketingSettings _marketingSettings;

    public CouponsController(
        IMediator mediator,
        IOptions<MarketingSettings> marketingSettings)
    {
        _mediator = mediator;
        _marketingSettings = marketingSettings.Value;
    }

    /// <summary>
    /// Tüm kuponları getirir (pagination ile)
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kupon listesi</returns>
    /// <response code="200">Kuponlar başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Tüm kuponları getirir",
        Description = "Sayfalama ile tüm aktif kuponları getirir. Herkese açık endpoint.")]
    [ProducesResponseType(typeof(PagedResult<CouponDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CouponDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetAllCouponsQuery(PageNumber: page, PageSize: pageSize);
        var coupons = await _mediator.Send(query, cancellationToken);
        return Ok(coupons);
    }

    /// <summary>
    /// Kupon koduna göre kupon getirir
    /// </summary>
    /// <param name="code">Kupon kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kupon detayları</returns>
    /// <response code="200">Kupon başarıyla getirildi</response>
    /// <response code="404">Kupon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Kupon koduna göre kupon getirir",
        Description = "Kupon koduna göre kupon detaylarını getirir. Herkese açık endpoint.")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CouponDto>> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetCouponByCodeQuery(code);
        var coupon = await _mediator.Send(query, cancellationToken);
        
        if (coupon == null)
        {
            return NotFound();
        }
        
        return Ok(coupon);
    }

    /// <summary>
    /// Kupon ID'sine göre kupon getirir
    /// </summary>
    /// <param name="id">Kupon ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kupon detayları</returns>
    /// <response code="200">Kupon başarıyla getirildi</response>
    /// <response code="404">Kupon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Kupon ID'sine göre kupon getirir",
        Description = "Kupon ID'sine göre kupon detaylarını getirir. Herkese açık endpoint.")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CouponDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetCouponByIdQuery(id);
        var coupon = await _mediator.Send(query, cancellationToken);
        
        if (coupon == null)
        {
            return NotFound();
        }
        
        return Ok(coupon);
    }

    /// <summary>
    /// Kupon kodunu doğrular ve indirim miktarını hesaplar
    /// </summary>
    /// <param name="dto">Kupon doğrulama bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İndirim miktarı</returns>
    /// <response code="200">Kupon doğrulandı ve indirim miktarı hesaplandı</response>
    /// <response code="400">Geçersiz istek veya kupon geçersiz</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("validate")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [SwaggerOperation(
        Summary = "Kupon kodunu doğrular ve indirim miktarını hesaplar",
        Description = "Kupon kodunu doğrular, geçerliliğini kontrol eder ve indirim miktarını hesaplar.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> ValidateCoupon(
        [FromBody] ValidateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new ValidateCouponCommand(
            dto.Code,
            dto.OrderAmount,
            dto.UserId,
            dto.ProductIds);
        
        var discount = await _mediator.Send(command, cancellationToken);
        return Ok(new { discount });
    }

    /// <summary>
    /// Yeni kupon oluşturur (Admin only
    /// </summary>
    /// <param name="dto">Kupon oluşturma bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kupon</returns>
    /// <response code="201">Kupon başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Yeni kupon oluşturur",
        Description = "Yeni bir kupon oluşturur. Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CouponDto>> Create(
        [FromBody] CreateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new CreateCouponCommand(
            dto.Code,
            dto.Description,
            dto.DiscountAmount,
            dto.DiscountPercentage,
            dto.StartDate,
            dto.EndDate,
            dto.UsageLimit,
            dto.MinimumPurchaseAmount,
            dto.MaximumDiscountAmount,
            dto.IsForNewUsersOnly,
            dto.ApplicableCategoryIds,
            dto.ApplicableProductIds);
        
        var coupon = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByCode), new { code = coupon.Code }, coupon);
    }

    /// <summary>
    /// Kupon bilgilerini günceller (Admin only)
    /// </summary>
    /// <param name="id">Kupon ID'si</param>
    /// <param name="dto">Kupon güncelleme bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenen kupon</returns>
    /// <response code="200">Kupon başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Kupon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Kupon bilgilerini günceller",
        Description = "Mevcut bir kuponun bilgilerini günceller. Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CouponDto>> Update(
        Guid id,
        [FromBody] UpdateCouponDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new UpdateCouponCommand(
            id,
            dto.Code,
            dto.Description,
            dto.DiscountAmount,
            dto.DiscountPercentage,
            dto.StartDate,
            dto.EndDate,
            dto.UsageLimit,
            dto.MinimumPurchaseAmount,
            dto.MaximumDiscountAmount,
            dto.IsActive,
            dto.IsForNewUsersOnly,
            dto.ApplicableCategoryIds,
            dto.ApplicableProductIds);
        
        var coupon = await _mediator.Send(command, cancellationToken);
        return Ok(coupon);
    }

    /// <summary>
    /// Kuponu siler (Admin only)
    /// </summary>
    /// <param name="id">Kupon ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="204">Kupon başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Kupon bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Kuponu siler",
        Description = "Mevcut bir kuponu siler (soft delete). Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new DeleteCouponCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}
