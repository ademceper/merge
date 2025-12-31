using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.DTOs.Logistics;


namespace Merge.API.Controllers.Logistics;

[ApiController]
[Route("api/logistics/stock-movements")]
[Authorize(Roles = "Admin,Seller")]
public class StockMovementsController : BaseController
{
    private readonly IStockMovementService _stockMovementService;

    public StockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockMovementDto>> GetById(Guid id)
    {
        var movement = await _stockMovementService.GetByIdAsync(id);
        if (movement == null)
        {
            return NotFound();
        }
        return Ok(movement);
    }

    [HttpGet("inventory/{inventoryId}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByInventory(Guid inventoryId)
    {
        var movements = await _stockMovementService.GetByInventoryIdAsync(inventoryId);
        return Ok(movements);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var movements = await _stockMovementService.GetByProductIdAsync(productId, page, pageSize);
        return Ok(movements);
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByWarehouse(
        Guid warehouseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var movements = await _stockMovementService.GetByWarehouseIdAsync(warehouseId, page, pageSize);
        return Ok(movements);
    }

    [HttpPost("filter")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetFiltered([FromBody] StockMovementFilterDto filter)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var movements = await _stockMovementService.GetFilteredAsync(filter);
        return Ok(movements);
    }

    [HttpPost]
    public async Task<ActionResult<StockMovementDto>> Create([FromBody] CreateStockMovementDto createDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var movement = await _stockMovementService.CreateAsync(createDto, userId);
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
    }
}
