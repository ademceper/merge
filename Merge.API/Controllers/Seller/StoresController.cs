using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Seller;
using Merge.Application.DTOs.Seller;


namespace Merge.API.Controllers.Seller;

[ApiController]
[Route("api/seller/stores")]
public class StoresController : BaseController
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<StoreDto>> GetStore(Guid id)
    {
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
        {
            return NotFound();
        }
        return Ok(store);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<StoreDto>> GetStoreBySlug(string slug)
    {
        var store = await _storeService.GetStoreBySlugAsync(slug);
        if (store == null)
        {
            return NotFound();
        }
        return Ok(store);
    }

    [HttpGet("seller/{sellerId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetSellerStores(Guid sellerId, [FromQuery] string? status = null)
    {
        var stores = await _storeService.GetSellerStoresAsync(sellerId, status);
        return Ok(stores);
    }

    [HttpGet("seller/{sellerId}/primary")]
    [AllowAnonymous]
    public async Task<ActionResult<StoreDto>> GetPrimaryStore(Guid sellerId)
    {
        var store = await _storeService.GetPrimaryStoreAsync(sellerId);
        if (store == null)
        {
            return NotFound();
        }
        return Ok(store);
    }

    [HttpGet("{id}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<StoreStatsDto>> GetStoreStats(Guid id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var stats = await _storeService.GetStoreStatsAsync(id, startDate, endDate);
        return Ok(stats);
    }

    // Seller endpoints
    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<StoreDto>> CreateStore([FromBody] CreateStoreDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sellerId = GetUserId();
        var store = await _storeService.CreateStoreAsync(sellerId, dto);
        return CreatedAtAction(nameof(GetStore), new { id = store.Id }, store);
    }

    [HttpGet("my-stores")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<IEnumerable<StoreDto>>> GetMyStores([FromQuery] string? status = null)
    {
        var sellerId = GetUserId();
        var stores = await _storeService.GetSellerStoresAsync(sellerId, status);
        return Ok(stores);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> UpdateStore(Guid id, [FromBody] UpdateStoreDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sellerId = GetUserId();

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi mağazalarını güncelleyebilir
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
        {
            return NotFound();
        }

        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _storeService.UpdateStoreAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> DeleteStore(Guid id)
    {
        var sellerId = GetUserId();

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi mağazalarını silebilir
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
        {
            return NotFound();
        }

        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _storeService.DeleteStoreAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/set-primary")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> SetPrimaryStore(Guid id)
    {
        var sellerId = GetUserId();

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi mağazalarını primary yapabilir
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null)
        {
            return NotFound();
        }

        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var success = await _storeService.SetPrimaryStoreAsync(sellerId, id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Admin endpoints
    [HttpPost("{id}/verify")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> VerifyStore(Guid id)
    {
        var success = await _storeService.VerifyStoreAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/suspend")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SuspendStore(Guid id, [FromBody] SuspendStoreDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _storeService.SuspendStoreAsync(id, dto.Reason);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

