---
paths:
  - "Merge.Application/**/*.cs"
  - "**/Commands/**/*.cs"
  - "**/Queries/**/*.cs"
  - "**/Handlers/**/*.cs"
alwaysApply: false
---

# CQRS PATTERN RULES - MediatR Implementation

> Bu dosya CQRS pattern kurallarını içerir.
> CLAUDE: Command ve Query oluştururken bu kurallara MUTLAKA uy!

---

## 1. COMMAND VS QUERY - TEMEL FARKLAR

| Aspect | Command | Query |
|--------|---------|-------|
| **Amaç** | State değiştirir (Create, Update, Delete) | Sadece data okur |
| **Return** | Created/Updated DTO veya bool | Data DTO veya collection |
| **Side Effects** | Evet (DB write, events, notifications) | Hayır (read-only) |
| **Caching** | Cache INVALIDATE eder | Cache KULLANIR |
| **Idempotency** | İdempotent olmalı | Doğal olarak idempotent |
| **Validation** | FluentValidation ZORUNLU | Opsiyonel |
| **Transaction** | IUnitOfWork.SaveChangesAsync | AsNoTracking |

---

## 2. DIRECTORY STRUCTURE

```
Merge.Application/
├── [Module]/                           # Her modül ayrı klasör
│   ├── Commands/                       # Tüm commands
│   │   ├── CreateProduct/
│   │   │   ├── CreateProductCommand.cs
│   │   │   ├── CreateProductCommandHandler.cs
│   │   │   └── CreateProductCommandValidator.cs
│   │   ├── UpdateProduct/
│   │   │   ├── UpdateProductCommand.cs
│   │   │   ├── UpdateProductCommandHandler.cs
│   │   │   └── UpdateProductCommandValidator.cs
│   │   ├── PatchProduct/
│   │   │   └── ...
│   │   └── DeleteProduct/
│   │       └── ...
│   ├── Queries/                        # Tüm queries
│   │   ├── GetProductById/
│   │   │   ├── GetProductByIdQuery.cs
│   │   │   └── GetProductByIdQueryHandler.cs
│   │   ├── GetAllProducts/
│   │   │   └── ...
│   │   └── SearchProducts/
│   │       └── ...
│   ├── DTOs/                           # Module-specific DTOs
│   │   ├── ProductDto.cs
│   │   ├── ProductListDto.cs
│   │   └── ProductDetailDto.cs
│   └── Mappings/                       # AutoMapper profiles
│       └── ProductMappingProfile.cs
├── Common/                             # Shared components
│   ├── Behaviors/                      # Pipeline behaviors
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── CachingBehavior.cs
│   ├── Interfaces/
│   │   ├── ICommand.cs
│   │   └── IQuery.cs
│   └── Models/
│       └── PagedResult.cs
└── DependencyInjection.cs
```

---

## 3. COMMAND IMPLEMENTATION

### 3.1 Command Definition

```csharp
namespace Merge.Application.Product.Commands.CreateProduct;

/// <summary>
/// Creates a new product in the catalog.
/// </summary>
/// <param name="Name">Product name (required, max 200 chars)</param>
/// <param name="Description">Product description (optional, max 5000 chars)</param>
/// <param name="SKU">Stock Keeping Unit (required, format: XXX-XXXXXX)</param>
/// <param name="Price">Product price (required, positive)</param>
/// <param name="StockQuantity">Initial stock (required, non-negative)</param>
/// <param name="CategoryId">Category ID (required, must exist)</param>
/// <param name="SellerId">Seller ID (optional, for marketplace)</param>
/// <param name="Attributes">Product attributes (optional)</param>
/// <param name="Tags">Product tags (optional)</param>
public record CreateProductCommand(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    string Currency,
    int StockQuantity,
    Guid CategoryId,
    Guid? SellerId = null,
    Dictionary<string, string>? Attributes = null,
    List<string>? Tags = null
) : IRequest<ProductDto>;
```

### 3.2 Command Handler (Full Implementation)

