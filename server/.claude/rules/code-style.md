---
paths:
  - "**/*.cs"
alwaysApply: true
---

# C# 12 & .NET 9.0 CODE STYLE RULES

> Bu dosya tüm C# dosyaları için kod stili kurallarını içerir.
> CLAUDE: Bu kurallara MUTLAKA uy!

---

## 1. MODERN C# 12 FEATURES (ZORUNLU)

### 1.1 Primary Constructors (STANDART)

```csharp
// ✅ DOĞRU: Primary constructor - TÜM sınıflarda kullan
public class ProductService(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cache,
    ILogger<ProductService> logger)
{
    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(id, ct);
        return mapper.Map<ProductDto>(product);
    }
}

// ❌ YANLIŞ: Eski constructor pattern
public class ProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(
        IRepository<Product> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
}
```

**Primary Constructor Kuralları:**
- Handler, Service, Controller sınıflarında ZORUNLU
- Domain entity'lerde KULLANMA (private constructor gerekli)
- Dependency'ler doğrudan parametre olarak kullanılır
- Field'a atama YAPILMAZ (`private readonly` gereksiz)

### 1.2 Collection Expressions (C# 12)

```csharp
// ✅ DOĞRU: Collection expressions
List<string> items = [];
int[] numbers = [1, 2, 3, 4, 5];
List<Product> products = [product1, product2];
Dictionary<string, int> dict = new() { ["key1"] = 1, ["key2"] = 2 };

// Spread operator
int[] combined = [..firstArray, ..secondArray, 99];
List<OrderItem> allItems = [..pendingItems, ..confirmedItems];

// Empty collections
IReadOnlyList<Product> empty = [];
var emptyOrders = Array.Empty<Order>();

// ❌ YANLIŞ: Eski syntax
var items = new List<string>();
var numbers = new int[] { 1, 2, 3 };
var products = new List<Product>() { product1, product2 };
```

### 1.3 Record Types

```csharp
// ✅ DOĞRU: DTOs için record kullan
public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAt);

// ✅ DOĞRU: Commands için record kullan
public record CreateProductCommand(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int StockQuantity,
    Guid CategoryId) : IRequest<ProductDto>;

// ✅ DOĞRU: Queries için record kullan
public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

// ✅ DOĞRU: Events için record kullan
public record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    string SKU,
    decimal Price) : DomainEvent;

// ❌ YANLIŞ: DTO için class kullanma
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
```

### 1.4 Pattern Matching (Gelişmiş)

```csharp
// ✅ DOĞRU: Property pattern
if (order is { Status: OrderStatus.Pending, Items.Count: > 0 })
{
    order.Submit();
}

// ✅ DOĞRU: List patterns
int[] numbers = [1, 2, 3, 4, 5];
if (numbers is [var first, .., var last])
{
    Console.WriteLine($"First: {first}, Last: {last}");
}

// ✅ DOĞRU: Type pattern with when clause
public decimal CalculateDiscount(object customer) => customer switch
{
    PremiumCustomer { YearsActive: > 5 } => 0.25m,
    PremiumCustomer { YearsActive: > 2 } => 0.15m,
    PremiumCustomer => 0.10m,
    RegularCustomer { OrderCount: > 100 } => 0.05m,
    RegularCustomer => 0.02m,
    _ => 0m
};

// ✅ DOĞRU: Relational patterns
public string GetPriceCategory(decimal price) => price switch
{
    < 10 => "Budget",
    >= 10 and < 50 => "Standard",
    >= 50 and < 200 => "Premium",
    >= 200 => "Luxury",
    _ => "Unknown"
};

// ✅ DOĞRU: is not pattern
if (product is not null)
{
    // process
}

if (order.Status is not (OrderStatus.Cancelled or OrderStatus.Refunded))
{
    // order is active
}
```

### 1.5 Null Checking (Modern)

