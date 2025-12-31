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
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();
        var cart = await _cartService.GetCartByUserIdAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartItemDto>> AddItem([FromBody] AddCartItemDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var cartItem = await _cartService.AddItemToCartAsync(userId, dto.ProductId, dto.Quantity);
        return Ok(cartItem);
    }

    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        
        // ✅ SECURITY: Authorization check - Users can only update their own cart items
        var cartItem = await _cartService.GetCartItemByIdAsync(cartItemId);
        if (cartItem == null)
        {
            return NotFound();
        }

        // Get cart to check UserId
        var cart = await _cartService.GetCartByCartItemIdAsync(cartItemId);
        if (cart == null || cart.UserId != userId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }
        }

        var result = await _cartService.UpdateCartItemQuantityAsync(cartItemId, dto.Quantity);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId)
    {
        var userId = GetUserId();
        
        // ✅ SECURITY: Authorization check - Users can only remove their own cart items
        var cartItem = await _cartService.GetCartItemByIdAsync(cartItemId);
        if (cartItem == null)
        {
            return NotFound();
        }

        // Get cart to check UserId
        var cart = await _cartService.GetCartByCartItemIdAsync(cartItemId);
        if (cart == null || cart.UserId != userId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }
        }

        var result = await _cartService.RemoveItemFromCartAsync(cartItemId);
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

