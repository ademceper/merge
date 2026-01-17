---
paths:
  - "**/*.cs"
---

# ERROR HANDLING KURALLARI (ULTRA KAPSAMLI)

> Bu dosya, Merge E-Commerce Backend projesinde hata yönetimi için
> kapsamlı kurallar ve en iyi uygulamaları içerir.

---

## İÇİNDEKİLER

1. [Exception Hiyerarşisi](#1-exception-hiyerarşisi)
2. [Domain Exceptions](#2-domain-exceptions)
3. [Application Exceptions](#3-application-exceptions)
4. [Global Exception Handler](#4-global-exception-handler)
5. [Result Pattern](#5-result-pattern)
6. [Validation Errors](#6-validation-errors)
7. [HTTP Status Code Mapping](#7-http-status-code-mapping)
8. [Logging Errors](#8-logging-errors)
9. [Retry Patterns](#9-retry-patterns)
10. [Best Practices](#10-best-practices)

---

## 1. EXCEPTION HİYEARŞİSİ

### 1.1 Custom Exception Base

```csharp
/// <summary>
/// Tüm custom exception'lar için base class.
/// </summary>
public abstract class MergeException : Exception
{
    public string ErrorCode { get; }
    public int HttpStatusCode { get; }
    public IDictionary<string, object> Metadata { get; }

    protected MergeException(
        string errorCode,
        string message,
        int httpStatusCode = 500,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
        Metadata = new Dictionary<string, object>();
    }

    public MergeException WithMetadata(string key, object value)
    {
        Metadata[key] = value;
        return this;
    }
}
```

### 1.2 Exception Kategorileri

```
MergeException (base)
├── DomainException (422) - Business rule violations
│   ├── InsufficientStockException
│   ├── InvalidOrderStateException
│   ├── DuplicateSkuException
│   └── PriceChangedException
├── NotFoundException (404) - Resource not found
│   ├── ProductNotFoundException
│   ├── OrderNotFoundException
│   └── UserNotFoundException
├── ValidationException (400) - Input validation errors
├── UnauthorizedException (401) - Authentication required
├── ForbiddenException (403) - Authorization denied
├── ConflictException (409) - Concurrency/Duplicate
│   ├── ConcurrencyException
│   └── DuplicateResourceException
└── IntegrationException (502) - External service errors
    ├── PaymentGatewayException
    └── EmailServiceException
```

---

## 2. DOMAIN EXCEPTIONS

### 2.1 Domain Exception Base

```csharp
/// <summary>
/// Domain layer business rule ihlalleri için.
/// HTTP Status: 422 Unprocessable Entity
/// </summary>
public abstract class DomainException : MergeException
{
    protected DomainException(string errorCode, string message)
        : base(errorCode, message, 422)
    {
    }
}
```

### 2.2 Specific Domain Exceptions

```csharp
/// <summary>
/// Yetersiz stok exception'ı.
/// </summary>
public class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }

    public InsufficientStockException(
        Guid productId,
        int requestedQuantity,
        int availableStock)
        : base(
            "INSUFFICIENT_STOCK",
            $"Product {productId} has insufficient stock. Requested: {requestedQuantity}, Available: {availableStock}")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableStock = availableStock;

        WithMetadata("productId", productId);
        WithMetadata("requestedQuantity", requestedQuantity);
        WithMetadata("availableStock", availableStock);
    }
}

/// <summary>
/// Geçersiz sipariş durumu exception'ı.
/// </summary>
public class InvalidOrderStateException : DomainException
{
    public Guid OrderId { get; }
    public string CurrentState { get; }
    public string AttemptedAction { get; }

    public InvalidOrderStateException(
        Guid orderId,
        string currentState,
        string attemptedAction)
        : base(
            "INVALID_ORDER_STATE",
            $"Cannot {attemptedAction} order {orderId}. Current state: {currentState}")
    {
        OrderId = orderId;
        CurrentState = currentState;
        AttemptedAction = attemptedAction;

        WithMetadata("orderId", orderId);
        WithMetadata("currentState", currentState);
        WithMetadata("attemptedAction", attemptedAction);
    }
}

/// <summary>
/// Duplicate SKU exception'ı.
/// </summary>
public class DuplicateSkuException : DomainException
{
    public string SKU { get; }

    public DuplicateSkuException(string sku)
        : base(
            "DUPLICATE_SKU",
            $"A product with SKU '{sku}' already exists")
    {
        SKU = sku;
        WithMetadata("sku", sku);
    }
}

/// <summary>
/// Fiyat değişikliği exception'ı (cart'ta ürün fiyatı değişti).
/// </summary>
public class PriceChangedException : DomainException
{
    public Guid ProductId { get; }
    public decimal ExpectedPrice { get; }
    public decimal CurrentPrice { get; }

    public PriceChangedException(
        Guid productId,
        decimal expectedPrice,
        decimal currentPrice)
        : base(
            "PRICE_CHANGED",
            $"Product {productId} price changed from {expectedPrice:C} to {currentPrice:C}")
    {
        ProductId = productId;
        ExpectedPrice = expectedPrice;
        CurrentPrice = currentPrice;

        WithMetadata("productId", productId);
        WithMetadata("expectedPrice", expectedPrice);
        WithMetadata("currentPrice", currentPrice);
    }
}
```

### 2.3 Domain'de Exception Fırlatma

```csharp
public class Product : BaseAggregateRoot
{
    public void ReduceStock(int quantity)
    {
        Guard.Against.NegativeOrZero(quantity, nameof(quantity));

        if (StockQuantity < quantity)
        {
            throw new InsufficientStockException(Id, quantity, StockQuantity);
        }

        StockQuantity -= quantity;
        AddDomainEvent(new ProductStockReducedEvent(Id, quantity, StockQuantity));
    }
}

public class Order : BaseAggregateRoot
{
    public void Cancel(string reason)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
        {
            throw new InvalidOrderStateException(Id, Status.ToString(), "cancel");
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }
}
```

---

## 3. APPLICATION EXCEPTIONS

### 3.1 NotFoundException

```csharp
/// <summary>
/// Resource bulunamadı exception'ı.
/// HTTP Status: 404 Not Found
/// </summary>
public class NotFoundException : MergeException
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public NotFoundException(string resourceType, object resourceId)
        : base(
            "RESOURCE_NOT_FOUND",
            $"{resourceType} with ID '{resourceId}' was not found",
            404)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;

        WithMetadata("resourceType", resourceType);
        WithMetadata("resourceId", resourceId);
    }

    // Generic factory methods
    public static NotFoundException For<T>(Guid id) where T : class
        => new(typeof(T).Name, id);

    public static NotFoundException For<T>(string identifier) where T : class
        => new(typeof(T).Name, identifier);
}

// Kullanım
var product = await _repository.GetByIdAsync(id, ct)
    ?? throw NotFoundException.For<Product>(id);
```

### 3.2 ValidationException

```csharp
/// <summary>
/// Input validation hatası.
/// HTTP Status: 400 Bad Request
/// </summary>
public class ValidationException : MergeException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base(
            "VALIDATION_ERROR",
            "One or more validation errors occurred",
            400)
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : this(new Dictionary<string, string[]>
        {
            [propertyName] = [errorMessage]
        })
    {
    }

    public static ValidationException FromFluentValidation(
        FluentValidation.Results.ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationException(errors);
    }
}
```

### 3.3 ConflictException

```csharp
/// <summary>
/// Concurrency veya duplicate conflict.
/// HTTP Status: 409 Conflict
/// </summary>
public class ConflictException : MergeException
{
    public ConflictException(string message)
        : base("CONFLICT", message, 409)
    {
    }
}

/// <summary>
/// Optimistic concurrency exception.
/// </summary>
public class ConcurrencyException : ConflictException
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public ConcurrencyException(string resourceType, object resourceId)
        : base($"{resourceType} '{resourceId}' was modified by another user. Please refresh and try again.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;

        WithMetadata("resourceType", resourceType);
        WithMetadata("resourceId", resourceId);
    }
}
```

### 3.4 Authentication/Authorization Exceptions

```csharp
/// <summary>
/// Authentication gerekli.
/// HTTP Status: 401 Unauthorized
/// </summary>
public class UnauthorizedException : MergeException
{
    public UnauthorizedException(string message = "Authentication is required")
        : base("UNAUTHORIZED", message, 401)
    {
    }
}

/// <summary>
/// Yetki yok.
/// HTTP Status: 403 Forbidden
/// </summary>
public class ForbiddenException : MergeException
{
    public ForbiddenException(string message = "You do not have permission to perform this action")
        : base("FORBIDDEN", message, 403)
    {
    }

    public ForbiddenException(string resource, string action)
        : base(
            "FORBIDDEN",
            $"You do not have permission to {action} this {resource}",
            403)
    {
        WithMetadata("resource", resource);
        WithMetadata("action", action);
    }
}
```

---

## 4. GLOBAL EXCEPTION HANDLER

### 4.1 IExceptionHandler Implementation

```csharp
/// <summary>
/// Global exception handler.
/// Tüm exception'ları RFC 7807 Problem Details formatına dönüştürür.
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
        var problemDetails = CreateProblemDetails(exception, httpContext);

        // Log
        LogException(exception, problemDetails);

        // Response
        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }

    private ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        var problemDetails = exception switch
        {
            ValidationException ex => CreateValidationProblemDetails(ex),
            MergeException ex => CreateMergeProblemDetails(ex),
            DbUpdateConcurrencyException => CreateConcurrencyProblemDetails(),
            OperationCanceledException => CreateCancellationProblemDetails(),
            _ => CreateInternalErrorProblemDetails(exception)
        };

        // Common properties
        problemDetails.Instance = context.Request.Path;
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.TraceIdentifier;

        return problemDetails;
    }

    private ValidationProblemDetails CreateValidationProblemDetails(ValidationException ex)
    {
        return new ValidationProblemDetails(ex.Errors)
        {
            Type = "https://api.merge.com/errors/validation",
            Title = "Validation Error",
            Status = 400,
            Detail = ex.Message
        };
    }

    private ProblemDetails CreateMergeProblemDetails(MergeException ex)
    {
        var problemDetails = new ProblemDetails
        {
            Type = $"https://api.merge.com/errors/{ex.ErrorCode.ToKebabCase()}",
            Title = ex.ErrorCode.ToTitleCase(),
            Status = ex.HttpStatusCode,
            Detail = ex.Message
        };

        foreach (var (key, value) in ex.Metadata)
        {
            problemDetails.Extensions[key.ToCamelCase()] = value;
        }

        return problemDetails;
    }

    private ProblemDetails CreateConcurrencyProblemDetails()
    {
        return new ProblemDetails
        {
            Type = "https://api.merge.com/errors/concurrency",
            Title = "Concurrency Conflict",
            Status = 409,
            Detail = "The resource was modified by another user. Please refresh and try again."
        };
    }

    private ProblemDetails CreateCancellationProblemDetails()
    {
        return new ProblemDetails
        {
            Type = "https://api.merge.com/errors/request-cancelled",
            Title = "Request Cancelled",
            Status = 499,
            Detail = "The request was cancelled by the client."
        };
    }

    private ProblemDetails CreateInternalErrorProblemDetails(Exception ex)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://api.merge.com/errors/internal",
            Title = "Internal Server Error",
            Status = 500
        };

        if (environment.IsDevelopment())
        {
            problemDetails.Detail = ex.Message;
            problemDetails.Extensions["stackTrace"] = ex.StackTrace;
            problemDetails.Extensions["exceptionType"] = ex.GetType().Name;
        }
        else
        {
            problemDetails.Detail = "An unexpected error occurred. Please try again later.";
        }

        return problemDetails;
    }

    private void LogException(Exception exception, ProblemDetails problemDetails)
    {
        var logLevel = problemDetails.Status switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        logger.Log(
            logLevel,
            exception,
            "HTTP {StatusCode} - {ErrorType}: {ErrorDetail}",
            problemDetails.Status,
            problemDetails.Title,
            problemDetails.Detail);
    }
}
```

### 4.2 Program.cs Registration

```csharp
// Exception handler registration
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Middleware pipeline
app.UseExceptionHandler();
```

---

## 5. RESULT PATTERN

### 5.1 Result Type

```csharp
/// <summary>
/// Operation result without value.
/// Exception fırlatmak yerine kullanılabilir.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

/// <summary>
/// Operation result with value.
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    internal Result(T value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Error representation.
/// </summary>
public record Error(string Code, string Message)
{
    public static Error NotFound(string resourceType, object id) =>
        new("NotFound", $"{resourceType} with ID '{id}' was not found");

    public static Error Validation(string message) =>
        new("Validation", message);

    public static Error Conflict(string message) =>
        new("Conflict", message);

    public static Error Unauthorized(string message = "Unauthorized") =>
        new("Unauthorized", message);

    public static Error Forbidden(string message = "Forbidden") =>
        new("Forbidden", message);
}
```

### 5.2 Result Pattern Kullanımı

```csharp
public class ProductService
{
    // Exception-based (tercih edilen - bu projede)
    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For<Product>(id);

        return _mapper.Map<ProductDto>(product);
    }

    // Result-based (alternatif)
    public async Task<Result<ProductDto>> GetByIdResultAsync(Guid id, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(id, ct);

        if (product is null)
            return Result.Failure<ProductDto>(Error.NotFound("Product", id));

        return _mapper.Map<ProductDto>(product);
    }
}

// Controller'da kullanım
[HttpGet("{id}")]
public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
{
    var result = await _service.GetByIdResultAsync(id, ct);

    if (result.IsFailure)
    {
        return result.Error!.Code switch
        {
            "NotFound" => NotFound(result.Error),
            "Validation" => BadRequest(result.Error),
            _ => StatusCode(500, result.Error)
        };
    }

    return Ok(result.Value);
}
```

---

## 6. VALIDATION ERRORS

### 6.1 FluentValidation Integration

```csharp
/// <summary>
/// MediatR validation pipeline behavior.
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
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            throw ValidationException.FromFluentValidation(
                new FluentValidation.Results.ValidationResult(failures));
        }

        return await next();
    }
}
```

### 6.2 Validation Problem Details

```json
{
  "type": "https://api.merge.com/errors/validation",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/products",
  "traceId": "00-abc123-def456-00",
  "errors": {
    "name": [
      "Name is required",
      "Name cannot exceed 200 characters"
    ],
    "price": [
      "Price must be positive"
    ],
    "sku": [
      "SKU already exists"
    ]
  }
}
```

---

## 7. HTTP STATUS CODE MAPPING

### 7.1 Exception to Status Code

| Exception | Status Code | Description |
|-----------|-------------|-------------|
| `ValidationException` | 400 | Bad Request |
| `UnauthorizedException` | 401 | Unauthorized |
| `ForbiddenException` | 403 | Forbidden |
| `NotFoundException` | 404 | Not Found |
| `ConflictException` | 409 | Conflict |
| `ConcurrencyException` | 409 | Conflict |
| `DomainException` | 422 | Unprocessable Entity |
| `IntegrationException` | 502 | Bad Gateway |
| `OperationCanceledException` | 499 | Client Closed Request |
| Unhandled | 500 | Internal Server Error |

### 7.2 Problem Details Types

```csharp
public static class ProblemTypes
{
    private const string BaseUrl = "https://api.merge.com/errors";

    public static string Validation => $"{BaseUrl}/validation";
    public static string NotFound => $"{BaseUrl}/not-found";
    public static string Unauthorized => $"{BaseUrl}/unauthorized";
    public static string Forbidden => $"{BaseUrl}/forbidden";
    public static string Conflict => $"{BaseUrl}/conflict";
    public static string Concurrency => $"{BaseUrl}/concurrency";
    public static string BusinessRule => $"{BaseUrl}/business-rule";
    public static string Integration => $"{BaseUrl}/integration";
    public static string Internal => $"{BaseUrl}/internal";

    public static string FromCode(string errorCode) =>
        $"{BaseUrl}/{errorCode.ToKebabCase()}";
}
```

---

## 8. LOGGING ERRORS

### 8.1 Error Logging Pattern

```csharp
// ✅ DOĞRU: Exception ile error log
try
{
    await ProcessOrderAsync(orderId, ct);
}
catch (InsufficientStockException ex)
{
    // Business rule - Warning level
    logger.LogWarning(
        "Order failed - Insufficient stock. Product: {ProductId}, Requested: {Requested}, Available: {Available}",
        ex.ProductId,
        ex.RequestedQuantity,
        ex.AvailableStock);
    throw;
}
catch (Exception ex)
{
    // Unexpected error - Error level
    logger.LogError(ex,
        "Unexpected error processing order {OrderId}",
        orderId);
    throw;
}

// ❌ YANLIŞ: Exception'ı string olarak log
catch (Exception ex)
{
    logger.LogError(ex.ToString()); // Stack trace readable değil
}
```

### 8.2 Correlation ID

```csharp
// Her error log'unda correlation ID olmalı
public class OrderService(ILogger<OrderService> logger)
{
    public async Task ProcessAsync(Guid orderId, string correlationId, CancellationToken ct)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["OrderId"] = orderId
        }))
        {
            try
            {
                await ProcessInternalAsync(orderId, ct);
            }
            catch (Exception ex)
            {
                // CorrelationId otomatik olarak log'a eklenir
                logger.LogError(ex, "Order processing failed");
                throw;
            }
        }
    }
}
```

---

## 9. RETRY PATTERNS

### 9.1 Polly Retry Policy

```csharp
// Transient error'lar için retry
builder.Services.AddHttpClient<IPaymentGateway, PaymentGateway>()
    .AddPolicyHandler(Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => r.StatusCode >= HttpStatusCode.InternalServerError)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt =>
                TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100),
            onRetry: (outcome, delay, attempt, context) =>
            {
                var logger = context.GetLogger();
                logger.LogWarning(
                    "Retry {Attempt} after {Delay}ms. Status: {StatusCode}",
                    attempt,
                    delay.TotalMilliseconds,
                    outcome.Result?.StatusCode);
            }));
```

### 9.2 Database Retry

```csharp
// EF Core retry policy
options.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.EnableRetryOnFailure(
        maxRetryCount: 3,
        maxRetryDelay: TimeSpan.FromSeconds(5),
        errorCodesToAdd: null);
});
```

---

## 10. BEST PRACTICES

### 10.1 Exception Handling Rules

```csharp
// ✅ DOĞRU: Specific exception catch
try
{
    await _repository.AddAsync(product, ct);
    await _unitOfWork.SaveChangesAsync(ct);
}
catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
{
    throw new DuplicateSkuException(product.SKU.Value);
}

// ❌ YANLIŞ: Catch-all
try
{
    // ...
}
catch (Exception) // Tüm exception'ları yutar
{
    return null;
}

// ✅ DOĞRU: Rethrow with context
catch (Exception ex)
{
    logger.LogError(ex, "Failed to create product {SKU}", product.SKU);
    throw; // Stack trace korunur
}

// ❌ YANLIŞ: throw ex (stack trace kaybolur)
catch (Exception ex)
{
    throw ex; // Stack trace sıfırlanır!
}
```

### 10.2 Guard Clauses

```csharp
public class Product
{
    public static Product Create(
        string name,
        string description,
        SKU sku,
        Money price,
        int stockQuantity,
        Guid categoryId)
    {
        // Guard clauses - erken fail
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.Null(sku, nameof(sku));
        Guard.Against.Null(price, nameof(price));
        Guard.Against.Negative(stockQuantity, nameof(stockQuantity));
        Guard.Against.Default(categoryId, nameof(categoryId));

        // Business rule
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        return new Product
        {
            // ...
        };
    }
}
```

### 10.3 Try-Parse Pattern

```csharp
// Exception fırlatmak yerine false dön
public static bool TryCreate(string value, out Email? email)
{
    email = null;

    if (string.IsNullOrWhiteSpace(value))
        return false;

    if (!IsValidEmail(value))
        return false;

    email = new Email(value.ToLowerInvariant());
    return true;
}

// Kullanım
if (!Email.TryCreate(input, out var email))
{
    return Result.Failure<User>(Error.Validation("Invalid email format"));
}
```

---

## ERROR HANDLING CHECKLIST

- [ ] Custom exception hiyerarşisi tanımlı
- [ ] Global exception handler aktif
- [ ] RFC 7807 Problem Details formatı kullanılıyor
- [ ] Validation exception'lar 400 dönüyor
- [ ] NotFound exception'lar 404 dönüyor
- [ ] Domain exception'lar 422 dönüyor
- [ ] Error'lar doğru seviyede loglanıyor
- [ ] Sensitive data exception message'larında yok
- [ ] Correlation ID her error'da var
- [ ] Production'da stack trace gizli

---

*Bu kural dosyası, Merge E-Commerce Backend projesi için hata yönetim standartlarını belirler.*