```csharp
// ✅ DOĞRU: ArgumentNullException.ThrowIfNull
public void ProcessOrder(Order order)
{
    ArgumentNullException.ThrowIfNull(order);
    // process
}

// ✅ DOĞRU: ArgumentException.ThrowIfNullOrEmpty
public void SetName(string name)
{
    ArgumentException.ThrowIfNullOrEmpty(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    // set name
}

// ✅ DOĞRU: Null-coalescing assignment
_cache ??= new ConcurrentDictionary<string, object>();
_logger ??= NullLogger<ProductService>.Instance;

// ✅ DOĞRU: Null-conditional with method calls
var length = customer?.Address?.Street?.Length ?? 0;
await customer?.NotifyAsync(message, ct);

// ❌ YANLIŞ: Manuel null check
if (order == null)
    throw new ArgumentNullException(nameof(order));

if (string.IsNullOrEmpty(name))
    throw new ArgumentException("Name is required");
```

### 1.6 File-Scoped Namespaces (ZORUNLU)

```csharp
// ✅ DOĞRU: File-scoped namespace
namespace Merge.Application.Product.Commands.CreateProduct;

public record CreateProductCommand(...) : IRequest<ProductDto>;

// ❌ YANLIŞ: Block-scoped namespace
namespace Merge.Application.Product.Commands.CreateProduct
{
    public record CreateProductCommand(...) : IRequest<ProductDto>;
}
```

### 1.7 Required Properties (C# 11+)

```csharp
// ✅ DOĞRU: required modifier
public class ProductConfiguration
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public string? Description { get; init; }
}

// Kullanım
var config = new ProductConfiguration
{
    Name = "Test Product",    // Required - compile error if missing
    Price = 99.99m,           // Required - compile error if missing
    Description = "Optional"  // Optional
};
```

### 1.8 Raw String Literals (C# 11+)

```csharp
// ✅ DOĞRU: Raw string literals for multi-line
var sql = """
    SELECT p.Id, p.Name, p.Price
    FROM Products p
    WHERE p.CategoryId = @categoryId
      AND p.IsActive = true
    ORDER BY p.CreatedAt DESC
    """;

var json = """
    {
        "name": "Test Product",
        "price": 99.99,
        "isActive": true
    }
    """;

// ❌ YANLIŞ: Escaped strings
var sql = "SELECT p.Id, p.Name, p.Price\n" +
          "FROM Products p\n" +
          "WHERE p.CategoryId = @categoryId";
```

---

## 2. NAMING CONVENTIONS

### 2.1 Identifier Naming

| Element | Convention | Example |
|---------|------------|---------|
| Class | PascalCase | `ProductService`, `OrderManager` |
| Interface | IPascalCase | `IProductService`, `IRepository<T>` |
| Abstract Class | BasePascalCase / Abstract | `BaseEntity`, `AbstractValidator` |
| Record | PascalCase | `ProductDto`, `CreateProductCommand` |
| Enum | PascalCase | `OrderStatus`, `PaymentMethod` |
| Enum Member | PascalCase | `OrderStatus.Pending` |
| Method | PascalCase | `GetProduct`, `CalculateTotal` |
| Async Method | PascalCaseAsync | `GetProductAsync`, `SaveChangesAsync` |
| Property | PascalCase | `ProductId`, `TotalAmount` |
| Event | PascalCase | `ProductCreated`, `OrderShipped` |
| Private Field | _camelCase | `_repository`, `_logger` |
| Parameter | camelCase | `productId`, `cancellationToken` |
| Local Variable | camelCase | `currentProduct`, `totalAmount` |
| Constant | PascalCase | `MaxPageSize`, `DefaultCurrency` |
| Type Parameter | TPascalCase | `TEntity`, `TRequest`, `TResponse` |

### 2.2 CQRS Naming

