---
paths:
  - "Merge.API/**/*.cs"
  - "**/Controllers/**/*.cs"
  - "**/Endpoints/**/*.cs"
  - "**/Middleware/**/*.cs"
  - "**/Filters/**/*.cs"
  - "**/Extensions/**/*.cs"
---

# REST API DESIGN KURALLARI (ULTRA KAPSAMLI)

> Bu dosya, Merge E-Commerce Backend projesinde REST API tasarımı için
> kapsamlı kurallar ve en iyi uygulamaları içerir.

---

## İÇİNDEKİLER

1. [Controller Yapısı](#1-controller-yapısı)
2. [HTTP Methods ve Status Codes](#2-http-methods-ve-status-codes)
3. [Route Conventions](#3-route-conventions)
4. [Request/Response Formatları](#4-requestresponse-formatları)
5. [Pagination](#5-pagination)
6. [Filtering ve Sorting](#6-filtering-ve-sorting)
7. [Error Handling (RFC 7807)](#7-error-handling-rfc-7807)
8. [Validation](#8-validation)
9. [Versioning](#9-versioning)
10. [Authentication & Authorization](#10-authentication--authorization)
11. [Rate Limiting](#11-rate-limiting)
12. [Caching](#12-caching)
13. [HATEOAS](#13-hateoas)
14. [Idempotency](#14-idempotency)
15. [API Documentation](#15-api-documentation)
16. [Middleware Pipeline](#16-middleware-pipeline)
17. [Minimal APIs vs Controllers](#17-minimal-apis-vs-controllers)
18. [Anti-Patterns](#18-anti-patterns)

---

## 1. CONTROLLER YAPISI

### 1.1 Base Controller

```csharp
/// <summary>
/// Tüm API controller'ları için base class.
/// Ortak davranışları ve helper method'ları içerir.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// MediatR instance - derived class'larda kullanılır.
    /// </summary>
    protected IMediator Mediator =>
        HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Current user ID.
    /// </summary>
    protected Guid? CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value is { } id
            ? Guid.Parse(id)
            : null;

    /// <summary>
    /// Current user roles.
    /// </summary>
    protected IEnumerable<string> CurrentUserRoles =>
        User.FindAll(ClaimTypes.Role).Select(c => c.Value);

    /// <summary>
    /// Created response with location header.
    /// </summary>
    protected CreatedAtActionResult CreatedAtAction<T>(
        string actionName,
        object routeValues,
        T value)
    {
        return base.CreatedAtAction(actionName, routeValues, value);
    }

    /// <summary>
    /// Problem details response.
    /// </summary>
    protected ObjectResult Problem(
        string detail,
        string title,
        int statusCode,
        string? type = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type ?? GetProblemType(statusCode),
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        return StatusCode(statusCode, problemDetails);
    }

    private static string GetProblemType(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
        429 => "https://tools.ietf.org/html/rfc6585#section-4",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
```

### 1.2 Domain Controller Örneği (Products)

```csharp
/// <summary>
/// Product API endpoints.
/// Tüm CRUD ve özel operasyonları içerir.
/// </summary>
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Tags("Products")]
public class ProductsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings,
    ILogger<ProductsController> logger) : BaseController
{
    private readonly PaginationSettings _pagination = paginationSettings.Value;

    #region GET Endpoints

    /// <summary>
    /// Get all products with pagination, filtering, and sorting.
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (max 100)</param>
    /// <param name="categoryId">Filter by category</param>
    /// <param name="brandId">Filter by brand</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="search">Search term for name/description</param>
    /// <param name="sortBy">Sort field (name, price, createdAt, rating)</param>
    /// <param name="sortOrder">Sort direction (asc, desc)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns paginated products</response>
    /// <response code="400">Invalid parameters</response>
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 60, VaryByQueryKeys = ["*"])]
    [ProducesResponseType(typeof(PagedResult<ProductListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc",
        CancellationToken ct = default)
    {
        // Validate and cap page size
        pageSize = Math.Min(pageSize, _pagination.MaxPageSize);

        var query = new GetProductsQuery(
            Page: page,
            PageSize: pageSize,
            CategoryId: categoryId,
            BrandId: brandId,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            IsActive: isActive,
            Search: search,
            SortBy: sortBy,
            SortOrder: sortOrder);

        var result = await mediator.Send(query, ct);

        // Add caching headers
        Response.AddETag(result);
        Response.AddPaginationHeaders(result);

        logger.LogInformation(
            "Retrieved {Count} products for page {Page}",
            result.Items.Count, page);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific product by ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300)]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetById(
        Guid id,
        CancellationToken ct = default)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query, ct);

        if (result is null)
        {
            return NotFound();
        }

        Response.AddETag(result);
        return Ok(result);
    }

    /// <summary>
    /// Get a product by SKU.
    /// </summary>
    [HttpGet("by-sku/{sku}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetBySku(
        string sku,
        CancellationToken ct = default)
    {
        var query = new GetProductBySkuQuery(sku);
        var result = await mediator.Send(query, ct);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Get featured products.
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    [ResponseCache(Duration = 600)]
    [ProducesResponseType(typeof(List<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductListDto>>> GetFeatured(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        count = Math.Min(count, 50);

        var query = new GetFeaturedProductsQuery(count);
        var result = await mediator.Send(query, ct);

        return Ok(result);
    }

    /// <summary>
    /// Get products by category.
    /// </summary>
    [HttpGet("by-category/{categoryId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductListDto>>> GetByCategory(
        Guid categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetProductsByCategoryQuery(categoryId, page, pageSize);
        var result = await mediator.Send(query, ct);

        Response.AddPaginationHeaders(result);
        return Ok(result);
    }

    /// <summary>
    /// Search products.
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductSearchResultDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Ok(new List<ProductSearchResultDto>());
        }

        var query = new SearchProductsQuery(q, limit);
        var result = await mediator.Send(query, ct);

        return Ok(result);
    }

    #endregion

    #region POST Endpoints

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="command">Product creation data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not authorized</response>
    /// <response code="409">SKU already exists</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(command, ct);

        logger.LogInformation(
            "Product {ProductId} created by user {UserId}",
            result.Id, CurrentUserId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    /// <summary>
    /// Bulk create products (max 100).
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BulkCreateResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkCreateResult>> BulkCreate(
        [FromBody] BulkCreateProductsCommand command,
        CancellationToken ct = default)
    {
        if (command.Products.Count > 100)
        {
            return BadRequest("Maximum 100 products can be created at once.");
        }

        var result = await mediator.Send(command, ct);

        return CreatedAtAction(null, result);
    }

    #endregion

    #region PUT Endpoints

    /// <summary>
    /// Update entire product (full replacement).
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Updated product data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Validation errors or ID mismatch</response>
    /// <response code="404">Product not found</response>
    /// <response code="409">Concurrency conflict</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken ct = default)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL does not match ID in body.");
        }

        var result = await mediator.Send(command, ct);

        logger.LogInformation(
            "Product {ProductId} updated by user {UserId}",
            id, CurrentUserId);

        return Ok(result);
    }

    #endregion

    #region PATCH Endpoints

    /// <summary>
    /// Partially update a product.
    /// Only provided fields will be updated.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Fields to update (null = unchanged)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product patched successfully</response>
    /// <response code="400">Validation errors</response>
    /// <response code="404">Product not found</response>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Patch(
        Guid id,
        [FromBody] PatchProductCommand command,
        CancellationToken ct = default)
    {
        // Set ID from route
        command = command with { Id = id };

        var result = await mediator.Send(command, ct);

        return Ok(result);
    }

    /// <summary>
    /// Update product price only.
    /// </summary>
    [HttpPatch("{id:guid}/price")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdatePrice(
        Guid id,
        [FromBody] UpdateProductPriceCommand command,
        CancellationToken ct = default)
    {
        command = command with { Id = id };
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Update product stock.
    /// </summary>
    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> UpdateStock(
        Guid id,
        [FromBody] UpdateProductStockCommand command,
        CancellationToken ct = default)
    {
        command = command with { Id = id };
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    #endregion

    #region DELETE Endpoints

    /// <summary>
    /// Delete a product (soft delete).
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct = default)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);

        logger.LogInformation(
            "Product {ProductId} deleted by user {UserId}",
            id, CurrentUserId);

        return NoContent();
    }

    /// <summary>
    /// Bulk delete products.
    /// </summary>
    [HttpDelete("bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BulkDeleteResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BulkDeleteResult>> BulkDelete(
        [FromBody] BulkDeleteProductsCommand command,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    #endregion

    #region Action Endpoints

    /// <summary>
    /// Activate a product.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Activate(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ActivateProductCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Deactivate a product.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Deactivate(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new DeactivateProductCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Mark product as featured.
    /// </summary>
    [HttpPost("{id:guid}/feature")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> Feature(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new FeatureProductCommand(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Remove product from featured.
    /// </summary>
    [HttpPost("{id:guid}/unfeature")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> Unfeature(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new UnfeatureProductCommand(id), ct);
        return Ok(result);
    }

    #endregion

    #region Nested Resources

    /// <summary>
    /// Get product reviews.
    /// </summary>
    [HttpGet("{id:guid}/reviews")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetReviews(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetProductReviewsQuery(id, page, pageSize);
        var result = await mediator.Send(query, ct);

        Response.AddPaginationHeaders(result);
        return Ok(result);
    }

    /// <summary>
    /// Add a review to product.
    /// </summary>
    [HttpPost("{id:guid}/reviews")]
    [Authorize]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> AddReview(
        Guid id,
        [FromBody] AddProductReviewCommand command,
        CancellationToken ct = default)
    {
        command = command with { ProductId = id };
        var result = await mediator.Send(command, ct);

        return CreatedAtAction(
            nameof(GetReviews),
            new { id },
            result);
    }

    /// <summary>
    /// Get product images.
    /// </summary>
    [HttpGet("{id:guid}/images")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductImageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductImageDto>>> GetImages(
        Guid id,
        CancellationToken ct = default)
    {
        var query = new GetProductImagesQuery(id);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Upload product image.
    /// </summary>
    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = "Admin,Seller")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProductImageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<ActionResult<ProductImageDto>> UploadImage(
        Guid id,
        [FromForm] IFormFile file,
        [FromForm] bool isPrimary = false,
        CancellationToken ct = default)
    {
        var command = new UploadProductImageCommand(id, file, isPrimary);
        var result = await mediator.Send(command, ct);

        return CreatedAtAction(nameof(GetImages), new { id }, result);
    }

    /// <summary>
    /// Delete product image.
    /// </summary>
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteImage(
        Guid id,
        Guid imageId,
        CancellationToken ct = default)
    {
        await mediator.Send(new DeleteProductImageCommand(id, imageId), ct);
        return NoContent();
    }

    /// <summary>
    /// Get product variants.
    /// </summary>
    [HttpGet("{id:guid}/variants")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductVariantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductVariantDto>>> GetVariants(
        Guid id,
        CancellationToken ct = default)
    {
        var query = new GetProductVariantsQuery(id);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    #endregion
}
```

---

## 2. HTTP METHODS VE STATUS CODES

### 2.1 HTTP Methods

| Method | Kullanım | Idempotent | Safe |
|--------|----------|------------|------|
| `GET` | Resource okuma | ✅ | ✅ |
| `POST` | Resource oluşturma, action tetikleme | ❌ | ❌ |
| `PUT` | Resource tamamen güncelleme | ✅ | ❌ |
| `PATCH` | Resource kısmi güncelleme | ❌* | ❌ |
| `DELETE` | Resource silme | ✅ | ❌ |
| `HEAD` | GET gibi ama body'siz (metadata) | ✅ | ✅ |
| `OPTIONS` | Desteklenen method'ları sorgulama | ✅ | ✅ |

*PATCH idempotent olarak implement edilebilir

### 2.2 Status Codes

```csharp
/// <summary>
/// HTTP Status Code kullanım kılavuzu.
/// </summary>
public static class StatusCodeGuide
{
    // ============================================================
    // 2xx SUCCESS
    // ============================================================

    /// <summary>
    /// 200 OK - Başarılı GET, PUT, PATCH, DELETE (body ile)
    /// </summary>
    public static IActionResult Ok(object? value) => new OkObjectResult(value);

    /// <summary>
    /// 201 Created - POST başarılı, yeni resource oluşturuldu
    /// Location header ZORUNLU
    /// </summary>
    public static IActionResult Created(string location, object value) =>
        new CreatedResult(location, value);

    /// <summary>
    /// 202 Accepted - Async işlem başlatıldı, henüz tamamlanmadı
    /// </summary>
    public static IActionResult Accepted(string? location = null) =>
        new AcceptedResult(location, null);

    /// <summary>
    /// 204 No Content - DELETE başarılı, body yok
    /// </summary>
    public static IActionResult NoContent() => new NoContentResult();

    // ============================================================
    // 4xx CLIENT ERRORS
    // ============================================================

    /// <summary>
    /// 400 Bad Request - Geçersiz request (syntax, format)
    /// </summary>
    public static IActionResult BadRequest(object? error) =>
        new BadRequestObjectResult(error);

    /// <summary>
    /// 401 Unauthorized - Authentication gerekli/geçersiz
    /// WWW-Authenticate header ZORUNLU
    /// </summary>
    public static IActionResult Unauthorized() => new UnauthorizedResult();

    /// <summary>
    /// 403 Forbidden - Authenticated ama yetkisiz
    /// </summary>
    public static IActionResult Forbidden() => new ForbidResult();

    /// <summary>
    /// 404 Not Found - Resource bulunamadı
    /// </summary>
    public static IActionResult NotFound(object? value = null) =>
        new NotFoundObjectResult(value);

    /// <summary>
    /// 409 Conflict - Concurrency conflict, duplicate
    /// </summary>
    public static IActionResult Conflict(object? error) =>
        new ConflictObjectResult(error);

    /// <summary>
    /// 422 Unprocessable Entity - Business rule violation
    /// Syntax doğru ama semantik hata
    /// </summary>
    public static IActionResult UnprocessableEntity(object? error) =>
        new UnprocessableEntityObjectResult(error);

    /// <summary>
    /// 429 Too Many Requests - Rate limit aşıldı
    /// Retry-After header ÖNERİLİR
    /// </summary>
    public static IActionResult TooManyRequests(int retryAfterSeconds)
    {
        var result = new StatusCodeResult(429);
        // Retry-After header middleware'de eklenir
        return result;
    }

    // ============================================================
    // 5xx SERVER ERRORS
    // ============================================================

    /// <summary>
    /// 500 Internal Server Error - Beklenmeyen sunucu hatası
    /// Detay loglanmalı, kullanıcıya gösterilmemeli
    /// </summary>
    public static IActionResult InternalServerError() =>
        new StatusCodeResult(500);

    /// <summary>
    /// 502 Bad Gateway - Upstream service hatası
    /// </summary>
    public static IActionResult BadGateway() =>
        new StatusCodeResult(502);

    /// <summary>
    /// 503 Service Unavailable - Geçici olarak kullanılamıyor
    /// Retry-After header ÖNERİLİR
    /// </summary>
    public static IActionResult ServiceUnavailable() =>
        new StatusCodeResult(503);

    /// <summary>
    /// 504 Gateway Timeout - Upstream service timeout
    /// </summary>
    public static IActionResult GatewayTimeout() =>
        new StatusCodeResult(504);
}
```

### 2.3 Status Code Decision Tree

```
Request geçerli mi?
├── Hayır → 400 Bad Request
└── Evet → Authenticated mi?
    ├── Hayır (gerekli ise) → 401 Unauthorized
    └── Evet → Authorized mı?
        ├── Hayır → 403 Forbidden
        └── Evet → Resource var mı?
            ├── Hayır → 404 Not Found
            └── Evet → Business rule sağlanıyor mu?
                ├── Hayır → 422 Unprocessable Entity
                └── Evet → Concurrency conflict var mı?
                    ├── Evet → 409 Conflict
                    └── Hayır → İşlem başarılı
                        ├── GET → 200 OK
                        ├── POST → 201 Created
                        ├── PUT/PATCH → 200 OK
                        └── DELETE → 204 No Content
```

---

## 3. ROUTE CONVENTIONS

### 3.1 URL Yapısı

```
https://api.merge.com/api/v{version}/{resource}[/{id}][/{sub-resource}][/{action}]

Örnekler:
GET    /api/v1/products                    # Liste
GET    /api/v1/products/{id}               # Tekil
POST   /api/v1/products                    # Oluştur
PUT    /api/v1/products/{id}               # Tam güncelle
PATCH  /api/v1/products/{id}               # Kısmi güncelle
DELETE /api/v1/products/{id}               # Sil

# Alt kaynaklar (nested resources)
GET    /api/v1/products/{id}/reviews       # Ürünün yorumları
POST   /api/v1/products/{id}/reviews       # Yorum ekle
GET    /api/v1/products/{id}/images        # Ürün resimleri
POST   /api/v1/products/{id}/images        # Resim yükle

# Özel sorgular
GET    /api/v1/products/featured           # Öne çıkan ürünler
GET    /api/v1/products/by-category/{categoryId}
GET    /api/v1/products/by-sku/{sku}
GET    /api/v1/products/search?q=laptop

# Aksiyonlar (CRUD'a uymayan işlemler)
POST   /api/v1/products/{id}/activate      # Ürünü aktifleştir
POST   /api/v1/products/{id}/deactivate    # Ürünü deaktif et
POST   /api/v1/orders/{id}/cancel          # Siparişi iptal et
POST   /api/v1/orders/{id}/ship            # Siparişi kargola
POST   /api/v1/carts/{id}/checkout         # Sepeti öde
```

### 3.2 Route Naming Rules

```csharp
// ✅ DOĞRU - Çoğul, küçük harf, tire ile ayır
/api/v1/products
/api/v1/product-categories
/api/v1/order-items
/api/v1/user-profiles

// ❌ YANLIŞ
/api/v1/Product        // Büyük harf
/api/v1/product        // Tekil
/api/v1/getProducts    // Fiil kullanımı
/api/v1/product_items  // Underscore
/api/v1/productItems   // CamelCase

// ✅ DOĞRU - ID tipleri
/api/v1/products/{id:guid}       // GUID
/api/v1/products/{id:int}        // Integer
/api/v1/products/by-sku/{sku}    // String (özel identifier)

// ✅ DOĞRU - Query parametreleri
/api/v1/products?page=1&pageSize=20
/api/v1/products?categoryId=xxx&minPrice=100
/api/v1/products?sortBy=price&sortOrder=desc
/api/v1/products?search=laptop

// ❌ YANLIŞ - Body'de olması gerekenler query'de
/api/v1/products?name=xxx&price=100&description=...  // POST body olmalı
```

### 3.3 Route Attribute Örnekleri

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : BaseController
{
    // GET /api/v1/products
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll() { }

    // GET /api/v1/products/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id) { }

    // GET /api/v1/products/by-sku/{sku}
    [HttpGet("by-sku/{sku}")]
    public async Task<ActionResult<ProductDto>> GetBySku(string sku) { }

    // GET /api/v1/products/{id}/reviews
    [HttpGet("{id:guid}/reviews")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviews(Guid id) { }

    // POST /api/v1/products/{id}/activate
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ProductDto>> Activate(Guid id) { }

    // Custom route (override [controller])
    // GET /api/v1/catalog/products
    [HttpGet("/api/v{version:apiVersion}/catalog/products")]
    public async Task<ActionResult<List<ProductDto>>> GetCatalog() { }
}
```

---

## 4. REQUEST/RESPONSE FORMATLARI

### 4.1 Request Format

```csharp
/// <summary>
/// Request DTO örnekleri.
/// </summary>

// POST/PUT için - tüm alanlar zorunlu
public record CreateProductRequest(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    string Currency,
    int StockQuantity,
    Guid CategoryId,
    Guid? BrandId,
    List<string>? Tags);

// PATCH için - nullable alanlar (null = değişmeyecek)
public record PatchProductRequest(
    string? Name,
    string? Description,
    decimal? Price,
    int? StockQuantity,
    bool? IsActive);

// Validasyonlu örnek
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .Matches(@"^[A-Z0-9\-]{3,50}$").WithMessage("Invalid SKU format");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be positive");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters")
            .Must(BeValidCurrency).WithMessage("Invalid currency code");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");
    }

    private bool BeValidCurrency(string currency) =>
        new[] { "TRY", "USD", "EUR" }.Contains(currency);
}
```

### 4.2 Response Format

```csharp
/// <summary>
/// Response DTO örnekleri.
/// </summary>

// Liste endpoint'i için
public record ProductListDto(
    Guid Id,
    string Name,
    string Slug,
    decimal Price,
    string Currency,
    string? ImageUrl,
    string CategoryName,
    decimal AverageRating,
    int ReviewCount,
    bool IsInStock);

// Detay endpoint'i için
public record ProductDetailDto(
    Guid Id,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    decimal? CompareAtPrice,
    string Currency,
    int StockQuantity,
    bool IsActive,
    bool IsFeatured,
    CategoryDto Category,
    BrandDto? Brand,
    List<ProductImageDto> Images,
    List<ProductVariantDto> Variants,
    List<string> Tags,
    decimal AverageRating,
    int ReviewCount,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

// Nested DTO
public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId);

// Sayfalı response
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

// Bulk operation response
public record BulkCreateResult(
    int SuccessCount,
    int FailureCount,
    List<Guid> CreatedIds,
    List<BulkError> Errors);

public record BulkError(
    int Index,
    string Error);
```

### 4.3 Response Envelope (Opsiyonel)

```csharp
/// <summary>
/// Standart response wrapper.
/// Tüm response'ları aynı formatta döner.
/// </summary>
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, object>? Meta { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(string message) => new()
    {
        Success = false,
        Message = message
    };

    public static ApiResponse<T> WithMeta(T data, Dictionary<string, object> meta) => new()
    {
        Success = true,
        Data = data,
        Meta = meta
    };
}

// Kullanım
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
{
    var product = await _mediator.Send(new GetProductByIdQuery(id));

    if (product is null)
        return NotFound(ApiResponse<ProductDto>.Fail("Product not found"));

    return Ok(ApiResponse<ProductDto>.Ok(product));
}

// Response örneği
{
  "success": true,
  "data": {
    "id": "xxx",
    "name": "Product Name",
    ...
  },
  "message": null,
  "meta": {
    "cacheHit": true,
    "responseTime": 45
  }
}
```

---

## 5. PAGINATION

### 5.1 Paginated Response

```csharp
/// <summary>
/// Sayfalama response modeli.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }
}

// JSON Response
{
  "items": [...],
  "totalCount": 150,
  "page": 2,
  "pageSize": 20,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": true
}
```

### 5.2 Pagination Headers

```csharp
/// <summary>
/// Pagination bilgilerini header'lara ekler.
/// </summary>
public static class PaginationHeaderExtensions
{
    public static void AddPaginationHeaders<T>(
        this HttpResponse response,
        PagedResult<T> result)
    {
        response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        response.Headers.Append("X-Page", result.Page.ToString());
        response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());
        response.Headers.Append("X-Has-Next-Page", result.HasNextPage.ToString());
        response.Headers.Append("X-Has-Previous-Page", result.HasPreviousPage.ToString());

        // Link header (RFC 5988)
        var links = new List<string>();
        var baseUrl = $"{response.HttpContext.Request.Scheme}://{response.HttpContext.Request.Host}{response.HttpContext.Request.Path}";

        if (result.HasPreviousPage)
        {
            links.Add($"<{baseUrl}?page={result.Page - 1}&pageSize={result.PageSize}>; rel=\"prev\"");
        }

        if (result.HasNextPage)
        {
            links.Add($"<{baseUrl}?page={result.Page + 1}&pageSize={result.PageSize}>; rel=\"next\"");
        }

        links.Add($"<{baseUrl}?page=1&pageSize={result.PageSize}>; rel=\"first\"");
        links.Add($"<{baseUrl}?page={result.TotalPages}&pageSize={result.PageSize}>; rel=\"last\"");

        response.Headers.Append("Link", string.Join(", ", links));
    }
}

// Response Headers
X-Total-Count: 150
X-Page: 2
X-Page-Size: 20
X-Total-Pages: 8
X-Has-Next-Page: true
X-Has-Previous-Page: true
Link: <https://api.merge.com/api/v1/products?page=1&pageSize=20>; rel="prev",
      <https://api.merge.com/api/v1/products?page=3&pageSize=20>; rel="next",
      <https://api.merge.com/api/v1/products?page=1&pageSize=20>; rel="first",
      <https://api.merge.com/api/v1/products?page=8&pageSize=20>; rel="last"
```

### 5.3 Cursor-Based Pagination

```csharp
/// <summary>
/// Cursor-based pagination (büyük veri setleri için).
/// Offset-based'den daha performanslı.
/// </summary>
public class CursorPagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public string? NextCursor { get; }
    public string? PreviousCursor { get; }
    public bool HasMore { get; }

    public CursorPagedResult(
        IReadOnlyList<T> items,
        string? nextCursor,
        string? previousCursor,
        bool hasMore)
    {
        Items = items;
        NextCursor = nextCursor;
        PreviousCursor = previousCursor;
        HasMore = hasMore;
    }
}

// Cursor encoding
public static class CursorEncoder
{
    public static string Encode(Guid id, DateTime createdAt) =>
        Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{id}|{createdAt:O}"));

    public static (Guid Id, DateTime CreatedAt) Decode(string cursor)
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
        var parts = decoded.Split('|');
        return (Guid.Parse(parts[0]), DateTime.Parse(parts[1]));
    }
}

// Query
[HttpGet]
public async Task<ActionResult<CursorPagedResult<ProductDto>>> GetAll(
    [FromQuery] string? cursor = null,
    [FromQuery] int limit = 20)
{
    var query = new GetProductsCursorQuery(cursor, limit);
    var result = await _mediator.Send(query);
    return Ok(result);
}

// Response
{
  "items": [...],
  "nextCursor": "YWJjMTIzfDIwMjQtMDEtMTU=",
  "previousCursor": null,
  "hasMore": true
}
```

---

## 6. FILTERING VE SORTING

### 6.1 Filter Parameters

```csharp
/// <summary>
/// Filtreleme parametreleri.
/// </summary>
public record ProductFilterQuery(
    // Pagination
    int Page = 1,
    int PageSize = 20,

    // Filters
    Guid? CategoryId = null,
    Guid? BrandId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsActive = null,
    bool? IsFeatured = null,
    bool? InStock = null,
    List<string>? Tags = null,

    // Search
    string? Search = null,

    // Sorting
    string SortBy = "createdAt",
    string SortOrder = "desc",

    // Date filters
    DateTime? CreatedAfter = null,
    DateTime? CreatedBefore = null
);

// Controller'da kullanım
[HttpGet]
public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
    [FromQuery] ProductFilterQuery filter,
    CancellationToken ct)
{
    var result = await _mediator.Send(
        new GetProductsQuery(filter), ct);
    return Ok(result);
}

// Request örneği
GET /api/v1/products?categoryId=xxx&minPrice=100&maxPrice=500&isActive=true&sortBy=price&sortOrder=asc&page=1&pageSize=20
```

### 6.2 Dynamic Sorting

```csharp
/// <summary>
/// Dinamik sıralama implementasyonu.
/// </summary>
public static class QueryableExtensions
{
    private static readonly Dictionary<string, Expression<Func<Product, object>>> SortExpressions = new()
    {
        ["name"] = p => p.Name,
        ["price"] = p => p.Price.Amount,
        ["createdAt"] = p => p.CreatedAt,
        ["rating"] = p => p.AverageRating,
        ["sold"] = p => p.SoldCount,
        ["stock"] = p => p.StockQuantity
    };

    public static IQueryable<Product> ApplySort(
        this IQueryable<Product> query,
        string sortBy,
        string sortOrder)
    {
        if (!SortExpressions.TryGetValue(sortBy.ToLowerInvariant(), out var expression))
        {
            expression = SortExpressions["createdAt"]; // Default
        }

        return sortOrder.ToLowerInvariant() == "asc"
            ? query.OrderBy(expression)
            : query.OrderByDescending(expression);
    }
}

// Handler'da kullanım
public async Task<PagedResult<ProductDto>> Handle(
    GetProductsQuery request,
    CancellationToken ct)
{
    var query = _context.Products
        .AsNoTracking()
        .Where(p => p.IsActive);

    // Apply filters
    if (request.Filter.CategoryId.HasValue)
        query = query.Where(p => p.CategoryId == request.Filter.CategoryId);

    if (request.Filter.MinPrice.HasValue)
        query = query.Where(p => p.Price.Amount >= request.Filter.MinPrice);

    if (request.Filter.MaxPrice.HasValue)
        query = query.Where(p => p.Price.Amount <= request.Filter.MaxPrice);

    if (!string.IsNullOrWhiteSpace(request.Filter.Search))
        query = query.Where(p =>
            EF.Functions.ILike(p.Name, $"%{request.Filter.Search}%"));

    // Count before pagination
    var totalCount = await query.CountAsync(ct);

    // Apply sorting and pagination
    var items = await query
        .ApplySort(request.Filter.SortBy, request.Filter.SortOrder)
        .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
        .Take(request.Filter.PageSize)
        .Select(p => new ProductDto(...))
        .ToListAsync(ct);

    return new PagedResult<ProductDto>(
        items, totalCount, request.Filter.Page, request.Filter.PageSize);
}
```

### 6.3 Filter Validation

```csharp
/// <summary>
/// Filter parametrelerini validate eden middleware.
/// </summary>
public class FilterValidationMiddleware(RequestDelegate next)
{
    private readonly string[] _allowedSortFields =
        ["name", "price", "createdAt", "rating", "sold"];
    private readonly string[] _allowedSortOrders = ["asc", "desc"];

    public async Task InvokeAsync(HttpContext context)
    {
        var query = context.Request.Query;

        // Validate sortBy
        if (query.TryGetValue("sortBy", out var sortBy) &&
            !_allowedSortFields.Contains(sortBy.ToString().ToLowerInvariant()))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 400,
                Title = "Invalid Sort Field",
                Detail = $"sortBy must be one of: {string.Join(", ", _allowedSortFields)}"
            });
            return;
        }

        // Validate sortOrder
        if (query.TryGetValue("sortOrder", out var sortOrder) &&
            !_allowedSortOrders.Contains(sortOrder.ToString().ToLowerInvariant()))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 400,
                Title = "Invalid Sort Order",
                Detail = "sortOrder must be 'asc' or 'desc'"
            });
            return;
        }

        // Validate page/pageSize
        if (query.TryGetValue("page", out var page) &&
            (!int.TryParse(page, out var pageNum) || pageNum < 1))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 400,
                Title = "Invalid Page",
                Detail = "page must be a positive integer"
            });
            return;
        }

        await next(context);
    }
}
```

---

## 7. ERROR HANDLING (RFC 7807)

### 7.1 Problem Details Format

```csharp
/// <summary>
/// RFC 7807 Problem Details implementasyonu.
/// Tüm hata response'ları bu formatta döner.
/// </summary>

// Standard ProblemDetails (ASP.NET Core built-in)
{
  "type": "https://api.merge.com/errors/validation",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/products",
  "traceId": "00-abc123def456-789ghi-00"
}

// ValidationProblemDetails (validation errors için)
{
  "type": "https://api.merge.com/errors/validation",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Name is required", "Name cannot exceed 200 characters"],
    "Price": ["Price must be positive"],
    "SKU": ["SKU already exists"]
  },
  "traceId": "00-abc123def456-789ghi-00"
}

// Custom extensions
{
  "type": "https://api.merge.com/errors/insufficient-stock",
  "title": "Insufficient Stock",
  "status": 422,
  "detail": "Product 'ABC-123' has only 5 items in stock, but 10 were requested.",
  "instance": "/api/v1/orders",
  "traceId": "00-abc123def456-789ghi-00",
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "availableStock": 5,
  "requestedQuantity": 10
}
```

### 7.2 Global Exception Handler

```csharp
/// <summary>
/// Global exception handler middleware.
/// Tüm exception'ları ProblemDetails formatına dönüştürür.
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException ex => HandleValidationException(ex),
            NotFoundException ex => HandleNotFoundException(ex),
            UnauthorizedException ex => HandleUnauthorizedException(ex),
            ForbiddenException ex => HandleForbiddenException(ex),
            ConflictException ex => HandleConflictException(ex),
            DomainException ex => HandleDomainException(ex),
            ConcurrencyException ex => HandleConcurrencyException(ex),
            _ => HandleUnknownException(exception)
        };

        // Add common properties
        problemDetails.Instance = httpContext.Request.Path;
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Log based on severity
        if (statusCode >= 500)
        {
            logger.LogError(exception,
                "Unhandled exception occurred. TraceId: {TraceId}",
                problemDetails.Extensions["traceId"]);
        }
        else
        {
            logger.LogWarning(
                "Client error occurred: {Title}. TraceId: {TraceId}",
                problemDetails.Title,
                problemDetails.Extensions["traceId"]);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }

    private (int, ProblemDetails) HandleValidationException(ValidationException ex)
    {
        var problemDetails = new ValidationProblemDetails
        {
            Type = "https://api.merge.com/errors/validation",
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred."
        };

        foreach (var error in ex.Errors)
        {
            problemDetails.Errors[error.PropertyName] =
                error.ErrorMessages.ToArray();
        }

        return (400, problemDetails);
    }

    private (int, ProblemDetails) HandleNotFoundException(NotFoundException ex)
    {
        return (404, new ProblemDetails
        {
            Type = "https://api.merge.com/errors/not-found",
            Title = "Resource Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = ex.Message
        });
    }

    private (int, ProblemDetails) HandleUnauthorizedException(UnauthorizedException ex)
    {
        return (401, new ProblemDetails
        {
            Type = "https://api.merge.com/errors/unauthorized",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = ex.Message
        });
    }

    private (int, ProblemDetails) HandleForbiddenException(ForbiddenException ex)
    {
        return (403, new ProblemDetails
        {
            Type = "https://api.merge.com/errors/forbidden",
            Title = "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = ex.Message
        });
    }

    private (int, ProblemDetails) HandleConflictException(ConflictException ex)
    {
        return (409, new ProblemDetails
        {
            Type = "https://api.merge.com/errors/conflict",
            Title = "Conflict",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message
        });
    }

    private (int, ProblemDetails) HandleDomainException(DomainException ex)
    {
        var problemDetails = new ProblemDetails
        {
            Type = $"https://api.merge.com/errors/{ex.ErrorCode.ToKebabCase()}",
            Title = ex.ErrorCode,
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = ex.Message
        };

        // Add custom extensions
        foreach (var (key, value) in ex.Metadata)
        {
            problemDetails.Extensions[key.ToCamelCase()] = value;
        }

        return (422, problemDetails);
    }

    private (int, ProblemDetails) HandleConcurrencyException(ConcurrencyException ex)
    {
        return (409, new ProblemDetails
        {
            Type = "https://api.merge.com/errors/concurrency",
            Title = "Concurrency Conflict",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message
        });
    }

    private (int, ProblemDetails) HandleUnknownException(Exception ex)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://api.merge.com/errors/internal",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError
        };

        // Development'ta detay göster
        if (environment.IsDevelopment())
        {
            problemDetails.Detail = ex.Message;
            problemDetails.Extensions["stackTrace"] = ex.StackTrace;
        }
        else
        {
            problemDetails.Detail = "An unexpected error occurred. Please try again later.";
        }

        return (500, problemDetails);
    }
}

