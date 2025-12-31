using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Product;

[ApiController]
[Route("api/products/comparisons")]
public class ProductComparisonsController : BaseController
{
    private readonly IProductComparisonService _comparisonService;

    public ProductComparisonsController(IProductComparisonService comparisonService)
    {
        _comparisonService = comparisonService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductComparisonDto>> CreateComparison([FromBody] CreateComparisonDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var comparison = await _comparisonService.CreateComparisonAsync(userId, dto);
        return CreatedAtAction(nameof(GetComparison), new { id = comparison.Id }, comparison);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductComparisonDto>> GetComparison(Guid id)
    {
        var comparison = await _comparisonService.GetComparisonAsync(id);

        if (comparison == null)
        {
            return NotFound();
        }

        return Ok(comparison);
    }

    [HttpGet("current")]
    [Authorize]
    public async Task<ActionResult<ProductComparisonDto>> GetCurrentComparison()
    {
        var userId = GetUserId();
        var comparison = await _comparisonService.GetUserComparisonAsync(userId);
        return Ok(comparison);
    }

    [HttpGet("my-comparisons")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductComparisonDto>>> GetMyComparisons([FromQuery] bool savedOnly = false)
    {
        var userId = GetUserId();
        var comparisons = await _comparisonService.GetUserComparisonsAsync(userId, savedOnly);
        return Ok(comparisons);
    }

    [HttpGet("shared/{shareCode}")]
    public async Task<ActionResult<ProductComparisonDto>> GetSharedComparison(string shareCode)
    {
        var comparison = await _comparisonService.GetComparisonByShareCodeAsync(shareCode);

        if (comparison == null)
        {
            return NotFound();
        }

        return Ok(comparison);
    }

    [HttpPost("add")]
    [Authorize]
    public async Task<ActionResult<ProductComparisonDto>> AddProduct([FromBody] AddToComparisonDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var comparison = await _comparisonService.AddProductToComparisonAsync(userId, dto.ProductId);
        return Ok(comparison);
    }

    [HttpDelete("remove/{productId}")]
    [Authorize]
    public async Task<IActionResult> RemoveProduct(Guid productId)
    {
        var userId = GetUserId();
        var success = await _comparisonService.RemoveProductFromComparisonAsync(userId, productId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("save")]
    [Authorize]
    public async Task<IActionResult> SaveComparison([FromBody] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("İsim boş olamaz.");
        }

        var userId = GetUserId();
        var success = await _comparisonService.SaveComparisonAsync(userId, name);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/share")]
    [Authorize]
    public async Task<ActionResult<string>> GenerateShareCode(Guid id)
    {
        var shareCode = await _comparisonService.GenerateShareCodeAsync(id);
        return Ok(new { shareCode });
    }

    [HttpDelete("clear")]
    [Authorize]
    public async Task<IActionResult> ClearComparison()
    {
        var userId = GetUserId();
        var success = await _comparisonService.ClearComparisonAsync(userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComparison(Guid id)
    {
        var userId = GetUserId();
        var success = await _comparisonService.DeleteComparisonAsync(id, userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{id}/matrix")]
    public async Task<ActionResult<ComparisonMatrixDto>> GetComparisonMatrix(Guid id)
    {
        var matrix = await _comparisonService.GetComparisonMatrixAsync(id);
        return Ok(matrix);
    }
}
