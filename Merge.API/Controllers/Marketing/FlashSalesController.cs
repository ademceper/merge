using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Marketing.Queries.GetActiveFlashSales;
using Merge.Application.Marketing.Queries.GetAllFlashSales;
using Merge.Application.Marketing.Queries.GetFlashSaleById;
using Merge.Application.Marketing.Commands.CreateFlashSale;
using Merge.Application.Marketing.Commands.UpdateFlashSale;
using Merge.Application.Marketing.Commands.DeleteFlashSale;
using Merge.Application.Marketing.Commands.AddProductToFlashSale;
using Merge.Application.Marketing.Commands.RemoveProductFromFlashSale;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Marketing;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/marketing/flash-sales")]
public class FlashSalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly MarketingSettings _marketingSettings;

    public FlashSalesController(
        IMediator mediator,
        IOptions<MarketingSettings> marketingSettings)
    {
        _mediator = mediator;
        _marketingSettings = marketingSettings.Value;
    }

    /// <summary>
    /// Aktif flash sale'leri getirir (pagination ile)
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Aktif flash sale listesi</returns>
    /// <response code="200">Aktif flash sale'ler başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Aktif flash sale'leri getirir",
        Description = "Sayfalama ile aktif flash sale'leri getirir. Herkese açık endpoint.")]
    [ProducesResponseType(typeof(PagedResult<FlashSaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FlashSaleDto>>> GetActiveSales(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetActiveFlashSalesQuery(PageNumber: page, PageSize: pageSize);
        var sales = await _mediator.Send(query, cancellationToken);
        return Ok(sales);
    }

    /// <summary>
    /// Tüm flash sale'leri getirir (pagination ile) (Admin only)
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tüm flash sale listesi</returns>
    /// <response code="200">Flash sale'ler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Tüm flash sale'leri getirir",
        Description = "Sayfalama ile tüm flash sale'leri getirir (aktif ve pasif). Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(typeof(PagedResult<FlashSaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FlashSaleDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        if (pageSize > _marketingSettings.MaxPageSize) pageSize = _marketingSettings.MaxPageSize;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetAllFlashSalesQuery(PageNumber: page, PageSize: pageSize);
        var sales = await _mediator.Send(query, cancellationToken);
        return Ok(sales);
    }

    /// <summary>
    /// Flash sale detaylarını getirir
    /// </summary>
    /// <param name="id">Flash sale ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Flash sale detayları</returns>
    /// <response code="200">Flash sale başarıyla getirildi</response>
    /// <response code="404">Flash sale bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [SwaggerOperation(
        Summary = "Flash sale detaylarını getirir",
        Description = "Flash sale ID'sine göre flash sale detaylarını getirir. Herkese açık endpoint.")]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FlashSaleDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var query = new GetFlashSaleByIdQuery(id);
        var sale = await _mediator.Send(query, cancellationToken);
        
        if (sale == null)
        {
            return NotFound();
        }
        
        return Ok(sale);
    }

    /// <summary>
    /// Yeni flash sale oluşturur (Admin only)
    /// </summary>
    /// <param name="dto">Flash sale oluşturma bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan flash sale</returns>
    /// <response code="201">Flash sale başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Yeni flash sale oluşturur",
        Description = "Yeni bir flash sale oluşturur. Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FlashSaleDto>> Create(
        [FromBody] CreateFlashSaleDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new CreateFlashSaleCommand(
            dto.Title,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.BannerImageUrl);
        
        var sale = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
    }

    /// <summary>
    /// Flash sale bilgilerini günceller (Admin only)
    /// </summary>
    /// <param name="id">Flash sale ID'si</param>
    /// <param name="dto">Flash sale güncelleme bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenen flash sale</returns>
    /// <response code="200">Flash sale başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Flash sale bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Flash sale bilgilerini günceller",
        Description = "Mevcut bir flash sale'in bilgilerini günceller. Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(typeof(FlashSaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FlashSaleDto>> Update(
        Guid id,
        [FromBody] UpdateFlashSaleDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new UpdateFlashSaleCommand(
            id,
            dto.Title,
            dto.Description,
            dto.StartDate,
            dto.EndDate,
            dto.IsActive,
            dto.BannerImageUrl);
        
        var sale = await _mediator.Send(command, cancellationToken);
        return Ok(sale);
    }

    /// <summary>
    /// Flash sale'i siler (Admin only)
    /// </summary>
    /// <param name="id">Flash sale ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="204">Flash sale başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Flash sale bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [SwaggerOperation(
        Summary = "Flash sale'i siler",
        Description = "Mevcut bir flash sale'i siler (soft delete). Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
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
        var command = new DeleteFlashSaleCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    /// <summary>
    /// Flash sale'e ürün ekler (Admin only)
    /// </summary>
    /// <param name="flashSaleId">Flash sale ID'si</param>
    /// <param name="dto">Ürün ekleme bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ekleme işlemi sonucu</returns>
    /// <response code="204">Ürün flash sale'e başarıyla eklendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Flash sale veya ürün bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{flashSaleId}/products")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Flash sale'e ürün ekler",
        Description = "Mevcut bir flash sale'e ürün ekler. Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddProduct(
        Guid flashSaleId,
        [FromBody] AddProductToSaleDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new AddProductToFlashSaleCommand(
            flashSaleId,
            dto.ProductId,
            dto.SalePrice,
            dto.StockLimit,
            dto.SortOrder);
        
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Flash sale'den ürün kaldırır (Admin only)
    /// </summary>
    /// <param name="flashSaleId">Flash sale ID'si</param>
    /// <param name="productId">Ürün ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kaldırma işlemi sonucu</returns>
    /// <response code="204">Ürün flash sale'den başarıyla kaldırıldı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Flash sale veya ürün bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("{flashSaleId}/products/{productId}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [SwaggerOperation(
        Summary = "Flash sale'den ürün kaldırır",
        Description = "Mevcut bir flash sale'den ürün kaldırır. Sadece Admin rolüne sahip kullanıcılar bu işlemi yapabilir.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveProduct(
        Guid flashSaleId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder
        var command = new RemoveProductFromFlashSaleCommand(flashSaleId, productId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}