// Program.cs'de registration
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

app.UseExceptionHandler();
```

### 7.3 Custom Exception Types

```csharp
/// <summary>
/// Özel exception tipleri.
/// </summary>

public class NotFoundException(string message) : Exception(message);

public class UnauthorizedException(string message = "Unauthorized") : Exception(message);

public class ForbiddenException(string message = "Forbidden") : Exception(message);

public class ConflictException(string message) : Exception(message);

public class ConcurrencyException(string message) : Exception(message);

/// <summary>
/// Domain exception - business rule ihlalleri için.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object> Metadata { get; }

    public DomainException(
        string errorCode,
        string message,
        Dictionary<string, object>? metadata = null)
        : base(message)
    {
        ErrorCode = errorCode;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
}

// Özel domain exception'lar
public class InsufficientStockException : DomainException
{
    public InsufficientStockException(
        Guid productId,
        int requested,
        int available)
        : base(
            "InsufficientStock",
            $"Requested {requested} items but only {available} available.",
            new Dictionary<string, object>
            {
                ["productId"] = productId,
                ["requestedQuantity"] = requested,
                ["availableStock"] = available
            })
    { }
}

public class DuplicateSkuException : DomainException
{
    public DuplicateSkuException(string sku)
        : base(
            "DuplicateSku",
            $"A product with SKU '{sku}' already exists.",
            new Dictionary<string, object> { ["sku"] = sku })
    { }
}
```

---

## 8. VALIDATION

### 8.1 FluentValidation Integration

```csharp
/// <summary>
/// Request validation - FluentValidation kullanır.
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        // Basit validasyonlar
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
            .Must(NotContainHtml).WithMessage("Name cannot contain HTML");

        RuleFor(x => x.Description)
            .MaximumLength(10000).WithMessage("Description cannot exceed 10000 characters");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("SKU can only contain uppercase letters, numbers, and hyphens")
            .MustAsync(BeUniqueSku).WithMessage("SKU already exists");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be positive")
            .LessThanOrEqualTo(1_000_000).WithMessage("Price cannot exceed 1,000,000");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters")
            .Must(BeValidCurrency).WithMessage("Invalid currency code");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
            .LessThanOrEqualTo(1_000_000).WithMessage("Stock cannot exceed 1,000,000");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");

        // Nested validation
        When(x => x.Tags != null && x.Tags.Any(), () =>
        {
            RuleForEach(x => x.Tags)
                .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters");

            RuleFor(x => x.Tags)
                .Must(x => x!.Count <= 20).WithMessage("Maximum 20 tags allowed");
        });
    }

    private bool NotContainHtml(string value) =>
        !Regex.IsMatch(value ?? "", @"<[^>]+>");

    private bool BeValidCurrency(string currency) =>
        new[] { "TRY", "USD", "EUR", "GBP" }.Contains(currency);

    private async Task<bool> BeUniqueSku(string sku, CancellationToken ct) =>
        !await _productRepository.ExistsBySkuAsync(sku, ct);
}
```

### 8.2 Validation Pipeline Behavior

```csharp
/// <summary>
/// MediatR validation pipeline behavior.
/// Tüm command'ları handler'a göndermeden önce validate eder.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .GroupBy(f => f.PropertyName)
            .Select(g => new ValidationError(
                g.Key,
                g.Select(e => e.ErrorMessage).ToList()))
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}

