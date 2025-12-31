using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Product;

[ApiController]
[Route("api/products/bundles")]
public class BundlesController : BaseController
{
    private readonly IProductBundleService _bundleService;
        public BundlesController(IProductBundleService bundleService)
    {
        _bundleService = bundleService;
            }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductBundleDto>>> GetActiveBundles()
    {
        var bundles = await _bundleService.GetActiveBundlesAsync();
        return Ok(bundles);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ProductBundleDto>>> GetAll()
    {
        var bundles = await _bundleService.GetAllAsync();
        return Ok(bundles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductBundleDto>> GetById(Guid id)
    {
        var bundle = await _bundleService.GetByIdAsync(id);
        if (bundle == null)
        {
            return NotFound();
        }
        return Ok(bundle);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductBundleDto>> Create([FromBody] CreateProductBundleDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var bundle = await _bundleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = bundle.Id }, bundle);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductBundleDto>> Update(Guid id, [FromBody] UpdateProductBundleDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var bundle = await _bundleService.UpdateAsync(id, dto);
        if (bundle == null)
        {
            return NotFound();
        }
        return Ok(bundle);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _bundleService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{bundleId}/products")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddProduct(Guid bundleId, [FromBody] AddProductToBundleDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _bundleService.AddProductToBundleAsync(bundleId, dto);
        return NoContent();
    }

    [HttpDelete("{bundleId}/products/{productId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveProduct(Guid bundleId, Guid productId)
    {
        var result = await _bundleService.RemoveProductFromBundleAsync(bundleId, productId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

