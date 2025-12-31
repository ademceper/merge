using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Product;

[ApiController]
[Route("api/products/size-guides")]
public class SizeGuidesController : BaseController
{
    private readonly ISizeGuideService _sizeGuideService;

    public SizeGuidesController(ISizeGuideService sizeGuideService)
    {
        _sizeGuideService = sizeGuideService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SizeGuideDto>> CreateSizeGuide([FromBody] CreateSizeGuideDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sizeGuide = await _sizeGuideService.CreateSizeGuideAsync(dto);
        return CreatedAtAction(nameof(GetSizeGuide), new { id = sizeGuide.Id }, sizeGuide);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SizeGuideDto>> GetSizeGuide(Guid id)
    {
        var sizeGuide = await _sizeGuideService.GetSizeGuideAsync(id);

        if (sizeGuide == null)
        {
            return NotFound();
        }

        return Ok(sizeGuide);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<SizeGuideDto>>> GetSizeGuidesByCategory(Guid categoryId)
    {
        var sizeGuides = await _sizeGuideService.GetSizeGuidesByCategoryAsync(categoryId);
        return Ok(sizeGuides);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SizeGuideDto>>> GetAllSizeGuides()
    {
        var sizeGuides = await _sizeGuideService.GetAllSizeGuidesAsync();
        return Ok(sizeGuides);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateSizeGuide(Guid id, [FromBody] CreateSizeGuideDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _sizeGuideService.UpdateSizeGuideAsync(id, dto);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteSizeGuide(Guid id)
    {
        var success = await _sizeGuideService.DeleteSizeGuideAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    public async Task<IActionResult> AssignSizeGuideToProduct([FromBody] AssignSizeGuideDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        await _sizeGuideService.AssignSizeGuideToProductAsync(dto);
        return NoContent();
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ProductSizeGuideDto>> GetProductSizeGuide(Guid productId)
    {
        var productSizeGuide = await _sizeGuideService.GetProductSizeGuideAsync(productId);

        if (productSizeGuide == null)
        {
            return NotFound();
        }

        return Ok(productSizeGuide);
    }

    [HttpDelete("product/{productId}")]
    [Authorize(Roles = "Admin,Manager,Seller")]
    public async Task<IActionResult> RemoveSizeGuideFromProduct(Guid productId)
    {
        var success = await _sizeGuideService.RemoveSizeGuideFromProductAsync(productId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("recommend")]
    public async Task<ActionResult<SizeRecommendationDto>> GetSizeRecommendation([FromBody] GetSizeRecommendationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var recommendation = await _sizeGuideService.GetSizeRecommendationAsync(
            dto.ProductId, 
            dto.Height, 
            dto.Weight, 
            dto.Chest, 
            dto.Waist);
        return Ok(recommendation);
    }
}
