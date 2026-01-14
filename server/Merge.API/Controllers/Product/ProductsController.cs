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
using Merge.Application.Product.Commands.DeleteProduct;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Product;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IOptions<PaginationSettings> _paginationSettings;

    public ProductsController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings;
    }

    /// <summary>
    /// Gets all active products with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: configured limit)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns the paginated list of products</response>
    /// <response code="429">Too many requests</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _paginationSettings.Value.MaxPageSize) pageSize = _paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAllProductsQuery(page, pageSize);
        var products = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
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

    /// <summary>
    /// Gets a product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    /// <response code="429">Too many requests</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetProductByIdQuery(id);
        var product = await _mediator.Send(query, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductLinks(Url, id, version);
        
        return Ok(new { product, _links = links });
    }

    /// <summary>
    /// Gets products by category ID with pagination
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: configured limit)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products in the category</returns>
    /// <response code="200">Returns the paginated list of products</response>
    /// <response code="429">Too many requests</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("category/{categoryId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetByCategory(
        Guid categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _paginationSettings.Value.MaxPageSize) pageSize = _paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetProductsByCategoryQuery(categoryId, page, pageSize);
        var products = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
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

    /// <summary>
    /// Searches products by name, description, or brand with pagination
    /// </summary>
    /// <param name="q">Search query</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: configured limit)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of matching products</returns>
    /// <response code="200">Returns the paginated list of matching products</response>
    /// <response code="400">Invalid search query</response>
    /// <response code="429">Too many requests</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("search")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _paginationSettings.Value.MaxPageSize) pageSize = _paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new SearchProductsQuery(q, page, pageSize);
        var products = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
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

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="command">Product creation command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="422">Business rule violation</response>
    /// <response code="429">Too many requests</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (Spam koruması)
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ SECURITY: Seller kendi SellerId'sini set etmeli
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // Admin değilse, SellerId'yi zorunlu olarak kendi userId'si yap
        var sellerId = User.IsInRole("Admin") ? command.SellerId : userId;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var updatedCommand = command with { SellerId = sellerId };
        var product = await _mediator.Send(updatedCommand, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductLinks(Url, product.Id, version);
        
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { product, _links = links });
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Product update command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized or IDOR violation</response>
    /// <response code="404">Product not found</response>
    /// <response code="422">Business rule violation or concurrency conflict</response>
    /// <response code="429">Too many requests</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerini güncelleyebilir
        var getQuery = new GetProductByIdQuery(id);
        var existingProduct = await _mediator.Send(getQuery, cancellationToken);
        if (existingProduct == null)
        {
            return NotFound();
        }

        if (existingProduct.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // Preserve SellerId from existing product (IDOR protection)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (UpdateProductCommandHandler)
        var updatedCommand = command with { Id = id, SellerId = existingProduct.SellerId, PerformedBy = userId };
        var product = await _mediator.Send(updatedCommand, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductLinks(Url, product.Id, version);
        
        return Ok(new { product, _links = links });
    }

    /// <summary>
    /// Deletes a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized or IDOR violation</response>
    /// <response code="404">Product not found</response>
    /// <response code="422">Business rule violation (product has orders) or concurrency conflict</response>
    /// <response code="429">Too many requests</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerini silebilir
        var getQuery = new GetProductByIdQuery(id);
        var existingProduct = await _mediator.Send(getQuery, cancellationToken);
        if (existingProduct == null)
        {
            return NotFound();
        }

        if (existingProduct.SellerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (DeleteProductCommandHandler)
        var command = new DeleteProductCommand(id, userId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

