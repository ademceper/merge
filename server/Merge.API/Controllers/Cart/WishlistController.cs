using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Queries.GetWishlist;
using Merge.Application.Cart.Queries.IsInWishlist;
using Merge.Application.Cart.Commands.AddToWishlist;
using Merge.Application.Cart.Commands.RemoveFromWishlist;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Cart;

/// <summary>
/// Wishlist API endpoints.
/// İstek listesi işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart/wishlist")]
[Authorize]
[Tags("Wishlist")]
public class WishlistController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// İstek listesini getirir
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış istek listesi</returns>
    /// <response code="200">İstek listesi başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetWishlist(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var query = new GetWishlistQuery(userId, page, pageSize);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// İstek listesine ürün ekler
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Ürün başarıyla istek listesine eklendi</response>
    /// <response code="400">Geçersiz istek (örn: ürün zaten listede)</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="404">Ürün bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{productId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AddToWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new AddToWishlistCommand(userId, productId);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return Problem("Invalid request", "Bad Request", StatusCodes.Status400BadRequest);
        }
        return NoContent();
    }

    /// <summary>
    /// İstek listesinden ürün kaldırır
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Ürün başarıyla istek listesinden kaldırıldı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="404">Ürün istek listesinde bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("{productId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new RemoveFromWishlistCommand(userId, productId);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Ürünün istek listesinde olup olmadığını kontrol eder
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ürünün istek listesinde olup olmadığı</returns>
    /// <response code="200">Kontrol sonucu başarıyla döndürüldü</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{productId}/check")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60 istek / dakika (yüksek limit - sık kullanılan)
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<bool>> IsInWishlist(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new IsInWishlistQuery(userId, productId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

