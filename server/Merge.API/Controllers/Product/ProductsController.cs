using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Product.Queries.GetAllProducts;
using Merge.Application.Product.Queries.GetProductById;
using Merge.Application.Product.Queries.GetProductsByCategory;
using Merge.Application.Product.Queries.SearchProducts;
using Merge.Application.Product.Commands.CreateProduct;
using Merge.Application.Product.Commands.UpdateProduct;
using Merge.Application.Product.Commands.PatchProduct;
using Merge.Application.Product.Commands.DeleteProduct;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.API.Extensions;
using Merge.Application.Exceptions;

namespace Merge.API.Controllers.Product;

/// <summary>
/// Product API endpoints.
/// Tüm CRUD ve özel operasyonları içerir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products")]
[Tags("Products")]
public class ProductsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{
    private readonly PaginationSettings _paginationSettings = paginationSettings.Value;

    /// <summary>
    /// Get all products with pagination.
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns paginated products</response>
    /// <response code="400">Invalid parameters</response>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;
        var query = new GetAllProductsQuery(page, pageSize);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Get a specific product by ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        var product = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("Product", id);

        // Generate ETag from product data (in real scenario, use RowVersion or LastModified)
        var productJson = System.Text.Json.JsonSerializer.Serialize(product);
        Response.SetETag(productJson);
        Response.SetCacheControl(maxAgeSeconds: 300, isPublic: true); // Cache for 5 minutes

        // Check if client has cached version (304 Not Modified)
        var etag = Response.Headers["ETag"].FirstOrDefault();
        if (!string.IsNullOrEmpty(etag) && Request.IsNotModified(etag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        return Ok(product);
    }

    /// <summary>
    /// Get products by category.
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetByCategory(
        Guid categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;
        var query = new GetProductsByCategoryQuery(categoryId, page, pageSize);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Search products.
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;
        var query = new SearchProductsQuery(q, page, pageSize);
        var products = await mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="command">Product creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not authorized</response>
    /// <response code="409">SKU already exists</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var sellerId = User.IsInRole("Admin") ? command.SellerId : userId;
        var updatedCommand = command with { SellerId = sellerId };
        var product = await mediator.Send(updatedCommand, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Update entire product (full replacement).
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Updated product data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Validation errors or ID mismatch</response>
    /// <response code="404">Product not found</response>
    /// <response code="409">Concurrency conflict</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getQuery = new GetProductByIdQuery(id);
        var existingProduct = await mediator.Send(getQuery, cancellationToken);
        if (existingProduct is null)
        {
            return Problem("Product not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (existingProduct.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var updatedCommand = command with { Id = id, SellerId = existingProduct.SellerId, PerformedBy = userId };
        var product = await mediator.Send(updatedCommand, cancellationToken);
        return Ok(product);
    }

    /// <summary>
    /// Partially update a product.
    /// Only provided fields will be updated.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="patchDto">Fields to update (null = unchanged)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product patched successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="404">Product not found</response>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> Patch(
        Guid id,
        [FromBody] PatchProductDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getQuery = new GetProductByIdQuery(id);
        var existingProduct = await mediator.Send(getQuery, cancellationToken);
        if (existingProduct is null)
        {
            return Problem("Product not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (existingProduct.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new PatchProductCommand(id, patchDto, userId);
        var product = await mediator.Send(command, cancellationToken);
        return Ok(product);
    }

    /// <summary>
    /// Delete a product (soft delete).
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getQuery = new GetProductByIdQuery(id);
        var existingProduct = await mediator.Send(getQuery, cancellationToken);
        if (existingProduct is null)
        {
            return Problem("Product not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (existingProduct.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new DeleteProductCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Product not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }
}