```csharp
namespace Merge.Application.Product.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IRepository<Merge.Domain.Modules.Catalog.Product> productRepository,
    IRepository<Category> categoryRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cache,
    ICurrentUserService currentUser,
    ILogger<CreateProductCommandHandler> logger
) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // 1. Validate business rules (beyond FluentValidation)
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException("Category", request.CategoryId);

        if (!category.IsActive)
            throw new DomainException($"Category '{category.Name}' is not active");

        // 2. Check for duplicate SKU
        var existingProduct = await productRepository.GetBySpecAsync(
            new ProductBySkuSpec(request.SKU), ct);

        if (existingProduct is not null)
            throw new ConflictException($"Product with SKU '{request.SKU}' already exists");

        // 3. Create domain entity via factory method (NEVER use new!)
        var product = Product.Create(
            name: request.Name,
            description: request.Description,
            sku: SKU.Create(request.SKU),
            price: Money.Create(request.Price, request.Currency),
            stockQuantity: request.StockQuantity,
            categoryId: request.CategoryId
        );

        // 4. Set optional properties via domain methods
        if (request.SellerId.HasValue)
        {
            product.SetSeller(request.SellerId.Value);
        }

        if (request.Attributes is not null)
        {
            foreach (var (key, value) in request.Attributes)
            {
                product.AddAttribute(key, value);
            }
        }

        if (request.Tags is not null)
        {
            foreach (var tag in request.Tags)
            {
                product.AddTag(tag);
            }
        }

        // 5. Persist (NO SaveChanges in repository!)
        await productRepository.AddAsync(product, ct);

        // 6. Commit transaction + publish domain events via Outbox
        await unitOfWork.SaveChangesAsync(ct);

        // 7. Invalidate related caches
        await InvalidateCachesAsync(product, ct);

        // 8. Log success (structured logging, no PII)
        logger.LogInformation(
            "Product created: {ProductId}, SKU: {SKU}, Category: {CategoryId}, CreatedBy: {UserId}",
            product.Id, product.SKU.Value, product.CategoryId, currentUser.UserId);

        // 9. Return DTO (NEVER return entity!)
        return mapper.Map<ProductDto>(product);
    }

    private async Task InvalidateCachesAsync(Product product, CancellationToken ct)
    {
        var tasks = new List<Task>
        {
            cache.RemoveByPrefixAsync("products_", ct),
            cache.RemoveByPrefixAsync($"category_{product.CategoryId}_products", ct),
            cache.RemoveAsync("product_count", ct)
        };

        if (product.SellerId.HasValue)
        {
            tasks.Add(cache.RemoveByPrefixAsync($"seller_{product.SellerId}_products", ct));
        }

        await Task.WhenAll(tasks);
    }
}
```

### 3.3 Command Validator (FluentValidation)

