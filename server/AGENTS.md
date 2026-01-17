# AGENTS.md - Cross-IDE AI Assistant Instructions

> **Universal AI Assistant Configuration for Merge E-Commerce Backend**
>
> Compatible with: Claude Code, Cursor, GitHub Copilot, Windsurf, Cody, Continue, and other AI assistants.

---

## 🎯 Project Overview

| Property | Value |
|----------|-------|
| **Project** | Merge E-Commerce Backend |
| **Framework** | .NET 9.0 / C# 12 |
| **Database** | PostgreSQL 16 |
| **Cache** | Redis |
| **Architecture** | Clean Architecture + DDD + CQRS |
| **Code Stats** | 4,262 files, ~208,800 lines |
| **Solution** | `Merge.sln` |

---

## 🏗️ Architecture Overview

### Dependency Flow (STRICT!)

```
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                            │
│              (Controllers, Middleware, Filters)              │
└─────────────────────────┬───────────────────────────────────┘
                          │ depends on
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│         (Commands, Queries, Handlers, Services, DTOs)        │
└─────────────────────────┬───────────────────────────────────┘
                          │ depends on
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│      (Entities, Value Objects, Events, Specifications)       │
│                    *** NO DEPENDENCIES ***                   │
└─────────────────────────────────────────────────────────────┘
                          ▲
                          │ implements interfaces from
┌─────────────────────────┴───────────────────────────────────┐
│                  Infrastructure Layer                        │
│        (DbContexts, Repositories, External Services)         │
└─────────────────────────────────────────────────────────────┘

❌ FORBIDDEN:
- Domain → Infrastructure
- Domain → Application
- Application → API
- Application → Infrastructure (except DI registration)
```

### Project Structure

```
Merge.sln
├── Merge.Domain/                    # Pure business logic (NO dependencies)
│   ├── Modules/                     # Bounded contexts
│   │   ├── Catalog/                 # Product, Category, Brand
│   │   ├── Ordering/                # Order, OrderItem
│   │   ├── Identity/                # User, Role
│   │   └── ...
│   ├── ValueObjects/                # Money, Email, Address, SKU
│   ├── SharedKernel/                # BaseEntity, Guard, DomainEvent
│   └── Specifications/              # Query specifications
│
├── Merge.Application/               # Business orchestration
│   ├── [Module]/
│   │   ├── Commands/                # State-changing operations
│   │   │   └── CreateProduct/
│   │   │       ├── CreateProductCommand.cs
│   │   │       ├── CreateProductCommandHandler.cs
│   │   │       └── CreateProductCommandValidator.cs
│   │   ├── Queries/                 # Read operations
│   │   │   └── GetProductById/
│   │   │       ├── GetProductByIdQuery.cs
│   │   │       └── GetProductByIdQueryHandler.cs
│   │   └── EventHandlers/           # Domain event handlers
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Services/                    # Application services
│   ├── Mappings/                    # AutoMapper profiles
│   └── Behaviors/                   # MediatR pipeline behaviors
│
├── Merge.Infrastructure/            # External concerns
│   ├── Data/
│   │   ├── Contexts/                # DbContexts (12 total)
│   │   ├── Configurations/          # Entity configurations
│   │   └── Migrations/              # EF Core migrations
│   ├── Repositories/                # Repository implementations
│   ├── ExternalServices/            # Payment, Email, SMS
│   └── Caching/                     # Redis implementation
│
├── Merge.API/                       # HTTP interface
│   ├── Controllers/                 # API controllers
│   ├── Middleware/                  # Exception, auth middleware
│   └── Filters/                     # Action filters
│
└── Merge.Tests/                     # All tests
    ├── Unit/                        # Unit tests
    └── Integration/                 # Integration tests
```

---

## 🔧 Code Patterns

### CQRS with MediatR

#### Command (Changes State)
```csharp
// Command definition
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    int StockQuantity
) : IRequest<ProductDto>;

// Handler with primary constructor (C# 12)
public class CreateProductCommandHandler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateProductCommandHandler> logger
) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        logger.LogInformation("Creating product {Name}", request.Name);

        // Use factory method (DDD)
        var product = Product.Create(
            request.Name,
            request.Description,
            Money.Create(request.Price, request.Currency),
            request.CategoryId,
            request.StockQuantity);

        await repository.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);  // Commits + publishes domain events

        return mapper.Map<ProductDto>(product);
    }
}

// Validator (FluentValidation)
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be 3 characters (ISO 4217)");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");
    }
}
```

