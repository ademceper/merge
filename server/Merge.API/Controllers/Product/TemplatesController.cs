using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Product.Commands.CreateProductTemplate;
using Merge.Application.Product.Commands.UpdateProductTemplate;
using Merge.Application.Product.Commands.DeleteProductTemplate;
using Merge.Application.Product.Commands.CreateProductFromTemplate;
using Merge.Application.Product.Queries.GetProductTemplate;
using Merge.Application.Product.Queries.GetAllProductTemplates;
using Merge.Application.Product.Queries.GetPopularProductTemplates;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Product;

/// <summary>
/// Product Templates API endpoints.
/// Ürün şablonlarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/templates")]
[Tags("ProductTemplates")]
public class ProductTemplatesController(IMediator mediator) : BaseController
{
            [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductTemplateDto>>> GetAllTemplates(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllProductTemplatesQuery(categoryId, isActive);
        var templates = await mediator.Send(query, cancellationToken);
        return Ok(templates);
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductTemplateDto>>> GetPopularTemplates(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit > 100) limit = 100;
        if (limit < 1) limit = 10;
        var query = new GetPopularProductTemplatesQuery(limit);
        var templates = await mediator.Send(query, cancellationToken);
        return Ok(templates);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductTemplateDto>> GetTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetProductTemplateQuery(id);
        var template = await mediator.Send(query, cancellationToken);
        if (template == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(template);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(ProductTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductTemplateDto>> CreateTemplate(
        [FromBody] CreateProductTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new CreateProductTemplateCommand(
            dto.Name,
            dto.Description,
            dto.CategoryId,
            dto.Brand,
            dto.DefaultSKUPrefix,
            dto.DefaultPrice,
            dto.DefaultStockQuantity,
            dto.DefaultImageUrl,
            dto.Specifications?.ToDictionary(kv => kv.Key, kv => kv.Value),
            dto.Attributes?.ToDictionary(kv => kv.Key, kv => kv.Value),
            dto.IsActive);
        var template = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateTemplate(
        Guid id,
        [FromBody] UpdateProductTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new UpdateProductTemplateCommand(
            id,
            dto.Name,
            dto.Description,
            dto.CategoryId,
            dto.Brand,
            dto.DefaultSKUPrefix,
            dto.DefaultPrice,
            dto.DefaultStockQuantity,
            dto.DefaultImageUrl,
            dto.Specifications?.ToDictionary(kv => kv.Key, kv => kv.Value),
            dto.Attributes?.ToDictionary(kv => kv.Key, kv => kv.Value),
            dto.IsActive);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Ürün şablonunu kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchTemplate(
        Guid id,
        [FromBody] PatchProductTemplateDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new UpdateProductTemplateCommand(
            id,
            patchDto.Name,
            patchDto.Description,
            patchDto.CategoryId,
            patchDto.Brand,
            patchDto.DefaultSKUPrefix,
            patchDto.DefaultPrice,
            patchDto.DefaultStockQuantity,
            patchDto.DefaultImageUrl,
            patchDto.Specifications,
            patchDto.Attributes,
            patchDto.IsActive);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductTemplateCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("create-product")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> CreateProductFromTemplate(
        [FromBody] CreateProductFromTemplateDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var sellerId = User.IsInRole("Admin") ? dto.SellerId : userId;
        var command = new CreateProductFromTemplateCommand(
            dto.TemplateId,
            dto.ProductName,
            dto.Description ?? string.Empty,
            dto.SKU ?? string.Empty,
            dto.Price ?? 0,
            dto.DiscountPrice,
            dto.StockQuantity ?? 0,
            sellerId,
            dto.StoreId,
            dto.ImageUrl,
            dto.ImageUrls?.ToList());
        var product = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTemplate), new { id = product.Id }, product);
    }
}