```csharp
namespace Merge.Application.Product.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(
        IRepository<Category> categoryRepository,
        IRepository<Product> productRepository)
    {
        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Product name is required")
            .MaximumLength(200)
                .WithMessage("Product name cannot exceed 200 characters")
            .Must(name => !ContainsHtml(name))
                .WithMessage("Product name cannot contain HTML");

        // Description validation
        RuleFor(x => x.Description)
            .MaximumLength(5000)
                .WithMessage("Description cannot exceed 5000 characters")
            .Must(desc => !ContainsScript(desc))
                .WithMessage("Description cannot contain scripts")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // SKU validation
        RuleFor(x => x.SKU)
            .NotEmpty()
                .WithMessage("SKU is required")
            .Matches(@"^[A-Z0-9-]{3,50}$")
                .WithMessage("SKU must be 3-50 characters, uppercase letters, numbers, and hyphens only")
            .MustAsync(async (sku, ct) =>
            {
                var exists = await productRepository.ExistsAsync(
                    new ProductBySkuSpec(sku), ct);
                return !exists;
            })
                .WithMessage("A product with this SKU already exists");

        // Price validation
        RuleFor(x => x.Price)
            .GreaterThan(0)
                .WithMessage("Price must be greater than zero")
            .LessThanOrEqualTo(10_000_000)
                .WithMessage("Price cannot exceed 10,000,000")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
                .WithMessage("Price must have at most 2 decimal places");

        // Currency validation
        RuleFor(x => x.Currency)
            .NotEmpty()
                .WithMessage("Currency is required")
            .Must(currency => SupportedCurrencies.Contains(currency.ToUpperInvariant()))
                .WithMessage("Currency must be one of: TRY, USD, EUR, GBP");

        // Stock validation
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
                .WithMessage("Stock quantity cannot be negative")
            .LessThanOrEqualTo(1_000_000)
                .WithMessage("Stock quantity cannot exceed 1,000,000");

        // Category validation
        RuleFor(x => x.CategoryId)
            .NotEmpty()
                .WithMessage("Category is required")
            .MustAsync(async (id, ct) =>
            {
                var category = await categoryRepository.GetByIdAsync(id, ct);
                return category is not null && category.IsActive;
            })
                .WithMessage("Category not found or inactive");

        // Tags validation
        RuleForEach(x => x.Tags)
            .MaximumLength(50)
                .WithMessage("Each tag cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9-]+$")
                .WithMessage("Tags can only contain letters, numbers, and hyphens")
            .When(x => x.Tags is not null);

        RuleFor(x => x.Tags)
            .Must(tags => tags is null || tags.Count <= 20)
                .WithMessage("Cannot have more than 20 tags");

        // Attributes validation
        RuleFor(x => x.Attributes)
            .Must(attrs => attrs is null || attrs.Count <= 50)
                .WithMessage("Cannot have more than 50 attributes");
    }

    private static readonly string[] SupportedCurrencies = ["TRY", "USD", "EUR", "GBP"];

    private static bool ContainsHtml(string value)
        => !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"<[^>]+>");

    private static bool ContainsScript(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        var lower = value.ToLowerInvariant();
        return lower.Contains("<script") || lower.Contains("javascript:");
    }
}
```

---

## 4. UPDATE COMMAND PATTERN

### 4.1 Full Update (PUT)

```csharp
namespace Merge.Application.Product.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    Guid CategoryId,
    bool IsActive
) : IRequest<ProductDto>;

public class UpdateProductCommandHandler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cache,
    ICurrentUserService currentUser,
    ILogger<UpdateProductCommandHandler> logger
) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        // 1. Get existing entity
        var product = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Product", request.Id);

        // 2. Authorization check
        if (product.SellerId.HasValue &&
            product.SellerId != currentUser.UserId &&
            !currentUser.IsInRole("Admin"))
        {
            throw new UnauthorizedException("You can only update your own products");
        }

        // 3. Store old values for comparison (for events)
        var oldPrice = product.Price;
        var oldCategoryId = product.CategoryId;

        // 4. Update via domain methods (NEVER set properties directly!)
        product.UpdateDetails(request.Name, request.Description);
        product.SetPrice(Money.Create(request.Price, request.Currency));
        product.SetStock(request.StockQuantity);

        if (request.CategoryId != product.CategoryId)
        {
            product.ChangeCategory(request.CategoryId);
        }

        if (request.IsActive && !product.IsActive)
        {
            product.Activate();
        }
        else if (!request.IsActive && product.IsActive)
        {
            product.Deactivate();
        }

        // 5. Commit
        await unitOfWork.SaveChangesAsync(ct);

        // 6. Invalidate caches
        await cache.RemoveAsync($"product_{product.Id}", ct);
        await cache.RemoveByPrefixAsync("products_", ct);

        if (oldCategoryId != product.CategoryId)
        {
            await cache.RemoveByPrefixAsync($"category_{oldCategoryId}_", ct);
            await cache.RemoveByPrefixAsync($"category_{product.CategoryId}_", ct);
        }

        // 7. Log
        logger.LogInformation(
            "Product updated: {ProductId}, UpdatedBy: {UserId}",
            product.Id, currentUser.UserId);

        return mapper.Map<ProductDto>(product);
    }
}
```