#### Query (Read Only)
```csharp
// Query definition
public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDetailDto?>;

// Handler with caching
public class GetProductByIdQueryHandler(
    IReadRepository<Product> repository,
    IMapper mapper,
    IDistributedCache cache,
    ILogger<GetProductByIdQueryHandler> logger
) : IRequestHandler<GetProductByIdQuery, ProductDetailDto?>
{
    public async Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var cacheKey = $"product:{request.ProductId}";

        // Try cache first
        var cached = await cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for product {ProductId}", request.ProductId);
            return JsonSerializer.Deserialize<ProductDetailDto>(cached);
        }

        // Query database (read-only)
        var product = await repository.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return null;

        var dto = mapper.Map<ProductDetailDto>(product);

        // Cache for 5 minutes
        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            ct);

        return dto;
    }
}
```

### Domain-Driven Design

#### Entity with Factory Method
```csharp
public sealed class Product : BaseEntity, IAggregateRoot, ISoftDeletable
{
    // Private constructor for EF Core
    private Product() { }

    // Factory method (REQUIRED)
    public static Product Create(
        string name,
        string? description,
        Money price,
        Guid categoryId,
        int stockQuantity = 0)
    {
        // Validation with Guard clauses
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNegative(stockQuantity, nameof(stockQuantity));

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Slug = name.ToSlug(),
            Price = price,
            CategoryId = categoryId,
            StockQuantity = stockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Raise domain event
        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name, product.Price.Amount));

        return product;
    }

    // Properties (private setters)
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Slug { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public Category Category { get; private set; } = null!;
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    private readonly List<ProductImage> _images = [];

    // Domain methods (not property setters!)
    public void SetPrice(Money newPrice)
    {
        Guard.AgainstNull(newPrice, nameof(newPrice));

        if (Price == newPrice) return;

        var oldPrice = Price;
        Price = newPrice;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice.Amount, newPrice.Amount));
    }

    public void ReduceStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        if (StockQuantity < quantity)
            throw new InsufficientStockException(Id, StockQuantity, quantity);

        StockQuantity -= quantity;
        LastModifiedAt = DateTime.UtcNow;

        if (StockQuantity == 0)
            AddDomainEvent(new ProductOutOfStockEvent(Id));
    }

    public void AddStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        StockQuantity += quantity;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
        AddDomainEvent(new ProductDeactivatedEvent(Id));
    }

    // ISoftDeletable
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        AddDomainEvent(new ProductDeletedEvent(Id));
    }
}
```

#### Value Object
```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        Guard.AgainstNegative(amount, nameof(amount));
        Guard.AgainstNullOrEmpty(currency, nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be 3 characters (ISO 4217)", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new CurrencyMismatchException(Currency, other.Currency);

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new CurrencyMismatchException(Currency, other.Currency);

        return Create(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier) =>
        new(Amount * multiplier, Currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
```

#### Domain Event
```csharp
public sealed record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    decimal Price
) : DomainEvent;

// Handler
public class ProductCreatedEventHandler(
    ILogger<ProductCreatedEventHandler> logger,
    ISearchIndexService searchIndex
) : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Product created: {ProductId} - {Name}",
            notification.ProductId, notification.Name);

        // Index for search
        await searchIndex.IndexProductAsync(notification.ProductId, ct);
    }
}
```

---

## 📋 Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Class | PascalCase | `ProductService` |
| Interface | IPascalCase | `IProductService` |
| Async Method | *Async suffix | `GetProductAsync` |
| Private Field | _camelCase | `_repository` |
| Constant | UPPER_SNAKE | `MAX_RETRY_COUNT` |
| Command | [Action][Entity]Command | `CreateProductCommand` |
| Query | Get[Entity]Query | `GetProductByIdQuery` |
| Handler | [Name]Handler | `CreateProductCommandHandler` |
| Validator | [Name]Validator | `CreateProductCommandValidator` |
| DTO | [Entity]Dto | `ProductDto`, `ProductListDto`, `ProductDetailDto` |
| Event | [Entity][Action]Event | `ProductCreatedEvent` |
| Exception | [Name]Exception | `ProductNotFoundException` |
| Specification | [Filter]Spec | `ActiveProductsByCategorySpec` |

---

## 🛡️ Security Rules