public record ValidationError(string PropertyName, List<string> ErrorMessages);

public class ValidationException(List<ValidationError> errors) : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; } = errors;
}
```

### 8.3 Model State Validation (Fallback)

```csharp
/// <summary>
/// ModelState validation (Data Annotations) için filter.
/// FluentValidation catch etmezse burası çalışır.
/// </summary>
public class ValidateModelStateFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Any() == true)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Type = "https://api.merge.com/errors/validation",
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new BadRequestObjectResult(problemDetails);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

---

## 9. VERSIONING

### 9.1 API Versioning Configuration

```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;

    // Version okuma stratejileri
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),                    // /api/v1/products
        new HeaderApiVersionReader("X-Api-Version"),         // X-Api-Version: 1.0
        new QueryStringApiVersionReader("api-version")       // ?api-version=1.0
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### 9.2 Version-Specific Controllers

```csharp
/// <summary>
/// V1 Products Controller.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsV1Controller : BaseController
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDtoV1>> GetById(Guid id)
    {
        // V1 implementation
    }
}

/// <summary>
/// V2 Products Controller - breaking changes için.
/// </summary>
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsV2Controller : BaseController
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDtoV2>> GetById(Guid id)
    {
        // V2 implementation - farklı response format
    }
}

/// <summary>
/// Her iki versiyonu da destekleyen controller.
/// </summary>
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/categories")]
public class CategoriesController : BaseController
{
    // V1 endpoint
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<List<CategoryDtoV1>>> GetAllV1() { }