### 4.2 Partial Update (PATCH) - Nullable Properties Pattern

```csharp
namespace Merge.Application.Product.Commands.PatchProduct;

/// <summary>
/// Partially updates a product. Only non-null properties are updated.
/// </summary>
public record PatchProductCommand(
    Guid Id,
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? StockQuantity = null,
    bool? IsActive = null,
    Guid? CategoryId = null
) : IRequest<ProductDto>;

public class PatchProductCommandHandler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cache,
    ILogger<PatchProductCommandHandler> logger
) : IRequestHandler<PatchProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(PatchProductCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Product", request.Id);

        // Track if anything changed
        var changed = false;

        // Only update properties that are provided
        if (request.Name is not null && request.Name != product.Name)
        {
            product.UpdateName(request.Name);
            changed = true;
        }

        if (request.Description is not null && request.Description != product.Description)
        {
            product.UpdateDescription(request.Description);
            changed = true;
        }

        if (request.Price.HasValue && request.Price.Value != product.Price.Amount)
        {
            product.SetPrice(Money.Create(request.Price.Value, product.Price.Currency));
            changed = true;
        }

        if (request.StockQuantity.HasValue && request.StockQuantity.Value != product.StockQuantity)
        {
            product.SetStock(request.StockQuantity.Value);
            changed = true;
        }

        if (request.IsActive.HasValue && request.IsActive.Value != product.IsActive)
        {
            if (request.IsActive.Value)
                product.Activate();
            else
                product.Deactivate();
            changed = true;
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value != product.CategoryId)
        {
            product.ChangeCategory(request.CategoryId.Value);
            changed = true;
        }

        // Only save if something changed
        if (changed)
        {
            await unitOfWork.SaveChangesAsync(ct);
            await cache.RemoveAsync($"product_{product.Id}", ct);
            await cache.RemoveByPrefixAsync("products_", ct);

            logger.LogInformation("Product patched: {ProductId}", product.Id);
        }

        return mapper.Map<ProductDto>(product);
    }
}

// Validator for PATCH - all fields optional
public class PatchProductCommandValidator : AbstractValidator<PatchProductCommand>
{
    public PatchProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        // Validate only if provided
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
            .When(x => x.Name is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be positive")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
            .When(x => x.StockQuantity.HasValue);
    }
}
```

---

## 5. DELETE COMMAND PATTERN

### 5.1 Soft Delete (PREFERRED)

```csharp
namespace Merge.Application.Product.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

public class DeleteProductCommandHandler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ICurrentUserService currentUser,
    ILogger<DeleteProductCommandHandler> logger
) : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Product", request.Id);

        // Authorization
        if (product.SellerId.HasValue &&
            product.SellerId != currentUser.UserId &&
            !currentUser.IsInRole("Admin"))
        {
            throw new UnauthorizedException("You can only delete your own products");
        }

        // Business rule: Can't delete products with pending orders
        if (await HasPendingOrdersAsync(product.Id, ct))
        {
            throw new DomainException("Cannot delete product with pending orders");
        }

        // Soft delete via domain method
        product.Delete();

        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate caches
        await cache.RemoveAsync($"product_{product.Id}", ct);
        await cache.RemoveByPrefixAsync("products_", ct);

        logger.LogInformation(
            "Product deleted: {ProductId}, DeletedBy: {UserId}",
            product.Id, currentUser.UserId);

        return true;
    }

    private async Task<bool> HasPendingOrdersAsync(Guid productId, CancellationToken ct)
    {
        // Implementation depends on your domain
        return false;
    }
}
```

### 5.2 Hard Delete (USE WITH CAUTION)

```csharp
public record HardDeleteProductCommand(Guid Id) : IRequest<bool>;

public class HardDeleteProductCommandHandler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork
) : IRequestHandler<HardDeleteProductCommand, bool>
{
    public async Task<bool> Handle(HardDeleteProductCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Product", request.Id);

        // Hard delete - actually removes from database
        await repository.DeleteAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
```

---

## 6. QUERY IMPLEMENTATION

