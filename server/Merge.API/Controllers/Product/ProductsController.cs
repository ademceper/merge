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
using Merge.Application.DTOs.Product;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.API.Extensions;

namespace Merge.API.Controllers.Product;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{
    private readonly PaginationSettings _paginationSettings = paginationSettings.Value;

    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url, 
            "GetAllProducts", 
            page, 
            pageSize, 
            products.TotalPages,
            null,
            version);
        return Ok(new { products, _links = paginationLinks });
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        var product = await mediator.Send(query, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        // ✅ HIGH-API-003: ETag/Cache-Control Headers - HTTP caching support
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

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductLinks(Url, id, version);
        return Ok(new { product, _links = links });
    }

    [HttpGet("category/{categoryId}")]
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url, 
            "GetByCategory", 
            page, 
            pageSize, 
            products.TotalPages,
            new { categoryId },
            version);
        return Ok(new { products, _links = paginationLinks });
    }

    [HttpGet("search")]
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url, 
            "Search", 
            page, 
            pageSize, 
            products.TotalPages,
            new { q },
            version);
        return Ok(new { products, _links = paginationLinks });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var sellerId = User.IsInRole("Admin") ? command.SellerId : userId;
        var updatedCommand = command with { SellerId = sellerId };
        var product = await mediator.Send(updatedCommand, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductLinks(Url, product.Id, version);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { product, _links = links });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getQuery = new GetProductByIdQuery(id);
        var existingProduct = await mediator.Send(getQuery, cancellationToken);
        if (existingProduct == null)
        {
            return NotFound();
        }
        if (existingProduct.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var updatedCommand = command with { Id = id, SellerId = existingProduct.SellerId, PerformedBy = userId };
        var product = await mediator.Send(updatedCommand, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductLinks(Url, product.Id, version);
        return Ok(new { product, _links = links });
    }

    /// <summary>
    /// Ürünü kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> Patch(
        Guid id,
        [FromBody] PatchProductDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getQuery = new GetProductByIdQuery(id);
        var existingProduct = await mediator.Send(getQuery, cancellationToken);
        if (existingProduct == null)
        {
            return NotFound();
        }
        if (existingProduct.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new PatchProductCommand(id, patchDto, userId);
        var product = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductLinks(Url, product.Id, version);
        return Ok(new { product, _links = links });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getQuery = new GetProductByIdQuery(id);
        var existingProduct = await mediator.Send(getQuery, cancellationToken);
        if (existingProduct == null)
        {
            return NotFound();
        }
        if (existingProduct.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new DeleteProductCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
