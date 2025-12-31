using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;


namespace Merge.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/flash-sales")]
public class FlashSalesController : BaseController
{
    private readonly IFlashSaleService _flashSaleService;
        public FlashSalesController(IFlashSaleService flashSaleService)
    {
        _flashSaleService = flashSaleService;
            }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FlashSaleDto>>> GetActiveSales()
    {
        var sales = await _flashSaleService.GetActiveSalesAsync();
        return Ok(sales);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<FlashSaleDto>>> GetAll()
    {
        var sales = await _flashSaleService.GetAllAsync();
        return Ok(sales);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FlashSaleDto>> GetById(Guid id)
    {
        var sale = await _flashSaleService.GetByIdAsync(id);
        if (sale == null)
        {
            return NotFound();
        }
        return Ok(sale);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FlashSaleDto>> Create([FromBody] CreateFlashSaleDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sale = await _flashSaleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FlashSaleDto>> Update(Guid id, [FromBody] UpdateFlashSaleDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sale = await _flashSaleService.UpdateAsync(id, dto);
        if (sale == null)
        {
            return NotFound();
        }
        return Ok(sale);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _flashSaleService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{flashSaleId}/products")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddProduct(Guid flashSaleId, [FromBody] AddProductToSaleDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _flashSaleService.AddProductToSaleAsync(flashSaleId, dto);
        return NoContent();
    }

    [HttpDelete("{flashSaleId}/products/{productId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveProduct(Guid flashSaleId, Guid productId)
    {
        var result = await _flashSaleService.RemoveProductFromSaleAsync(flashSaleId, productId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