```csharp
// Commands: [Action][Entity]Command
public record CreateProductCommand(...) : IRequest<ProductDto>;
public record UpdateProductPriceCommand(...) : IRequest<ProductDto>;
public record DeleteProductCommand(...) : IRequest<bool>;
public record PatchProductCommand(...) : IRequest<ProductDto>;

// Command Handlers: [CommandName]Handler
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>;
public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, ProductDto>;

// Command Validators: [CommandName]Validator
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>;

// Queries: Get[Entity]Query, Get[Entity]ByXQuery, GetAll[Entities]Query
public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
public record GetProductBySkuQuery(string SKU) : IRequest<ProductDto?>;
public record GetAllProductsQuery(int Page, int PageSize) : IRequest<PagedResult<ProductDto>>;
public record SearchProductsQuery(string? Term, Guid? CategoryId) : IRequest<PagedResult<ProductDto>>;

// Query Handlers: [QueryName]Handler
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>;
```

### 2.3 Domain Naming

```csharp
// Entities: Singular noun
public class Product : BaseAggregateRoot { }
public class Order : BaseAggregateRoot { }
public class OrderItem : BaseEntity { }

// Value Objects: Descriptive noun
public record Money(decimal Amount, string Currency);
public record Address(string Street, string City, string PostalCode, string Country);
public record SKU(string Value);
public record Email(string Value);

// Domain Events: [Entity][PastTenseAction]Event
public record ProductCreatedEvent(Guid ProductId, string Name) : DomainEvent;
public record ProductPriceChangedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice) : DomainEvent;
public record OrderSubmittedEvent(Guid OrderId, Money TotalAmount) : DomainEvent;
public record OrderShippedEvent(Guid OrderId, string TrackingNumber) : DomainEvent;

// Specifications: [Entity][Criteria]Spec
public class ProductByIdSpec : Specification<Product>;
public class ProductByCategorySpec : Specification<Product>;
public class ActiveProductsSpec : Specification<Product>;
public class OrdersByUserIdSpec : Specification<Order>;
```

### 2.4 Infrastructure Naming

```csharp
// Repositories: [Entity]Repository (implementing IRepository<T>)
public class ProductRepository : IRepository<Product>;

// DbContexts: [Module]DbContext
public class CatalogDbContext : DbContext;
public class OrderingDbContext : DbContext;

// Configurations: [Entity]Configuration
public class ProductConfiguration : IEntityTypeConfiguration<Product>;
public class OrderConfiguration : IEntityTypeConfiguration<Order>;

// Services: [Feature]Service
public class EmailService : IEmailService;
public class PaymentService : IPaymentService;
public class CacheService : ICacheService;
```

### 2.5 API Naming

```csharp
// Controllers: [Entity]Controller (plural)
[ApiController]
[Route("api/v1/products")]
public class ProductsController : BaseController;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : BaseController;

// Endpoints: HTTP method + noun/verb
// GET    /api/v1/products           → GetAll
// GET    /api/v1/products/{id}      → GetById
// POST   /api/v1/products           → Create
// PUT    /api/v1/products/{id}      → Update
// PATCH  /api/v1/products/{id}      → Patch
// DELETE /api/v1/products/{id}      → Delete
```

---

## 3. FILE ORGANIZATION

### 3.1 File Structure

```csharp
// 1. File-scoped namespace (TEK SATIR)
namespace Merge.Application.Product.Commands.CreateProduct;

// 2. Using directives (namespace içinde, alphabetically sorted)
using AutoMapper;
using FluentValidation;
using MediatR;
using Merge.Domain.Modules.Catalog;

// 3. Primary type (dosya adıyla AYNI)
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price) : IRequest<ProductDto>;
```

### 3.2 One Type Per File

```csharp
// ✅ DOĞRU: Her dosyada TEK type
// CreateProductCommand.cs
public record CreateProductCommand(...) : IRequest<ProductDto>;

// CreateProductCommandHandler.cs
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>;

// CreateProductCommandValidator.cs
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>;

// ❌ YANLIŞ: Birden fazla type
// CreateProduct.cs
public record CreateProductCommand(...);
public class CreateProductCommandHandler { }  // AYIR!
public class CreateProductCommandValidator { } // AYIR!
```

### 3.3 Using Directives

