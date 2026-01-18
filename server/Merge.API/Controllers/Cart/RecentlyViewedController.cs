using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Queries.GetRecentlyViewed;
using Merge.Application.Cart.Commands.AddToRecentlyViewed;
using Merge.Application.Cart.Commands.ClearRecentlyViewed;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Cart;

/// <summary>
/// Recently Viewed API endpoints.
/// Son görüntülenen ürünleri yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart/recently-viewed")]
[Authorize]
[Tags("RecentlyViewed")]
public class RecentlyViewedController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Son görüntülenen ürünleri getirir
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış son görüntülenen ürün listesi</returns>
    /// <response code="200">Son görüntülenen ürünler başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetRecentlyViewed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var query = new GetRecentlyViewedQuery(userId, page, pageSize);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Ürünü son görüntülenenlere ekler
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Ürün başarıyla son görüntülenenlere eklendi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="404">Ürün bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{productId}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (yüksek limit - tracking için)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddToRecentlyViewed(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new AddToRecentlyViewedCommand(userId, productId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Son görüntülenen ürünleri temizler
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Son görüntülenen ürünler başarıyla temizlendi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearRecentlyViewed(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new ClearRecentlyViewedCommand(userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