### 6.1 GetById Query

```csharp
namespace Merge.Application.Product.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler(
    IRepository<Product> repository,
    IMapper mapper,
    ICacheService cache,
    ILogger<GetProductByIdQueryHandler> logger
) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var cacheKey = $"product_{request.Id}";

        // 1. Try cache first
        var cached = await cache.GetAsync<ProductDto>(cacheKey, ct);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for product: {ProductId}", request.Id);
            return cached;
        }

        // 2. Query with specification (includes, filters)
        var spec = new ProductByIdSpec(request.Id);
        var product = await repository.GetBySpecAsync(spec, ct);

        if (product is null)
        {
            logger.LogDebug("Product not found: {ProductId}", request.Id);
            return null;
        }

        // 3. Map to DTO
        var dto = mapper.Map<ProductDto>(product);

        // 4. Cache result
        await cache.SetAsync(cacheKey, dto, CacheDuration, ct);

        return dto;
    }
}
```

### 6.2 GetAll Query (Paginated)

```csharp
namespace Merge.Application.Product.Queries.GetAllProducts;

public record GetAllProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false,
    bool? IsActive = null,
    Guid? CategoryId = null
) : IRequest<PagedResult<ProductListDto>>;

public class GetAllProductsQueryHandler(
    IRepository<Product> repository,
    IMapper mapper,
    ICacheService cache,
    ILogger<GetAllProductsQueryHandler> logger
) : IRequestHandler<GetAllProductsQuery, PagedResult<ProductListDto>>
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<ProductListDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken ct)
    {
        // Sanitize pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        // Build cache key from request parameters
        var cacheKey = BuildCacheKey(request, page, pageSize);

        // Try cache
        var cached = await cache.GetAsync<PagedResult<ProductListDto>>(cacheKey, ct);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for products list");
            return cached;
        }

        // Build specification
        var spec = new ProductsFilterSpec(
            page: page,
            pageSize: pageSize,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            isActive: request.IsActive,
            categoryId: request.CategoryId);

        // Execute queries in parallel
        var productsTask = repository.ListAsync(spec, ct);
        var countTask = repository.CountAsync(spec.WithoutPaging(), ct);

        await Task.WhenAll(productsTask, countTask);

        var products = await productsTask;
        var totalCount = await countTask;

        // Map and create result
        var dtos = mapper.Map<List<ProductListDto>>(products);

        var result = new PagedResult<ProductListDto>(
            items: dtos,
            page: page,
            pageSize: pageSize,
            totalCount: totalCount);

        // Cache for shorter duration (list changes more frequently)
        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);

        return result;
    }

    private static string BuildCacheKey(GetAllProductsQuery request, int page, int pageSize)
    {
        var parts = new List<string> { "products", $"p{page}", $"s{pageSize}" };

        if (request.SortBy is not null)
            parts.Add($"sort_{request.SortBy}_{(request.SortDescending ? "desc" : "asc")}");

        if (request.IsActive.HasValue)
            parts.Add($"active_{request.IsActive}");

        if (request.CategoryId.HasValue)
            parts.Add($"cat_{request.CategoryId}");

        return string.Join("_", parts);
    }
}
```

### 6.3 Search Query

```csharp
namespace Merge.Application.Product.Queries.SearchProducts;

public record SearchProductsQuery(
    string? SearchTerm = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    Guid? CategoryId = null,
    List<string>? Tags = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "Relevance",
    bool SortDescending = true
) : IRequest<PagedResult<ProductSearchDto>>;

public class SearchProductsQueryHandler(
    IRepository<Product> repository,
    IMapper mapper,
    ISearchService searchService,
    ICacheService cache
) : IRequestHandler<SearchProductsQuery, PagedResult<ProductSearchDto>>
{
    public async Task<PagedResult<ProductSearchDto>> Handle(
        SearchProductsQuery request,
        CancellationToken ct)
    {
        // For complex search, consider using Elasticsearch
        if (searchService.IsAvailable && !string.IsNullOrEmpty(request.SearchTerm))
        {
            return await searchService.SearchProductsAsync(request, ct);
        }

        // Fallback to database search
        var spec = new ProductSearchSpec(
            searchTerm: request.SearchTerm,
            minPrice: request.MinPrice,
            maxPrice: request.MaxPrice,
            categoryId: request.CategoryId,
            tags: request.Tags,
            page: request.Page,
            pageSize: request.PageSize);

        var products = await repository.ListAsync(spec, ct);
        var totalCount = await repository.CountAsync(spec.WithoutPaging(), ct);

        var dtos = mapper.Map<List<ProductSearchDto>>(products);

        return new PagedResult<ProductSearchDto>(
            items: dtos,
            page: request.Page,
            pageSize: request.PageSize,
            totalCount: totalCount);
    }
}
```