    // V2 endpoint (pagination eklendi)
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult<PagedResult<CategoryDtoV2>>> GetAllV2(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    { }
}
```

### 9.3 Deprecation

```csharp
/// <summary>
/// Deprecated API version.
/// </summary>
[ApiVersion("0.9", Deprecated = true)]
[Route("api/v{version:apiVersion}/legacy-products")]
public class LegacyProductsController : BaseController
{
    [HttpGet]
    [Obsolete("Use /api/v1/products instead")]
    public async Task<ActionResult<List<OldProductDto>>> GetAll()
    {
        // Sunset header ekle
        Response.Headers.Append("Sunset", "Sat, 01 Jan 2025 00:00:00 GMT");
        Response.Headers.Append("Deprecation", "true");
        Response.Headers.Append("Link",
            "</api/v1/products>; rel=\"successor-version\"");

        // Eski implementasyon
        return Ok(await _legacyService.GetAllAsync());
    }
}
```

---

## 10. AUTHENTICATION & AUTHORIZATION

### 10.1 JWT Authentication

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append(
                        "Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/problem+json";
                return context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Type = "https://api.merge.com/errors/unauthorized",
                    Title = "Unauthorized",
                    Status = 401,
                    Detail = "Invalid or expired token"
                });
            }
        };
    });
```

### 10.2 Authorization Policies

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("SellerOrAdmin", policy =>
        policy.RequireRole("Admin", "Seller"));

