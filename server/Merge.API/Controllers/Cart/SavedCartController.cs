using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Queries.GetSavedItems;
using Merge.Application.Cart.Commands.SaveItem;
using Merge.Application.Cart.Commands.RemoveSavedItem;
using Merge.Application.Cart.Commands.MoveToCart;
using Merge.Application.Cart.Commands.ClearSavedItems;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Cart;

/// <summary>
/// Saved Cart API endpoints.
/// Kaydedilmiş sepet işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart/saved")]
[Authorize]
[Tags("SavedCart")]
public class SavedCartController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Kaydedilmiş sepet öğelerini getirir
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış kaydedilmiş sepet öğeleri</returns>
    /// <response code="200">Kaydedilmiş sepet öğeleri başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<SavedCartItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SavedCartItemDto>>> GetSavedItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var query = new GetSavedItemsQuery(userId, page, pageSize);
        var items = await mediator.Send(query, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Ürünü kaydedilmiş sepet öğelerine ekler
    /// </summary>
    /// <param name="dto">Kaydedilecek ürün bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kaydedilen sepet öğesi</returns>
    /// <response code="201">Ürün başarıyla kaydedildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="404">Ürün bulunamadı</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(SavedCartItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SavedCartItemDto>> SaveItem(
        [FromBody] SaveItemDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new SaveItemCommand(userId, dto.ProductId, dto.Quantity, dto.Notes);
        var item = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSavedItems), new { id = item.Id }, item);
    }

    /// <summary>
    /// Kaydedilmiş sepet öğesini kaldırır
    /// </summary>
    /// <param name="id">Kaydedilmiş sepet öğesi ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Kaydedilmiş sepet öğesi başarıyla kaldırıldı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu sepet öğesine erişim yetkisi yok</response>
    /// <response code="404">Kaydedilmiş sepet öğesi bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("{id}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveItem(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        var command = new RemoveSavedItemCommand(userId, id);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Kaydedilmiş sepet öğesini sepete taşır
    /// </summary>
    /// <param name="id">Kaydedilmiş sepet öğesi ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Ürün başarıyla sepete taşındı</response>
    /// <response code="400">Geçersiz istek (örn: ürün stokta yok)</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu sepet öğesine erişim yetkisi yok</response>
    /// <response code="404">Kaydedilmiş sepet öğesi bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: yetersiz stok)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/move-to-cart")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MoveToCart(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        var command = new MoveToCartCommand(userId, id);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return BadRequest("Ürün sepete eklenemedi.");
        }
        return NoContent();
    }

    /// <summary>
    /// Tüm kaydedilmiş sepet öğelerini temizler
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Tüm kaydedilmiş sepet öğeleri başarıyla temizlendi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearSavedItems(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new ClearSavedItemsCommand(userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