```csharp
// ✅ DOĞRU: Implicit usings + explicit when needed
namespace Merge.Application.Product.Commands.CreateProduct;

using Merge.Domain.Modules.Catalog; // Only non-implicit ones

// Global usings (GlobalUsings.cs)
global using AutoMapper;
global using FluentValidation;
global using MediatR;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Logging;
```

---

## 4. METHOD GUIDELINES

### 4.1 Method Length (MAX 50 Satır)

```csharp
// ✅ DOĞRU: Kısa, odaklı method
public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
{
    var product = Product.Create(request.Name, request.Price, request.CategoryId);
    await repository.AddAsync(product, ct);
    await unitOfWork.SaveChangesAsync(ct);
    return mapper.Map<ProductDto>(product);
}

// ❌ YANLIŞ: 50+ satır method - böl!
public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
{
    // 100+ satır kod... BÖL!
}
```

### 4.2 Single Responsibility

```csharp
// ✅ DOĞRU: Her method TEK iş yapar
public async Task<ProductDto> CreateProductAsync(CreateProductCommand request, CancellationToken ct)
{
    var product = await CreateEntityAsync(request, ct);
    await PersistAsync(product, ct);
    await InvalidateCacheAsync(product.CategoryId, ct);
    return MapToDto(product);
}

private async Task<Product> CreateEntityAsync(CreateProductCommand request, CancellationToken ct)
{
    var category = await ValidateCategoryAsync(request.CategoryId, ct);
    return Product.Create(request.Name, request.Price, category.Id);
}

private async Task PersistAsync(Product product, CancellationToken ct)
{
    await repository.AddAsync(product, ct);
    await unitOfWork.SaveChangesAsync(ct);
}
```

### 4.3 Early Return Pattern

```csharp
// ✅ DOĞRU: Early return
public async Task<ProductDto?> GetProductAsync(Guid id, CancellationToken ct)
{
    if (id == Guid.Empty)
        return null;

    var cached = await cache.GetAsync<ProductDto>($"product_{id}", ct);
    if (cached is not null)
        return cached;

    var product = await repository.GetByIdAsync(id, ct);
    if (product is null)
        return null;

    return mapper.Map<ProductDto>(product);
}

// ❌ YANLIŞ: Nested conditions
public async Task<ProductDto?> GetProductAsync(Guid id, CancellationToken ct)
{
    if (id != Guid.Empty)
    {
        var cached = await cache.GetAsync<ProductDto>($"product_{id}", ct);
        if (cached is null)
        {
            var product = await repository.GetByIdAsync(id, ct);
            if (product is not null)
            {
                return mapper.Map<ProductDto>(product);
            }
        }
        else
        {
            return cached;
        }
    }
    return null;
}
```

### 4.4 Parameter Count (MAX 5)

```csharp
// ✅ DOĞRU: 5 veya daha az parametre
public async Task<Product> CreateProductAsync(
    string name,
    decimal price,
    Guid categoryId,
    CancellationToken ct);

// ✅ DOĞRU: Çok parametre varsa object kullan
public async Task<Product> CreateProductAsync(
    CreateProductRequest request,
    CancellationToken ct);

// ❌ YANLIŞ: 5'ten fazla parametre
public async Task<Product> CreateProductAsync(
    string name,
    string description,
    decimal price,
    int stock,
    Guid categoryId,
    Guid? sellerId,
    bool isActive,
    CancellationToken ct);
```

---

## 5. ASYNC/AWAIT RULES

### 5.1 Async All The Way

```csharp
// ✅ DOĞRU: Async chain korunur
public async Task<ProductDto> GetProductAsync(Guid id, CancellationToken ct)
{
    var product = await repository.GetByIdAsync(id, ct);
    var category = await categoryRepository.GetByIdAsync(product.CategoryId, ct);
    return mapper.Map<ProductDto>(product);
}

// ❌ YANLIŞ: Blocking calls
public ProductDto GetProduct(Guid id)
{
    var product = repository.GetByIdAsync(id, default).Result; // BLOCK!
    var category = categoryRepository.GetByIdAsync(product.CategoryId, default).GetAwaiter().GetResult(); // BLOCK!
    return mapper.Map<ProductDto>(product);
}
```