---

## 7. PIPELINE BEHAVIORS

### 7.1 Validation Behavior

```csharp
namespace Merge.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            logger.LogWarning(
                "Validation failed for {RequestName}: {Errors}",
                requestName,
                string.Join(", ", failures.Select(f => f.ErrorMessage)));

            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### 7.2 Logging Behavior

```csharp
namespace Merge.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUser
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUser.UserId;

        logger.LogInformation(
            "Handling {RequestName} for user {UserId}",
            requestName, userId);

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await next();

            sw.Stop();
            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMs}ms: {ErrorMessage}",
                requestName, sw.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }
}
```

### 7.3 Caching Behavior (For Queries)

```csharp
namespace Merge.Application.Common.Behaviors;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? CacheDuration { get; }
}

public class CachingBehavior<TRequest, TResponse>(
    ICacheService cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var cacheKey = request.CacheKey;
        var duration = request.CacheDuration ?? TimeSpan.FromMinutes(10);

        var cached = await cache.GetAsync<TResponse>(cacheKey, ct);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        var response = await next();

        await cache.SetAsync(cacheKey, response, duration, ct);
        logger.LogDebug("Cached {CacheKey} for {Duration}", cacheKey, duration);

        return response;
    }
}
```

### 7.4 Transaction Behavior (For Commands)

```csharp
namespace Merge.Application.Common.Behaviors;

public interface ITransactionalCommand { }

public class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITransactionalCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            logger.LogDebug("Beginning transaction for {RequestName}", requestName);

            var response = await next();

            await transaction.CommitAsync(ct);

            logger.LogDebug("Committed transaction for {RequestName}", requestName);

            return response;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            logger.LogWarning("Rolled back transaction for {RequestName}", requestName);
            throw;
        }
    }
}
```

---

## 8. SPECIFICATIONS

### 8.1 Base Specification

```csharp
namespace Merge.Application.Common.Specifications;

public abstract class Specification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int? Skip { get; protected set; }
    public int? Take { get; protected set; }
    public bool AsNoTracking { get; protected set; } = true;
    public bool AsSplitQuery { get; protected set; }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void ApplyPaging(int page, int pageSize)
    {
        Skip = (page - 1) * pageSize;
        Take = pageSize;
    }

    public Specification<T> WithoutPaging()
    {
        var clone = (Specification<T>)MemberwiseClone();
        clone.Skip = null;
        clone.Take = null;
        return clone;
    }
}
```

### 8.2 Product Specifications

```csharp
namespace Merge.Application.Product.Specifications;

public class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(Guid id)
    {
        Criteria = p => p.Id == id && !p.IsDeleted;

        AddInclude(p => p.Category);
        AddInclude("Attributes");
        AddInclude("Tags");
        AddInclude("Images");
    }
}

public class ProductBySkuSpec : Specification<Product>
{
    public ProductBySkuSpec(string sku)
    {
        Criteria = p => p.SKU.Value == sku && !p.IsDeleted;
    }
}