    // Claim-based policies
    options.AddPolicy("VerifiedSeller", policy =>
        policy.RequireClaim("SellerVerified", "true"));

    // Custom requirement policies
    options.AddPolicy("ResourceOwner", policy =>
        policy.Requirements.Add(new ResourceOwnerRequirement()));

    // Age restriction
    options.AddPolicy("AdultOnly", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));

    // Subscription-based
    options.AddPolicy("PremiumUser", policy =>
        policy.Requirements.Add(new PremiumSubscriptionRequirement()));
});

// Custom authorization handler
public class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement, IOwnable>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        IOwnable resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId != null && resource.OwnerId.ToString() == userId)
        {
            context.Succeed(requirement);
        }
        else if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### 10.3 Controller Authorization

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Default: authenticated user gerekli
public class OrdersController : BaseController
{
    // Herkes erişebilir
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicInfo() { }

    // Sadece admin
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id) { }

    // Admin veya Seller
    [HttpPost]
    [Authorize(Policy = "SellerOrAdmin")]
    public async Task<IActionResult> Create(CreateOrderCommand command) { }

    // Resource owner kontrolü
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);

        // IDOR protection
        if (order.UserId != CurrentUserId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(order);
    }

    // Custom policy
    [HttpPut("{id}")]
    [Authorize(Policy = "ResourceOwner")]
    public async Task<IActionResult> Update(Guid id, UpdateOrderCommand command)
    {
        // ResourceOwnerHandler otomatik kontrol eder
        var order = await _orderService.GetByIdAsync(id);
        var authResult = await _authorizationService.AuthorizeAsync(
            User, order, "ResourceOwner");

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        return Ok(await _mediator.Send(command));
    }
}
```

---

## 11. RATE LIMITING

### 11.1 Rate Limiting Configuration

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    // Global rate limit
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // Endpoint-specific rate limits
    options.AddFixedWindowLimiter("api", options =>
    {
        options.PermitLimit = 60;
        options.Window = TimeSpan.FromMinutes(1);
    });

    options.AddSlidingWindowLimiter("auth", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
        options.SegmentsPerWindow = 2;
    });

    options.AddTokenBucketLimiter("premium", options =>
    {
        options.TokenLimit = 200;
        options.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        options.TokensPerPeriod = 200;
    });

    // Rejection response
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/problem+json";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Type = "https://api.merge.com/errors/rate-limit",
            Title = "Too Many Requests",
            Status = 429,
            Detail = "Rate limit exceeded. Please try again later."
        }, ct);
    };
});

app.UseRateLimiter();
```