### 5.2 CancellationToken (ZORUNLU)

```csharp
// ✅ DOĞRU: CancellationToken her async method'da
public async Task<ProductDto> CreateProductAsync(
    CreateProductCommand request,
    CancellationToken ct) // ZORUNLU
{
    var product = Product.Create(request.Name, request.Price);
    await repository.AddAsync(product, ct);      // ct geçir
    await unitOfWork.SaveChangesAsync(ct);        // ct geçir
    await cache.RemoveAsync("products", ct);      // ct geçir
    return mapper.Map<ProductDto>(product);
}

// ❌ YANLIŞ: CancellationToken yok veya default
public async Task<ProductDto> CreateProductAsync(CreateProductCommand request)
{
    await repository.AddAsync(product, default); // default KULLANMA!
}
```

### 5.3 ConfigureAwait

```csharp
// Library kodu için (Infrastructure layer):
public async Task<T?> GetFromCacheAsync<T>(string key, CancellationToken ct)
{
    var value = await _cache.GetStringAsync(key, ct).ConfigureAwait(false);
    return value is null ? default : JsonSerializer.Deserialize<T>(value);
}

// Application/API layer için ConfigureAwait KULLANMA
// ASP.NET Core context gerekli olabilir
```

### 5.4 ValueTask vs Task

```csharp
// ✅ DOĞRU: Sık çağrılan, genellikle sync dönen methodlar için ValueTask
public ValueTask<Product?> GetFromCacheAsync(Guid id, CancellationToken ct)
{
    if (_memoryCache.TryGetValue(id, out Product? product))
        return ValueTask.FromResult(product);

    return new ValueTask<Product?>(LoadFromDatabaseAsync(id, ct));
}

// Normal durumlar için Task kullan
public Task<Product> CreateProductAsync(CreateProductCommand request, CancellationToken ct);
```

---

## 6. LOGGING RULES

### 6.1 Structured Logging (ZORUNLU)

```csharp
// ✅ DOĞRU: Structured logging
logger.LogInformation(
    "Product created: {ProductId}, Name: {ProductName}, Category: {CategoryId}",
    product.Id, product.Name, product.CategoryId);

logger.LogWarning(
    "Product not found: {ProductId}",
    id);

logger.LogError(
    ex,
    "Failed to create product: {ProductName}, Error: {ErrorMessage}",
    request.Name, ex.Message);

// ❌ YANLIŞ: String interpolation
logger.LogInformation($"Product created: {product.Id}"); // YANLIŞ!
logger.LogError($"Error: {ex.Message}");                  // YANLIŞ!
```

### 6.2 Log Levels

```csharp
// Trace: Çok detaylı debug bilgisi (prod'da kapalı)
logger.LogTrace("Entering method {MethodName} with params: {Params}", nameof(GetProduct), id);

// Debug: Geliştirme için debug bilgisi
logger.LogDebug("Cache miss for product: {ProductId}", id);

// Information: Normal akış bilgisi
logger.LogInformation("Order created: {OrderId}, Amount: {Amount}", order.Id, order.TotalAmount);

// Warning: Potansiyel sorun (ama devam edebilir)
logger.LogWarning("Payment retry attempt {Attempt} for order: {OrderId}", attempt, orderId);

// Error: Hata (exception var)
logger.LogError(ex, "Payment failed for order: {OrderId}", orderId);

// Critical: Kritik hata (sistem durabilir)
logger.LogCritical(ex, "Database connection lost!");
```

### 6.3 Sensitive Data (ASLA LOGLANMAZ)

