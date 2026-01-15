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
using Merge.Application.Cart.Commands.RemoveCartItem;
using Merge.Application.Cart.Commands.ClearCart;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Cart;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart")]
[Authorize]
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
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var query = new GetCartByUserIdQuery(userId);
        var cart = await mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateCartLinks(Url, cart.Id, cart.UserId, version);
        
        return Ok(new { cart, _links = links });
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var userId = GetUserId();
        var command = new AddItemToCartCommand(userId, dto.ProductId, dto.Quantity);
        var cartItem = await mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateCartItemLinks(Url, cartItem.Id, cartItem.ProductId, version);
        
        return Ok(new { cartItem, _links = links });
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var cartQuery = new GetCartByCartItemIdQuery(cartItemId);
        var cart = await mediator.Send(cartQuery, cancellationToken);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (cart is null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new UpdateCartItemCommand(cartItemId, dto.Quantity);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var cartQuery = new GetCartByCartItemIdQuery(cartItemId);
        var cart = await mediator.Send(cartQuery, cancellationToken);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (cart is null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new RemoveCartItemCommand(cartItemId);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var command = new ClearCartCommand(userId);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