### 11.2 Endpoint Rate Limiting

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseController
{
    // Strict rate limit for login
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginCommand command) { }

    // Even stricter for password reset
    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    [EnableRateLimitingByIp(5, 60)] // 5 per minute per IP
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command) { }
}

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("api")] // Default for all endpoints
public class ProductsController : BaseController
{
    // Disable rate limiting for this endpoint
    [HttpGet]
    [DisableRateLimiting]
    public async Task<IActionResult> GetAll() { }

    // Premium users get higher limits
    [HttpGet("premium")]
    [Authorize(Policy = "PremiumUser")]
    [EnableRateLimiting("premium")]
    public async Task<IActionResult> GetPremiumProducts() { }
}
```

### 11.3 Rate Limit Headers

```csharp
/// <summary>
/// Rate limit bilgilerini response header'lara ekler.
/// </summary>
public class RateLimitHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        // Rate limit headers (standart değil ama yaygın)
        context.Response.Headers.Append("X-RateLimit-Limit", "60");
        context.Response.Headers.Append("X-RateLimit-Remaining", "45");
        context.Response.Headers.Append("X-RateLimit-Reset",
            DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString());
    }
}

// Response headers
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 1640000000
Retry-After: 30  // (429 durumunda)
```

---

## 12. CACHING

### 12.1 Response Caching

```csharp
// Program.cs
builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(options =>
{
    // Default policy
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));

    // Named policies
    options.AddPolicy("Products", builder =>
        builder
            .Expire(TimeSpan.FromMinutes(10))
            .SetVaryByQuery("page", "pageSize", "categoryId"));

    options.AddPolicy("Static", builder =>
        builder.Expire(TimeSpan.FromHours(24)));

    options.AddPolicy("NoCache", builder =>
        builder.NoCache());
});

app.UseOutputCache();
```

### 12.2 Cache Headers

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : BaseController
{
    // Response caching with attributes
    [HttpGet]
    [ResponseCache(Duration = 60, VaryByQueryKeys = ["page", "pageSize", "categoryId"])]
    [OutputCache(PolicyName = "Products")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1) { }

    // ETag-based caching
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));

        if (product is null)
            return NotFound();

        // Generate ETag
        var etag = GenerateETag(product);

        // Check If-None-Match header
        if (Request.Headers.IfNoneMatch.Contains(etag))
        {
            return StatusCode(304); // Not Modified
        }

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "public, max-age=300";
        Response.Headers.LastModified = product.LastModifiedAt?.ToString("R")
            ?? product.CreatedAt.ToString("R");

        return Ok(product);
    }

    private static string GenerateETag(ProductDto product)
    {
        var content = JsonSerializer.Serialize(product);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return $"\"{Convert.ToBase64String(hash[..8])}\"";
    }
}
```

### 12.3 Cache Extension Methods

```csharp
/// <summary>
/// Response caching helper extensions.
/// </summary>
public static class CacheExtensions
{
    public static void AddETag<T>(this HttpResponse response, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        response.Headers.ETag = $"\"{Convert.ToBase64String(hash[..8])}\"";
    }

    public static void AddCacheControl(this HttpResponse response, TimeSpan maxAge)
    {
        response.Headers.CacheControl = $"public, max-age={(int)maxAge.TotalSeconds}";
    }

    public static void AddNoCacheHeaders(this HttpResponse response)
    {
        response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        response.Headers.Pragma = "no-cache";
        response.Headers.Expires = "0";
    }

    public static void AddLastModified(this HttpResponse response, DateTime lastModified)
    {
        response.Headers.LastModified = lastModified.ToUniversalTime().ToString("R");
    }

    public static bool IsNotModified(this HttpRequest request, string etag)
    {
        return request.Headers.IfNoneMatch.Contains(etag);
    }

    public static bool IsNotModifiedSince(this HttpRequest request, DateTime lastModified)
    {
        if (request.Headers.TryGetValue("If-Modified-Since", out var values) &&
            DateTime.TryParse(values.FirstOrDefault(), out var ifModifiedSince))
        {
            return lastModified <= ifModifiedSince;
        }
        return false;
    }
}
```

---

## 13. HATEOAS

### 13.1 HATEOAS Response Format