```csharp
// ❌ YASAK: Sensitive data loglama
logger.LogInformation("User email: {Email}", user.Email);           // PII!
logger.LogDebug("Token: {Token}", token);                           // SECRET!
logger.LogInformation("Password: {Password}", password);            // SECRET!
logger.LogInformation("Credit card: {CardNumber}", cardNumber);     // PCI!
logger.LogInformation("Phone: {Phone}", user.PhoneNumber);          // PII!

// ✅ DOĞRU: Sadece ID logla
logger.LogInformation("User logged in: {UserId}", user.Id);
logger.LogInformation("Payment processed for order: {OrderId}", orderId);
```

---

## 7. EXCEPTION HANDLING

### 7.1 Domain Exceptions

```csharp
// Domain-specific exceptions
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, Guid id)
        : base($"{entity} with ID {id} not found") { }
}

public class ValidationException : DomainException
{
    public IEnumerable<string> Errors { get; }
    public ValidationException(IEnumerable<string> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized") : base(message) { }
}
```

### 7.2 Throwing Exceptions

```csharp
// ✅ DOĞRU: Meaningful exceptions
public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
{
    var product = await repository.GetByIdAsync(request.Id, ct)
        ?? throw new NotFoundException("Product", request.Id);

    return mapper.Map<ProductDto>(product);
}

// Domain validation
public void SetPrice(Money newPrice)
{
    if (newPrice.Amount < 0)
        throw new DomainException("Price cannot be negative");

    if (newPrice.Currency != Price.Currency)
        throw new DomainException($"Cannot change currency from {Price.Currency} to {newPrice.Currency}");

    var oldPrice = Price;
    Price = newPrice;
    AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
}
```

### 7.3 Exception Handling in Handlers

```csharp
// Global exception handler middleware handles most cases
// Only catch specific exceptions when needed

public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
{
    try
    {
        // Normal flow - exceptions propagate to middleware
        var order = await CreateOrderAsync(request, ct);
        return mapper.Map<OrderDto>(order);
    }
    catch (InsufficientStockException ex)
    {
        // Specific business logic for this exception
        logger.LogWarning("Order creation failed due to insufficient stock: {ProductId}", ex.ProductId);

        // Optionally notify or queue for later
        await notificationService.NotifyBackInStockAsync(ex.ProductId, request.UserId, ct);

        throw; // Re-throw for global handler
    }
}
```

---

## 8. LINQ GUIDELINES

### 8.1 Method Syntax (PREFERRED)

```csharp
// ✅ DOĞRU: Method syntax
var activeProducts = products
    .Where(p => p.IsActive)
    .Where(p => p.StockQuantity > 0)
    .OrderByDescending(p => p.CreatedAt)
    .Select(p => new ProductListDto(p.Id, p.Name, p.Price))
    .ToList();

// Query syntax sadece complex joins için
var orderDetails =
    from o in orders
    join c in customers on o.CustomerId equals c.Id
    join a in addresses on c.AddressId equals a.Id
    where o.Status == OrderStatus.Pending
    select new { Order = o, Customer = c, Address = a };
```

### 8.2 Deferred Execution

```csharp
// ✅ DOĞRU: ToList/ToArray ile materialize et
var productList = await context.Products
    .Where(p => p.IsActive)
    .ToListAsync(ct);

// ❌ YANLIŞ: IQueryable'ı döndürme
public IQueryable<Product> GetActiveProducts()
{
    return context.Products.Where(p => p.IsActive); // Tehlikeli!
}
```

### 8.3 Null Safety

```csharp
// ✅ DOĞRU: FirstOrDefault + null check
var product = await context.Products
    .FirstOrDefaultAsync(p => p.Id == id, ct);

if (product is null)
    throw new NotFoundException("Product", id);

// ✅ DOĞRU: SingleOrDefault for unique constraints
var user = await context.Users
    .SingleOrDefaultAsync(u => u.Email == email, ct);

// ❌ YANLIŞ: First() - exception risk
var product = await context.Products.FirstAsync(p => p.Id == id, ct); // Throws if not found!
```

### 8.4 Projection

