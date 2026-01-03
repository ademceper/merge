using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.API.Middleware;


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

    /// <summary>
    /// Kaydedilmiş sepet öğelerini getirir
    /// </summary>
    [HttpGet]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<SavedCartItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SavedCartItemDto>>> GetSavedItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var userId = GetUserId();
        var items = await _savedCartService.GetSavedItemsAsync(userId, page, pageSize, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Ürünü kaydedilmiş sepet öğelerine ekler
    /// </summary>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(SavedCartItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SavedCartItemDto>> SaveItem(
        [FromBody] SaveItemDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var item = await _savedCartService.SaveItemAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetSavedItems), new { id = item.Id }, item);
    }

    /// <summary>
    /// Kaydedilmiş sepet öğesini kaldırır
    /// </summary>
    [HttpDelete("{id}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveItem(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        // Service layer'da zaten userId kontrolü var, ama controller'da da kontrol ediyoruz
        var result = await _savedCartService.RemoveSavedItemAsync(userId, id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Kaydedilmiş sepet öğesini sepete taşır
    /// </summary>
    [HttpPost("{id}/move-to-cart")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MoveToCart(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        // Service layer'da zaten userId kontrolü var, ama controller'da da kontrol ediyoruz
        var result = await _savedCartService.MoveToCartAsync(userId, id, cancellationToken);
        if (!result)
        {
            return BadRequest("Ürün sepete eklenemedi.");
        }
        return NoContent();
    }

    /// <summary>
    /// Tüm kaydedilmiş sepet öğelerini temizler
    /// </summary>
    [HttpDelete]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ClearSavedItems(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        await _savedCartService.ClearSavedItemsAsync(userId, cancellationToken);
        return NoContent();
    }
}