### CRITICAL: Never Do
```csharp
// ❌ Hardcoded secrets
var secret = "MySecretKey123!";
var connectionString = "Host=localhost;Password=admin";

// ❌ PII in logs
_logger.LogInformation("User {Email} logged in", user.Email);
_logger.LogInformation("Card: {CardNumber}", payment.CardNumber);

// ❌ String interpolation in logs (prevents structured logging)
_logger.LogInformation($"User {userId} logged in");

// ❌ Exposed entity in API
[HttpGet("{id}")]
public async Task<Product> Get(Guid id) => await _repo.GetAsync(id);  // BAD!

// ❌ No IDOR protection
public async Task<Order> GetOrder(Guid id) =>
    await _context.Orders.FindAsync(id);  // Anyone can access any order!
```

### CORRECT Patterns
```csharp
// ✅ Environment variables
var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
var connectionString = Configuration.GetConnectionString("Default");

// ✅ Structured logging without PII
_logger.LogInformation("User {UserId} logged in", user.Id);
_logger.LogInformation("Payment processed for order {OrderId}", order.Id);

// ✅ Return DTO, not entity
[HttpGet("{id}")]
public async Task<ActionResult<ProductDto>> Get(Guid id)
{
    var result = await _mediator.Send(new GetProductByIdQuery(id));
    return result is null ? NotFound() : Ok(result);
}

// ✅ IDOR protection
public async Task<Order?> GetOrder(Guid id, Guid currentUserId)
{
    var order = await _context.Orders.FindAsync(id);
    if (order is null) return null;

    if (order.UserId != currentUserId && !await IsAdminAsync(currentUserId))
        throw new ForbiddenException("Access denied to this order");

    return order;
}

// ✅ Proper hashing
// Passwords: BCrypt
var hash = BCrypt.Net.BCrypt.HashPassword(password);

// Tokens: SHA256
using var sha256 = SHA256.Create();
var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
```

### Authorization Patterns
```csharp
[ApiController]
[Authorize]  // Default: authenticated required
public class OrdersController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Customer")]  // Role-based
    public async Task<IActionResult> GetAll() { }

    [HttpPost]
    [Authorize(Policy = "CanCreateOrders")]  // Policy-based
    public async Task<IActionResult> Create() { }

    [HttpGet("public")]
    [AllowAnonymous]  // Explicit public access
    public async Task<IActionResult> GetPublicInfo() { }
}
```

---

## 🗄️ Database Rules

### Query Optimization
```csharp
// ✅ ALWAYS: AsNoTracking for read-only queries
var products = await _context.Products
    .AsNoTracking()
    .Where(p => p.IsActive)
    .ToListAsync(ct);

// ✅ ALWAYS: AsSplitQuery for multiple includes
var order = await _context.Orders
    .AsSplitQuery()
    .Include(o => o.Items).ThenInclude(i => i.Product)
    .Include(o => o.ShippingAddress)
    .FirstOrDefaultAsync(o => o.Id == id, ct);

// ✅ ALWAYS: Project to DTO
var products = await _context.Products
    .AsNoTracking()
    .ProjectTo<ProductListDto>(_mapper.ConfigurationProvider)
    .ToListAsync(ct);

// ✅ ALWAYS: Server-side pagination
var products = await _context.Products
    .AsNoTracking()
    .OrderByDescending(p => p.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(ct);
```

### Repository Pattern
```csharp
// ✅ Repository adds, UnitOfWork saves
public async Task AddAsync(Product product, CancellationToken ct)
{
    await _context.Products.AddAsync(product, ct);
    // NO SaveChanges here!
}

// In handler:
await _repository.AddAsync(product, ct);
await _unitOfWork.SaveChangesAsync(ct);  // Commits + processes Outbox
```

---

## 🎨 API Design

### Controller Structure
```csharp
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetAllProductsQuery(page, pageSize);
        return Ok(await mediator.Send(query, ct));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        return Ok(await mediator.Send(command, ct));
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin,Seller")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> Patch(
        Guid id,
        [FromBody] PatchProductCommand command,
        CancellationToken ct)
    {
        command = command with { Id = id };
        return Ok(await mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);
        return NoContent();
    }
}
```

### HTTP Status Codes