```csharp
/// <summary>
/// HATEOAS (Hypermedia as the Engine of Application State).
/// Response'lara ilişkili link'ler ekler.
/// </summary>
public class HateoasProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }

    // HATEOAS links
    public List<Link> Links { get; init; } = [];
}

public record Link(string Rel, string Href, string Method);

// Response örneği
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Product Name",
  "price": 99.99,
  "links": [
    { "rel": "self", "href": "/api/v1/products/550e8400...", "method": "GET" },
    { "rel": "update", "href": "/api/v1/products/550e8400...", "method": "PUT" },
    { "rel": "delete", "href": "/api/v1/products/550e8400...", "method": "DELETE" },
    { "rel": "reviews", "href": "/api/v1/products/550e8400.../reviews", "method": "GET" },
    { "rel": "images", "href": "/api/v1/products/550e8400.../images", "method": "GET" },
    { "rel": "category", "href": "/api/v1/categories/xxx", "method": "GET" }
  ]
}
```

### 13.2 Link Generator Service

```csharp
/// <summary>
/// HATEOAS link generator.
/// </summary>
public class LinkGenerator(IHttpContextAccessor httpContextAccessor)
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;

    public List<Link> GenerateProductLinks(Guid productId, bool canEdit)
    {
        var baseUrl = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}";
        var productUrl = $"{baseUrl}/api/v1/products/{productId}";

        var links = new List<Link>
        {
            new("self", productUrl, "GET"),
            new("reviews", $"{productUrl}/reviews", "GET"),
            new("images", $"{productUrl}/images", "GET")
        };

        if (canEdit)
        {
            links.Add(new("update", productUrl, "PUT"));
            links.Add(new("patch", productUrl, "PATCH"));
            links.Add(new("delete", productUrl, "DELETE"));
            links.Add(new("add-review", $"{productUrl}/reviews", "POST"));
            links.Add(new("upload-image", $"{productUrl}/images", "POST"));
        }

        return links;
    }

    public Dictionary<string, Link> GenerateCollectionLinks(
        int page, int pageSize, int totalPages)
    {
        var baseUrl = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}{_httpContext.Request.Path}";

        var links = new Dictionary<string, Link>
        {
            ["self"] = new("self", $"{baseUrl}?page={page}&pageSize={pageSize}", "GET"),
            ["first"] = new("first", $"{baseUrl}?page=1&pageSize={pageSize}", "GET"),
            ["last"] = new("last", $"{baseUrl}?page={totalPages}&pageSize={pageSize}", "GET")
        };

        if (page > 1)
            links["prev"] = new("prev", $"{baseUrl}?page={page - 1}&pageSize={pageSize}", "GET");

        if (page < totalPages)
            links["next"] = new("next", $"{baseUrl}?page={page + 1}&pageSize={pageSize}", "GET");

        return links;
    }
}
```

---

## 14. IDEMPOTENCY

### 14.1 Idempotency Key Middleware

```csharp
/// <summary>
/// Idempotency key middleware.
/// POST/PUT/PATCH isteklerinin tekrarlanmasını önler.
/// </summary>
public class IdempotencyMiddleware(
    RequestDelegate next,
    IDistributedCache cache,
    ILogger<IdempotencyMiddleware> logger)
{
    private static readonly string[] IdempotentMethods = ["POST", "PUT", "PATCH"];
    private const string IdempotencyKeyHeader = "X-Idempotency-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        // GET, DELETE, HEAD, OPTIONS için skip
        if (!IdempotentMethods.Contains(context.Request.Method))
        {
            await next(context);
            return;
        }

        // Idempotency key header kontrolü
        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var keyValues) ||
            string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            await next(context);
            return;
        }

        var idempotencyKey = keyValues.First()!;
        var cacheKey = $"idempotency:{context.Request.Path}:{idempotencyKey}";

        // Cache'te var mı kontrol et
        var cachedResponse = await cache.GetStringAsync(cacheKey);

        if (cachedResponse != null)
        {
            logger.LogInformation(
                "Returning cached response for idempotency key {Key}",
                idempotencyKey);

            var cached = JsonSerializer.Deserialize<CachedResponse>(cachedResponse)!;

            context.Response.StatusCode = cached.StatusCode;
            context.Response.ContentType = "application/json";

            foreach (var header in cached.Headers)
            {
                context.Response.Headers.TryAdd(header.Key, header.Value);
            }

            await context.Response.WriteAsync(cached.Body);
            return;
        }

        // Response'u yakala
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await next(context);

        // Response'u cache'le
        responseBody.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(responseBody).ReadToEndAsync();

        var responseToCache = new CachedResponse
        {
            StatusCode = context.Response.StatusCode,
            Body = body,
            Headers = context.Response.Headers
                .Where(h => h.Key.StartsWith("X-") || h.Key == "Location")
                .ToDictionary(h => h.Key, h => h.Value.ToString())
        };

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(responseToCache),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

        // Original stream'e yaz
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private record CachedResponse
    {
        public int StatusCode { get; init; }
        public string Body { get; init; } = null!;
        public Dictionary<string, string> Headers { get; init; } = new();
    }
}
```

### 14.2 Controller'da Idempotency

```csharp
[HttpPost]
[Authorize]
public async Task<ActionResult<OrderDto>> CreateOrder(
    [FromBody] CreateOrderCommand command,
    [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey,
    CancellationToken ct)
{
    // IdempotencyMiddleware otomatik handle eder

    // Veya manuel kontrol:
    if (!string.IsNullOrEmpty(idempotencyKey))
    {
        var existingOrder = await _orderService.GetByIdempotencyKeyAsync(
            idempotencyKey, ct);

        if (existingOrder != null)
        {
            return Ok(existingOrder); // Mevcut order'ı dön
        }
    }

    var result = await _mediator.Send(command with
    {
        IdempotencyKey = idempotencyKey
    }, ct);

    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

---

## 15. API DOCUMENTATION

### 15.1 Swagger/OpenAPI Configuration

```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Merge E-Commerce API",
        Description = "Enterprise-grade e-commerce platform API",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "api@merge.com",
            Url = new Uri("https://merge.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "Merge E-Commerce API (Preview)",
        Description = "Preview version with new features"
    });

    // JWT authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header. Example: 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Operation filters
    options.OperationFilter<AuthorizeCheckOperationFilter>();
    options.OperationFilter<ApiVersionOperationFilter>();
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Merge API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Merge API v2 (Preview)");
    options.RoutePrefix = "docs";
    options.DocumentTitle = "Merge API Documentation";
    options.DefaultModelsExpandDepth(-1); // Hide schemas by default
});
```

### 15.2 XML Documentation

```csharp
/// <summary>
/// Product yönetimi API endpoint'leri.
/// </summary>
/// <remarks>
/// Bu controller ürün CRUD işlemlerini ve
/// ilgili aksiyonları yönetir.
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Products")]
public class ProductsController : BaseController
{
    /// <summary>
    /// Yeni bir ürün oluşturur.
    /// </summary>
    /// <param name="command">Ürün oluşturma verileri</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan ürün</returns>
    /// <response code="201">Ürün başarıyla oluşturuldu</response>
    /// <response code="400">Validasyon hatası</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="409">SKU zaten mevcut</response>
    /// <example>
    /// POST /api/v1/products
    /// {
    ///   "name": "iPhone 15 Pro",
    ///   "sku": "IPHONE-15-PRO",
    ///   "price": 64999.99,
    ///   "currency": "TRY"
    /// }
    /// </example>
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken ct = default)
    {
        // Implementation
    }
}
```

---

## 16. MIDDLEWARE PIPELINE

### 16.1 Middleware Sıralaması

```csharp
// Program.cs - Middleware sıralaması ÖNEMLİ!
var app = builder.Build();

// 1. Exception handling (en üstte)
app.UseExceptionHandler();

// 2. HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// 3. Static files (varsa)
app.UseStaticFiles();

// 4. Routing
app.UseRouting();

// 5. CORS (authentication'dan önce)
app.UseCors("AllowAll");

// 6. Rate limiting
app.UseRateLimiter();

// 7. Request logging
app.UseRequestLogging();

// 8. Authentication
app.UseAuthentication();

// 9. Authorization
app.UseAuthorization();

// 10. Custom middleware
app.UseIdempotency();
app.UseRequestTiming();

// 11. Response caching
app.UseResponseCaching();
app.UseOutputCache();

// 12. Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### 16.2 Custom Middleware Örnekleri

