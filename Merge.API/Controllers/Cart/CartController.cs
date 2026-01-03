using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Cart;
using Merge.API.Middleware;


namespace Merge.API.Controllers.Cart;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Kullanıcının sepetini getirir
    /// </summary>
    [HttpGet]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var cart = await _cartService.GetCartByUserIdAsync(userId, cancellationToken);
        return Ok(cart);
    }

    /// <summary>
    /// Sepete ürün ekler
    /// </summary>
    [HttpPost("items")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CartItemDto>> AddItem([FromBody] AddCartItemDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var cartItem = await _cartService.AddItemToCartAsync(userId, dto.ProductId, dto.Quantity, cancellationToken);
        return Ok(cartItem);
    }

    /// <summary>
    /// Sepet öğesi miktarını günceller
    /// </summary>
    [HttpPut("items/{cartItemId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        // Get cart to check UserId (CartDto contains UserId)
        var cart = await _cartService.GetCartByCartItemIdAsync(cartItemId, cancellationToken);
        if (cart == null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var result = await _cartService.UpdateCartItemQuantityAsync(cartItemId, dto.Quantity, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sepetten ürün kaldırır
    /// </summary>
    [HttpDelete("items/{cartItemId}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        // Get cart to check UserId (CartDto contains UserId)
        var cart = await _cartService.GetCartByCartItemIdAsync(cartItemId, cancellationToken);
        if (cart == null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var result = await _cartService.RemoveItemFromCartAsync(cartItemId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sepeti temizler
    /// </summary>
    [HttpDelete]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _cartService.ClearCartAsync(userId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

