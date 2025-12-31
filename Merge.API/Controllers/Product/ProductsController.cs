using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Product;

[ApiController]
[Route("api/products")]
public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var products = await _productService.GetAllAsync(page, pageSize);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var products = await _productService.GetByCategoryAsync(categoryId, page, pageSize);
        return Ok(products);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var products = await _productService.SearchAsync(q, page, pageSize);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductDto productDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var product = await _productService.CreateAsync(productDto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] ProductDto productDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var product = await _productService.UpdateAsync(id, productDto);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

