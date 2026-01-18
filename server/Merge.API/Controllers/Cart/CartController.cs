using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Queries.GetCartByUserId;
using Merge.Application.Cart.Queries.GetCartByCartItemId;
using Merge.Application.Cart.Commands.AddItemToCart;
using Merge.Application.Cart.Commands.UpdateCartItem;
using Merge.Application.Cart.Commands.PatchCartItem;
using Merge.Application.Cart.Commands.RemoveCartItem;
using Merge.Application.Cart.Commands.ClearCart;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.API.Extensions;
using Merge.Application.Exceptions;

namespace Merge.API.Controllers.Cart;

/// <summary>
/// Cart API endpoints.
/// Sepet işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart")]
[Authorize]
[Tags("Cart")]
public class CartController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Kullanıcının sepetini getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kullanıcının sepeti</returns>
    /// <response code="200">Sepet başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetCartByUserIdQuery(userId);
        var cart = await mediator.Send(query, cancellationToken);
        
        var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
        Response.SetETag(cartJson);
        Response.SetCacheControl(maxAgeSeconds: 30, isPublic: false); // Cache for 30 seconds (private - user-specific)

        // Check if client has cached version (304 Not Modified)
        var etag = Response.Headers["ETag"].FirstOrDefault();
        if (!string.IsNullOrEmpty(etag) && Request.IsNotModified(etag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        
        return Ok(cart);
    }

    /// <summary>
    /// Sepete ürün ekler
    /// </summary>
    /// <param name="dto">Sepete eklenecek ürün bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Eklenen sepet öğesi</returns>
    /// <response code="200">Ürün başarıyla sepete eklendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="422">İş kuralı ihlali (örn: ürün stokta yok, ürün aktif değil)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("items")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CartItemDto>> AddItem([FromBody] AddCartItemDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new AddItemToCartCommand(userId, dto.ProductId, dto.Quantity);
        var cartItem = await mediator.Send(command, cancellationToken);
        
        return Ok(cartItem);
    }

    /// <summary>
    /// Sepet öğesi miktarını günceller
    /// </summary>
    /// <param name="cartItemId">Sepet öğesi ID</param>
    /// <param name="dto">Güncellenecek miktar bilgisi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Sepet öğesi başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu sepet öğesine erişim yetkisi yok</response>
    /// <response code="404">Sepet öğesi bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: yetersiz stok)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("items/{cartItemId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        var cartQuery = new GetCartByCartItemIdQuery(cartItemId);
        var cart = await mediator.Send(cartQuery, cancellationToken)
            ?? throw new NotFoundException("CartItem", cartItemId);

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new UpdateCartItemCommand(cartItemId, dto.Quantity);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("CartItem", cartItemId);

        return NoContent();
    }

    /// <summary>
    /// Sepet öğesini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("items/{cartItemId}")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchItem(
        Guid cartItemId,
        [FromBody] PatchCartItemDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var userId = GetUserId();

        var cartQuery = new GetCartByCartItemIdQuery(cartItemId);
        var cart = await mediator.Send(cartQuery, cancellationToken)
            ?? throw new NotFoundException("CartItem", cartItemId);

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new PatchCartItemCommand(cartItemId, patchDto.Quantity);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("CartItem", cartItemId);

        return NoContent();
    }

    /// <summary>
    /// Sepetten ürün kaldırır
    /// </summary>
    /// <param name="cartItemId">Sepet öğesi ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Ürün başarıyla sepetten kaldırıldı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu sepet öğesine erişim yetkisi yok</response>
    /// <response code="404">Sepet öğesi bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("items/{cartItemId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        var cartQuery = new GetCartByCartItemIdQuery(cartItemId);
        var cart = await mediator.Send(cartQuery, cancellationToken)
            ?? throw new NotFoundException("CartItem", cartItemId);

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new RemoveCartItemCommand(cartItemId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("CartItem", cartItemId);

        return NoContent();
    }

    /// <summary>
    /// Sepeti temizler (tüm ürünleri kaldırır)
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Sepet başarıyla temizlendi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="404">Sepet bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new ClearCartCommand(userId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("Cart", userId);

        return NoContent();
    }
}