```csharp
// ✅ DOĞRU: Project to DTO (better performance)
var productDtos = await context.Products
    .Where(p => p.IsActive)
    .Select(p => new ProductDto(
        p.Id,
        p.Name,
        p.Price.Amount,
        p.Category.Name))
    .ToListAsync(ct);

// ❌ YANLIŞ: Load full entity then map
var products = await context.Products
    .Include(p => p.Category)
    .ToListAsync(ct);
var dtos = mapper.Map<List<ProductDto>>(products); // Unnecessary data loaded!
```

---

## 9. COMMENTS & DOCUMENTATION

### 9.1 When to Comment

```csharp
// ✅ DOĞRU: Complex business logic
// Orders older than 30 days are archived to cold storage
// This improves query performance on the main table
public async Task ArchiveOldOrdersAsync(CancellationToken ct)

// ✅ DOĞRU: Non-obvious decisions
// Using HMACSHA256 instead of SHA256 as per security audit requirement (SEC-2024-001)
private byte[] HashToken(string token)

// ❌ YANLIŞ: Obvious code
// Get product by id
public async Task<Product> GetByIdAsync(Guid id, CancellationToken ct)

// ❌ YANLIŞ: Describing what code does (code should be self-documenting)
// Loop through products
foreach (var product in products)
```

### 9.2 XML Documentation (Public APIs Only)

```csharp
/// <summary>
/// Creates a new product in the catalog.
/// </summary>
/// <param name="request">Product creation details</param>
/// <param name="ct">Cancellation token</param>
/// <returns>Created product DTO</returns>
/// <exception cref="ValidationException">Thrown when validation fails</exception>
/// <exception cref="ConflictException">Thrown when SKU already exists</exception>
[HttpPost]
public async Task<ActionResult<ProductDto>> Create(
    CreateProductCommand request,
    CancellationToken ct)
```

---

## 10. CODE SMELLS TO AVOID

### 10.1 Magic Numbers/Strings

```csharp
// ✅ DOĞRU: Constants
public const int MaxPageSize = 100;
public const int DefaultPageSize = 20;
public const string DefaultCurrency = "TRY";

var products = await GetProductsAsync(page, Math.Min(pageSize, MaxPageSize), ct);

// ❌ YANLIŞ: Magic numbers
var products = await GetProductsAsync(page, Math.Min(pageSize, 100), ct);
```

### 10.2 Boolean Parameters

```csharp
// ✅ DOĞRU: Enum veya named parameters
public enum ProductFilter { Active, Inactive, All }

public async Task<List<ProductDto>> GetProductsAsync(ProductFilter filter, CancellationToken ct);

// Usage
var activeProducts = await GetProductsAsync(ProductFilter.Active, ct);

// ❌ YANLIŞ: Boolean parameters (unclear at call site)
public async Task<List<ProductDto>> GetProductsAsync(bool includeInactive, bool includeDeleted, CancellationToken ct);

// Unclear what true/false means
var products = await GetProductsAsync(true, false, ct); // ??
```

### 10.3 Long Parameter Lists

```csharp
// ✅ DOĞRU: Parameter object
public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    Guid CategoryId);

public async Task<Product> CreateAsync(CreateProductRequest request, CancellationToken ct);

// ❌ YANLIŞ: Long parameter list
public async Task<Product> CreateAsync(
    string name, string description, decimal price, int stock,
    Guid categoryId, Guid? sellerId, List<string> tags,
    Dictionary<string, string> attributes, CancellationToken ct);
```

---

## SUMMARY CHECKLIST

Her PR'da kontrol et:

- [ ] Primary constructors kullanılmış
- [ ] Collection expressions kullanılmış
- [ ] Record types DTOs/Commands/Queries için kullanılmış
- [ ] File-scoped namespaces kullanılmış
- [ ] Naming conventions uygulanmış
- [ ] Method'lar 50 satırı geçmiyor
- [ ] Early return pattern kullanılmış
- [ ] CancellationToken tüm async methodlarda var
- [ ] Structured logging kullanılmış
- [ ] Sensitive data loglanmamış
- [ ] LINQ method syntax kullanılmış
- [ ] Magic numbers/strings yok