public class ProductsFilterSpec : Specification<Product>
{
    public ProductsFilterSpec(
        int page,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        bool? isActive,
        Guid? categoryId)
    {
        // Base criteria
        Criteria = p => !p.IsDeleted;

        // Combine with additional filters
        if (isActive.HasValue)
        {
            var baseCriteria = Criteria;
            Criteria = p => baseCriteria.Compile()(p) && p.IsActive == isActive.Value;
        }

        if (categoryId.HasValue)
        {
            var baseCriteria = Criteria;
            Criteria = p => baseCriteria.Compile()(p) && p.CategoryId == categoryId.Value;
        }

        // Includes
        AddInclude(p => p.Category);
        AsSplitQuery = true;

        // Sorting
        ApplySorting(sortBy, sortDescending);

        // Pagination
        ApplyPaging(page, pageSize);
    }

    private void ApplySorting(string? sortBy, bool descending)
    {
        Expression<Func<Product, object>> orderExpression = sortBy?.ToLowerInvariant() switch
        {
            "name" => p => p.Name,
            "price" => p => p.Price.Amount,
            "created" => p => p.CreatedAt,
            "stock" => p => p.StockQuantity,
            _ => p => p.CreatedAt
        };

        if (descending)
            OrderByDescending = orderExpression;
        else
            OrderBy = orderExpression;
    }
}
```

---

## 9. DTOs & MAPPING

### 9.1 DTOs

```csharp
namespace Merge.Application.Product.DTOs;

// Full detail DTO
public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string SKU,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    string CategoryName,
    Guid? SellerId,
    string? SellerName,
    List<ProductAttributeDto> Attributes,
    List<string> Tags,
    List<ProductImageDto> Images,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// List DTO (lighter)
public record ProductListDto(
    Guid Id,
    string Name,
    string SKU,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    string CategoryName,
    string? MainImageUrl);

// Search result DTO
public record ProductSearchDto(
    Guid Id,
    string Name,
    string SKU,
    decimal Price,
    string Currency,
    string CategoryName,
    string? MainImageUrl,
    double? RelevanceScore);

// Nested DTOs
public record ProductAttributeDto(string Key, string Value);
public record ProductImageDto(Guid Id, string Url, int DisplayOrder, bool IsMain);
```

### 9.2 AutoMapper Profile

```csharp
namespace Merge.Application.Product.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Price.Currency))
            .ForMember(d => d.SKU, opt => opt.MapFrom(s => s.SKU.Value))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SellerName, opt => opt.MapFrom(s => s.Seller != null ? s.Seller.Name : null))
            .ForMember(d => d.Tags, opt => opt.MapFrom(s => s.Tags.Select(t => t.Value).ToList()));

        CreateMap<Product, ProductListDto>()
            .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Price.Currency))
            .ForMember(d => d.SKU, opt => opt.MapFrom(s => s.SKU.Value))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.MainImageUrl, opt => opt.MapFrom(s =>
                s.Images.FirstOrDefault(i => i.IsMain) != null
                    ? s.Images.First(i => i.IsMain).Url
                    : s.Images.FirstOrDefault() != null
                        ? s.Images.First().Url
                        : null));

        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<ProductImage, ProductImageDto>();
    }
}
```

---

## 10. PAGED RESULT

```csharp
namespace Merge.Application.Common.Models;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Empty(int page = 1, int pageSize = 20)
        => new([], page, pageSize, 0);
}
```

---

## CQRS CHECKLIST

Her Command için:
- [ ] Record type kullanılmış
- [ ] IRequest<T> implement edilmiş
- [ ] Handler primary constructor kullanıyor
- [ ] FluentValidation validator var
- [ ] Factory method ile entity oluşturuluyor
- [ ] IUnitOfWork.SaveChangesAsync çağrılıyor
- [ ] Cache invalidate ediliyor
- [ ] Structured logging yapılıyor
- [ ] DTO return ediliyor (entity değil!)

Her Query için:
- [ ] Record type kullanılmış
- [ ] IRequest<T?> veya IRequest<PagedResult<T>> implement edilmiş
- [ ] AsNoTracking kullanılıyor
- [ ] Cache kullanılıyor (uygun ise)
- [ ] Specification pattern kullanılıyor
- [ ] Pagination implement edilmiş (list queries)