```csharp
/// <summary>
/// Request timing middleware.
/// Her isteğin süresini ölçer ve header'a ekler.
/// </summary>
public class RequestTimingMiddleware(
    RequestDelegate next,
    ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Response başlamadan önce header ekle
        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            context.Response.Headers.Append(
                "X-Response-Time",
                $"{stopwatch.ElapsedMilliseconds}ms");
            return Task.CompletedTask;
        });

        await next(context);

        // Log slow requests
        if (stopwatch.ElapsedMilliseconds > 1000)
        {
            logger.LogWarning(
                "Slow request: {Method} {Path} took {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
        }
    }
}

/// <summary>
/// Request logging middleware.
/// Tüm istekleri structured format'ta loglar.
/// </summary>
public class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;

        logger.LogInformation(
            "Request {RequestId} started: {Method} {Path}",
            requestId,
            context.Request.Method,
            context.Request.Path);

        try
        {
            await next(context);

            logger.LogInformation(
                "Request {RequestId} completed: {StatusCode}",
                requestId,
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Request {RequestId} failed",
                requestId);
            throw;
        }
    }
}

/// <summary>
/// Correlation ID middleware.
/// Her isteğe benzersiz ID atar.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        // Add to log scope
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await next(context);
        }
    }
}
```

---

## 17. MINIMAL APIs VS CONTROLLERS

### 17.1 Ne Zaman Minimal API?

```csharp
// ✅ Minimal API için uygun senaryolar:
// - Basit CRUD endpoint'leri
// - Microservice'ler
// - Lambda/serverless
// - Prototipleme

// Minimal API örneği
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/api/v1/products", async (
    [FromQuery] int page,
    [FromQuery] int pageSize,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send(new GetProductsQuery(page, pageSize), ct);
    return Results.Ok(result);
})
.WithName("GetProducts")
.WithTags("Products")
.RequireAuthorization()
.Produces<PagedResult<ProductDto>>()
.ProducesValidationProblem();

app.MapGet("/api/v1/products/{id:guid}", async (
    Guid id,
    IMediator mediator,
    CancellationToken ct) =>
{
    var result = await mediator.Send(new GetProductByIdQuery(id), ct);
    return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetProductById")
.WithTags("Products")
.Produces<ProductDto>()
.ProducesProblem(404);

// ✅ Controller için uygun senaryolar (bu projede tercih edilen):
// - Karmaşık business logic
// - Çok sayıda endpoint
// - Attribute-based configuration
// - Inheritance gereksinimi
// - API versioning
```

### 17.2 Hybrid Yaklaşım

```csharp
// Controllers + Minimal API birlikte kullanılabilir

// Controllers: Ana business logic
app.MapControllers();

// Minimal API: Utility endpoint'leri
app.MapGet("/health", () => Results.Ok())
    .ExcludeFromDescription();

app.MapGet("/ready", async (ApplicationDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok();
    }
    catch
    {
        return Results.StatusCode(503);
    }
})
.ExcludeFromDescription();

// Minimal API: Simple CRUD for internal use
var internalApi = app.MapGroup("/internal/v1")
    .RequireAuthorization("InternalOnly");

internalApi.MapGet("/cache/clear", async (IDistributedCache cache) =>
{
    // Cache clear logic
    return Results.Ok();
});
```

---

## 18. ANTI-PATTERNS

### 18.1 Kaçınılması Gereken Hatalar

```csharp
// ❌ YANLIŞ 1: Fiil kullanımı
[HttpGet("/api/v1/getProducts")]      // YANLIŞ
[HttpGet("/api/v1/products")]         // DOĞRU

// ❌ YANLIŞ 2: Tekil isim
[Route("/api/v1/product/{id}")]       // YANLIŞ
[Route("/api/v1/products/{id}")]      // DOĞRU

// ❌ YANLIŞ 3: Entity'yi direkt dönmek
[HttpGet("{id}")]
public async Task<Product> GetById(Guid id)  // YANLIŞ - Entity
{
    return await _context.Products.FindAsync(id);
}

[HttpGet("{id}")]
public async Task<ProductDto> GetById(Guid id)  // DOĞRU - DTO
{
    var product = await _context.Products.FindAsync(id);
    return _mapper.Map<ProductDto>(product);
}

// ❌ YANLIŞ 4: Yanlış status code
[HttpPost]
public async Task<IActionResult> Create(...)
{
    var product = await _service.CreateAsync(...);
    return Ok(product);  // YANLIŞ - 200 yerine 201 olmalı
}

[HttpPost]
public async Task<IActionResult> Create(...)
{
    var product = await _service.CreateAsync(...);
    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);  // DOĞRU
}

// ❌ YANLIŞ 5: Business logic controller'da
[HttpPost("{id}/approve")]
public async Task<IActionResult> ApproveOrder(Guid id)
{
    var order = await _context.Orders.FindAsync(id);
    order.Status = OrderStatus.Approved;  // YANLIŞ - logic controller'da
    order.ApprovedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return Ok(order);
}

[HttpPost("{id}/approve")]
public async Task<IActionResult> ApproveOrder(Guid id)
{
    var result = await _mediator.Send(new ApproveOrderCommand(id));  // DOĞRU
    return Ok(result);
}

// ❌ YANLIŞ 6: Exception'ı client'a göstermek
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id)
{
    try
    {
        var product = await _service.GetByIdAsync(id);
        return Ok(product);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);  // YANLIŞ - internal details leak
    }
}
// Global exception handler kullan!

// ❌ YANLIŞ 7: Async void
[HttpPost]
public async void Create(...)  // YANLIŞ - exception handle edilemez
{
    await _service.CreateAsync(...);
}

[HttpPost]
public async Task<IActionResult> Create(...)  // DOĞRU
{
    var result = await _service.CreateAsync(...);
    return Created(...);
}

// ❌ YANLIŞ 8: CancellationToken kullanmamak
[HttpGet]
public async Task<IActionResult> GetAll()  // YANLIŞ
{
    var products = await _service.GetAllAsync();
    return Ok(products);
}

[HttpGet]
public async Task<IActionResult> GetAll(CancellationToken ct)  // DOĞRU
{
    var products = await _service.GetAllAsync(ct);
    return Ok(products);
}

// ❌ YANLIŞ 9: Senkron I/O
[HttpGet("{id}")]
public IActionResult GetById(Guid id)  // YANLIŞ - blocking
{
    var product = _service.GetById(id);
    return Ok(product);
}

[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id, CancellationToken ct)  // DOĞRU
{
    var product = await _service.GetByIdAsync(id, ct);
    return Ok(product);
}

// ❌ YANLIŞ 10: Magic string'ler
[Authorize(Roles = "Admin,Seller")]  // YANLIŞ
[Authorize(Policy = Policies.SellerOrAdmin)]  // DOĞRU

// ❌ YANLIŞ 11: Pagination olmadan liste dönmek
[HttpGet]
public async Task<List<ProductDto>> GetAll()  // YANLIŞ - tüm datayı çeker
{
    return await _service.GetAllAsync();
}

[HttpGet]
public async Task<PagedResult<ProductDto>> GetAll(  // DOĞRU
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    return await _service.GetPagedAsync(page, pageSize);
}
```

---

## CHECKLIST

### Controller Tasarımı
- [ ] Base controller kullanılıyor
- [ ] Async/await doğru kullanılıyor
- [ ] CancellationToken alınıyor
- [ ] DTO'lar kullanılıyor (entity değil)
- [ ] Doğru HTTP method kullanılıyor
- [ ] Doğru status code dönülüyor

### Route Design
- [ ] Çoğul isim kullanılıyor (/products)
- [ ] Küçük harf ve tire (/product-categories)
- [ ] Fiil yok (aksiyon endpoint'leri hariç)
- [ ] API version var (/api/v1/...)
- [ ] Nested resource'lar mantıklı

### Error Handling
- [ ] RFC 7807 Problem Details kullanılıyor
- [ ] Global exception handler var
- [ ] Doğru error code'lar dönülüyor
- [ ] Internal error'lar gizleniyor

### Security
- [ ] Authentication/Authorization var
- [ ] Rate limiting uygulandı
- [ ] Input validation var
- [ ] CORS doğru yapılandırıldı

### Documentation
- [ ] Swagger/OpenAPI aktif
- [ ] XML comments yazıldı
- [ ] Response type'lar belirtildi
- [ ] Örnek request/response var

---

*Bu kural dosyası, Merge E-Commerce Backend projesi için REST API tasarım standartlarını belirler.*
