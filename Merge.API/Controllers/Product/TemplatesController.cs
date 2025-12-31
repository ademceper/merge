using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Product;

[ApiController]
[Route("api/products/templates")]
public class ProductTemplatesController : BaseController
{
    private readonly IProductTemplateService _productTemplateService;

    public ProductTemplatesController(IProductTemplateService productTemplateService)
    {
        _productTemplateService = productTemplateService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductTemplateDto>>> GetAllTemplates(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null)
    {
        var templates = await _productTemplateService.GetAllTemplatesAsync(categoryId, isActive);
        return Ok(templates);
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductTemplateDto>>> GetPopularTemplates([FromQuery] int limit = 10)
    {
        var templates = await _productTemplateService.GetPopularTemplatesAsync(limit);
        return Ok(templates);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductTemplateDto>> GetTemplate(Guid id)
    {
        var template = await _productTemplateService.GetTemplateByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ProductTemplateDto>> CreateTemplate([FromBody] CreateProductTemplateDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var template = await _productTemplateService.CreateTemplateAsync(dto);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateProductTemplateDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _productTemplateService.UpdateTemplateAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var success = await _productTemplateService.DeleteTemplateAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("create-product")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<ProductDto>> CreateProductFromTemplate([FromBody] CreateProductFromTemplateDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var product = await _productTemplateService.CreateProductFromTemplateAsync(dto);
        return CreatedAtAction(nameof(GetTemplate), new { id = product.Id }, product);
    }
}

