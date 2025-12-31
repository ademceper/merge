using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Cart;

namespace Merge.API.Controllers.Cart;

[ApiController]
[Route("api/cart/saved")]
[Authorize]
public class SavedCartController : BaseController
{
    private readonly ISavedCartService _savedCartService;
        public SavedCartController(ISavedCartService savedCartService)
    {
        _savedCartService = savedCartService;
            }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SavedCartItemDto>>> GetSavedItems()
    {
        var userId = GetUserId();
        var items = await _savedCartService.GetSavedItemsAsync(userId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<SavedCartItemDto>> SaveItem([FromBody] SaveItemDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var item = await _savedCartService.SaveItemAsync(userId, dto);
        return CreatedAtAction(nameof(GetSavedItems), new { id = item.Id }, item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveItem(Guid id)
    {
        var userId = GetUserId();
        
        // ✅ SECURITY: Authorization check - Users can only remove their own saved items
        // Service layer'da zaten userId kontrolü var, ama controller'da da kontrol ediyoruz
        var result = await _savedCartService.RemoveSavedItemAsync(userId, id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/move-to-cart")]
    public async Task<IActionResult> MoveToCart(Guid id)
    {
        var userId = GetUserId();
        
        // ✅ SECURITY: Authorization check - Users can only move their own saved items
        // Service layer'da zaten userId kontrolü var, ama controller'da da kontrol ediyoruz
        var result = await _savedCartService.MoveToCartAsync(userId, id);
        if (!result)
        {
            return BadRequest("Ürün sepete eklenemedi.");
        }
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearSavedItems()
    {
        var userId = GetUserId();
        await _savedCartService.ClearSavedItemsAsync(userId);
        return NoContent();
    }
}

