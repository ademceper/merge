using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Cart;


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

    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var cart = await _cartService.GetCartByUserIdAsync(userId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartItemDto>> AddItem([FromBody] AddCartItemDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var cartItem = await _cartService.AddItemToCartAsync(userId, dto.ProductId, dto.Quantity, cancellationToken);
        return Ok(cartItem);
    }

    [HttpPut("items/{cartItemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        
        // ✅ SECURITY: Authorization check - Users can only update their own cart items
        var cartItem = await _cartService.GetCartItemByIdAsync(cartItemId, cancellationToken);
        if (cartItem == null)
        {
            return NotFound();
        }

        // Get cart to check UserId
        var cart = await _cartService.GetCartByCartItemIdAsync(cartItemId, cancellationToken);
        if (cart == null || cart.UserId != userId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }
        }

        var result = await _cartService.UpdateCartItemQuantityAsync(cartItemId, dto.Quantity, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("items/{cartItemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ SECURITY: Authorization check - Users can only remove their own cart items
        var cartItem = await _cartService.GetCartItemByIdAsync(cartItemId, cancellationToken);
        if (cartItem == null)
        {
            return NotFound();
        }

        // Get cart to check UserId
        var cart = await _cartService.GetCartByCartItemIdAsync(cartItemId, cancellationToken);
        if (cart == null || cart.UserId != userId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }
        }

        var result = await _cartService.RemoveItemFromCartAsync(cartItemId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        var result = await _cartService.ClearCartAsync(userId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