| Status | Use Case |
|--------|----------|
| 200 | Successful GET, PUT, PATCH |
| 201 | Successful POST (+ Location header) |
| 204 | Successful DELETE |
| 400 | Validation error |
| 401 | Not authenticated |
| 403 | Not authorized |
| 404 | Resource not found |
| 409 | Conflict (duplicate, concurrency) |
| 422 | Business rule violation |
| 429 | Rate limit exceeded |
| 500 | Internal server error |

### Error Response (RFC 7807)
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/products",
  "errors": {
    "Name": ["Name is required", "Name cannot exceed 200 characters"],
    "Price": ["Price must be greater than 0"]
  },
  "traceId": "00-abc123-def456-00"
}
```

---

## 🧪 Testing

### Test Naming
```
[MethodName]_[Scenario]_[ExpectedResult]

Examples:
- Create_ValidParameters_ReturnsProduct
- Create_EmptyName_ThrowsArgumentException
- Handle_ValidCommand_CreatesAndReturnsDto
- Validate_InvalidPrice_ReturnsError
```

### Unit Test Example
```csharp
public class ProductTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsProduct()
    {
        // Arrange
        var name = "Test Product";
        var price = Money.Create(99.99m, "USD");
        var categoryId = Guid.NewGuid();

        // Act
        var product = Product.Create(name, price, categoryId);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(name);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_InvalidName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var price = Money.Create(99.99m, "USD");

        // Act
        var act = () => Product.Create(name!, price, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
```

### Coverage Targets

| Category | Target |
|----------|--------|
| Domain Entities | 90%+ |
| Value Objects | 95%+ |
| Command Handlers | 80%+ |
| Overall | 60%+ |

---

## 🚀 Commands

```bash
# Build
dotnet build

# Run (https://localhost:5001)
dotnet run --project Merge.API

# Test
dotnet test
dotnet test --filter "Category=Unit"
dotnet test --filter "FullyQualifiedName~ProductTests"

# Migrations
dotnet ef migrations add MigrationName \
    --project Merge.Infrastructure \
    --startup-project Merge.API

dotnet ef database update \
    --project Merge.Infrastructure \
    --startup-project Merge.API

# Docker
docker-compose up -d
docker-compose logs -f api

# Format
dotnet format
```

---

## 📦 Tech Stack

| Category | Technology | Version |
|----------|------------|---------|
| Framework | .NET | 9.0 |
| Language | C# | 12 |
| ORM | Entity Framework Core | 9.0 |
| Database | PostgreSQL | 16 |
| Cache | Redis | Latest |
| Mediator | MediatR | 12.4.1 |
| Mapping | AutoMapper | 12.0.1 |
| Validation | FluentValidation | 11.9.2 |
| API Docs | Swashbuckle | 7.2.0 |
| Auth | JWT Bearer | Latest |
| Testing | xUnit | 2.9.* |
| Assertions | FluentAssertions | 7.0.* |
| Mocking | Moq | 4.20.* |
| Containers | Testcontainers | 3.10.* |

---

## ✅ Checklist for Every PR

### Before Committing
- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes
- [ ] No hardcoded secrets
- [ ] No PII in logs
- [ ] Using factory methods for entities
- [ ] Using domain methods (not setters)
- [ ] Using AsNoTracking for reads
- [ ] Using AsSplitQuery for includes
- [ ] Returning DTOs (not entities)
- [ ] FluentValidation for commands
- [ ] CancellationToken in async methods
- [ ] Proper authorization attributes

### Entity Checklist
- [ ] Private constructor
- [ ] Factory method
- [ ] Domain events for state changes
- [ ] Guard clauses for validation
- [ ] No public setters
- [ ] Soft delete implementation

### Handler Checklist
- [ ] Uses CancellationToken
- [ ] Proper error handling
- [ ] Logging included
- [ ] Returns DTO
- [ ] Cache invalidation (if needed)

### Controller Checklist
- [ ] Proper HTTP methods
- [ ] Authorization attributes
- [ ] ProducesResponseType attributes
- [ ] CancellationToken parameter

---

## 📚 Additional Resources

For more detailed rules, see:
- `.claude/CLAUDE.md` - Claude Code specific instructions
- `.claude/rules/` - Modular rule files
- `.claude/commands/` - Slash commands
- `.cursor/rules/` - Cursor specific rules
- `arch.md` - Full architecture analysis

---

*This file is compatible with all major AI coding assistants including Claude Code, Cursor, GitHub Copilot, Windsurf, Cody, and Continue.*
