---
paths:
  - "**/*.cs"
---

# LOGGING KURALLARI (ULTRA KAPSAMLI)

> Bu dosya, Merge E-Commerce Backend projesinde logging için
> kapsamlı kurallar ve en iyi uygulamaları içerir.

---

## İÇİNDEKİLER

1. [Temel Kurallar](#1-temel-kurallar)
2. [Log Seviyeleri](#2-log-seviyeleri)
3. [Structured Logging](#3-structured-logging)
4. [Logging Patterns](#4-logging-patterns)
5. [Sensitive Data](#5-sensitive-data)
6. [Performance](#6-performance)
7. [Correlation](#7-correlation)
8. [Serilog Configuration](#8-serilog-configuration)

---

## 1. TEMEL KURALLAR

### 1.1 Logger Injection

```csharp
// ✅ DOĞRU: Primary constructor ile injection
public class ProductService(
    IRepository<Product> repository,
    ILogger<ProductService> logger) // Generic logger
{
    public async Task<ProductDto> CreateAsync(CreateProductCommand cmd, CancellationToken ct)
    {
        logger.LogInformation(
            "Creating product {ProductName} in category {CategoryId}",
            cmd.Name, cmd.CategoryId);
        // ...
    }
}

// ❌ YANLIŞ: Static logger
public class BadService
{
    private static readonly ILogger _logger = LogManager.GetLogger(typeof(BadService));
}
```

### 1.2 Message Template Syntax

```csharp
// ✅ DOĞRU: Placeholder ile structured logging
logger.LogInformation(
    "Order {OrderId} created for user {UserId} with total {Total:C}",
    order.Id,
    order.UserId,
    order.TotalAmount);

// ❌ YANLIŞ: String interpolation
logger.LogInformation(
    $"Order {order.Id} created for user {order.UserId}"); // Searchable değil!

// ❌ YANLIŞ: String concatenation
logger.LogInformation(
    "Order " + order.Id + " created"); // GC pressure + searchable değil!
```

---

## 2. LOG SEVİYELERİ

### 2.1 Seviye Kullanım Kılavuzu

| Seviye | Kullanım | Örnek |
|--------|----------|-------|
| `Trace` | Çok detaylı debug bilgisi | Method entry/exit, variable values |
| `Debug` | Geliştirme sırasında debugging | Query parameters, cache hit/miss |
| `Information` | Normal operasyonlar | User login, order created |
| `Warning` | Beklenmeyen ama handle edilen durumlar | Retry, cache miss, deprecated API |
| `Error` | Hatalar (işlem başarısız) | Exception, validation failure |
| `Critical` | Sistem çökmesi | Database connection lost, out of memory |

### 2.2 Doğru Seviye Seçimi

```csharp
// TRACE: Çok detaylı (production'da kapalı)
logger.LogTrace(
    "Executing query with parameters: {Parameters}",
    JsonSerializer.Serialize(parameters));

// DEBUG: Geliştirme debugging
logger.LogDebug(
    "Cache {CacheKey} - Hit: {IsHit}",
    cacheKey, cacheHit);

// INFORMATION: Normal operasyonlar
logger.LogInformation(
    "Order {OrderId} created successfully for user {UserId}",
    order.Id, order.UserId);

// WARNING: Dikkat gerektiren durumlar
logger.LogWarning(
    "Retry attempt {Attempt} for payment {PaymentId}",
    attemptNumber, paymentId);

// ERROR: Hatalar
logger.LogError(exception,
    "Failed to process order {OrderId}",
    orderId);

// CRITICAL: Sistem kritik
logger.LogCritical(exception,
    "Database connection lost. Application cannot continue.");
```

---

## 3. STRUCTURED LOGGING

### 3.1 Property Naming

```csharp
// ✅ DOĞRU: PascalCase, anlamlı isimler
logger.LogInformation(
    "Product {ProductId} price updated from {OldPrice:C} to {NewPrice:C}",
    productId,
    oldPrice,
    newPrice);

// ✅ DOĞRU: @ prefix ile object destructuring
logger.LogInformation(
    "Order created: {@Order}",
    new { order.Id, order.UserId, order.TotalAmount });

// ✅ DOĞRU: $ prefix ile stringify
logger.LogInformation(
    "Processing request: {$Request}",
    request); // ToString() çağrılır

// ❌ YANLIŞ: camelCase, kısa isimler
logger.LogInformation("Product {id} updated", productId);
```

### 3.2 Consistent Property Names

```csharp
// Proje genelinde tutarlı property isimleri kullan:
public static class LogProperties
{
    // Entity IDs
    public const string ProductId = "ProductId";
    public const string OrderId = "OrderId";
    public const string UserId = "UserId";
    public const string CategoryId = "CategoryId";

    // Operations
    public const string OperationType = "OperationType";
    public const string Duration = "DurationMs";
    public const string Status = "Status";

    // Context
    public const string CorrelationId = "CorrelationId";
    public const string RequestPath = "RequestPath";
    public const string HttpMethod = "HttpMethod";
}

// Kullanım
logger.LogInformation(
    "Product {" + LogProperties.ProductId + "} retrieved in {" + LogProperties.Duration + "}ms",
    productId,
    stopwatch.ElapsedMilliseconds);
```

---

## 4. LOGGING PATTERNS

### 4.1 Method Entry/Exit (Trace Level)

```csharp
public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken ct)
{
    logger.LogTrace("Entering {Method} with {ProductId}", nameof(GetByIdAsync), id);

    try
    {
        var product = await _repository.GetByIdAsync(id, ct);

        logger.LogTrace(
            "Exiting {Method} - Found: {Found}",
            nameof(GetByIdAsync),
            product is not null);

        return _mapper.Map<ProductDto>(product);
    }
    catch (Exception ex)
    {
        logger.LogTrace(ex, "Exiting {Method} with exception", nameof(GetByIdAsync));
        throw;
    }
}
```

### 4.2 Operation Logging

```csharp
public async Task<OrderDto> CreateOrderAsync(CreateOrderCommand cmd, CancellationToken ct)
{
    // Operation start
    logger.LogInformation(
        "Creating order for user {UserId} with {ItemCount} items",
        cmd.UserId,
        cmd.Items.Count);

    var stopwatch = Stopwatch.StartNew();

    try
    {
        var order = await CreateOrderInternalAsync(cmd, ct);

        // Operation success
        logger.LogInformation(
            "Order {OrderId} created successfully. Total: {Total:C}. Duration: {Duration}ms",
            order.Id,
            order.TotalAmount,
            stopwatch.ElapsedMilliseconds);

        return _mapper.Map<OrderDto>(order);
    }
    catch (InsufficientStockException ex)
    {
        // Business rule violation (Warning)
        logger.LogWarning(
            "Order creation failed - Insufficient stock for product {ProductId}. Requested: {Requested}, Available: {Available}",
            ex.ProductId,
            ex.RequestedQuantity,
            ex.AvailableStock);
        throw;
    }
    catch (Exception ex)
    {
        // Unexpected error
        logger.LogError(ex,
            "Order creation failed for user {UserId}. Duration: {Duration}ms",
            cmd.UserId,
            stopwatch.ElapsedMilliseconds);
        throw;
    }
}
```

### 4.3 Loop Logging

```csharp
public async Task ProcessOrdersAsync(IEnumerable<Order> orders, CancellationToken ct)
{
    var orderList = orders.ToList();

    logger.LogInformation(
        "Starting batch processing of {OrderCount} orders",
        orderList.Count);

    var successCount = 0;
    var failCount = 0;

    foreach (var order in orderList)
    {
        try
        {
            await ProcessSingleOrderAsync(order, ct);
            successCount++;

            // Her N. item için progress log (çok fazla log basma)
            if (successCount % 100 == 0)
            {
                logger.LogDebug(
                    "Processed {Processed}/{Total} orders",
                    successCount,
                    orderList.Count);
            }
        }
        catch (Exception ex)
        {
            failCount++;
            logger.LogError(ex,
                "Failed to process order {OrderId}",
                order.Id);
        }
    }

    logger.LogInformation(
        "Batch processing completed. Success: {Success}, Failed: {Failed}",
        successCount,
        failCount);
}
```

### 4.4 Conditional Logging

```csharp
// IsEnabled check ile performance optimization
if (logger.IsEnabled(LogLevel.Debug))
{
    // Expensive operation sadece debug aktifse çalışır
    var serialized = JsonSerializer.Serialize(largeObject, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    logger.LogDebug("Request payload: {Payload}", serialized);
}
```

---

## 5. SENSITIVE DATA

### 5.1 Asla Loglanmaması Gerekenler

```csharp
// ❌ ASLA LOGLAMA:
// - Passwords
// - Credit card numbers
// - CVV codes
// - Access tokens / API keys
// - Personal health information
// - Government IDs (TC Kimlik No, SSN)
// - Full email addresses (mask it)
// - Full phone numbers (mask it)
// - IP addresses (GDPR)

// ❌ YANLIŞ: Token loglama
logger.LogInformation(
    "User {UserId} logged in with token {Token}",
    userId,
    accessToken); // GÜVENLİK AÇIĞI!

// ✅ DOĞRU: Token'ı maskele
logger.LogInformation(
    "User {UserId} logged in. Token prefix: {TokenPrefix}",
    userId,
    accessToken[..8] + "...");
```

### 5.2 Data Masking Helper

```csharp
public static class LogMasking
{
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return "[empty]";

        var atIndex = email.IndexOf('@');
        if (atIndex <= 2) return "***@***";

        return email[..2] + "***" + email[(atIndex - 1)..];
    }

    public static string MaskPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4) return "[masked]";
        return "***" + phone[^4..];
    }

    public static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 8) return "[masked]";
        return cardNumber[..4] + " **** **** " + cardNumber[^4..];
    }

    public static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length < 12) return "[masked]";
        return token[..8] + "...";
    }
}

// Kullanım
logger.LogInformation(
    "Payment processed for card {MaskedCard}",
    LogMasking.MaskCardNumber(cardNumber));
```

### 5.3 Serilog Destructuring Policy

```csharp
// Sensitive property'leri otomatik maskele
public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "PasswordHash", "Token", "AccessToken", "RefreshToken",
        "ApiKey", "Secret", "CreditCard", "CardNumber", "Cvv", "Pin"
    };

    public bool TryDestructure(object value, ILogEventPropertyValueFactory factory,
        out LogEventPropertyValue result)
    {
        result = null!;

        if (value is null) return false;

        var type = value.GetType();
        var properties = new List<LogEventProperty>();

        foreach (var prop in type.GetProperties())
        {
            var propValue = prop.GetValue(value);

            if (SensitiveProperties.Contains(prop.Name))
            {
                properties.Add(new LogEventProperty(prop.Name,
                    factory.CreatePropertyValue("[REDACTED]")));
            }
            else
            {
                properties.Add(new LogEventProperty(prop.Name,
                    factory.CreatePropertyValue(propValue, true)));
            }
        }

        result = new StructureValue(properties);
        return true;
    }
}
```

---

## 6. PERFORMANCE

### 6.1 Avoid Expensive Operations

```csharp
// ❌ YANLIŞ: Her log için serialization
logger.LogDebug(
    "Request: {Request}",
    JsonSerializer.Serialize(request)); // Her çağrıda serialize!

// ✅ DOĞRU: IsEnabled check
if (logger.IsEnabled(LogLevel.Debug))
{
    logger.LogDebug(
        "Request: {Request}",
        JsonSerializer.Serialize(request));
}

// ✅ DOĞRU: LoggerMessage.Define ile source generation
public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Product {ProductId} created with name {ProductName}")]
    public static partial void ProductCreated(
        this ILogger logger,
        Guid productId,
        string productName);
}

// Kullanım
logger.ProductCreated(product.Id, product.Name);
```

### 6.2 High-Performance Logging

```csharp
// Source-generated logging (en performanslı)
public static partial class HighPerfLogs
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Order {OrderId} processed in {Duration}ms")]
    public static partial void OrderProcessed(
        this ILogger logger,
        Guid orderId,
        long duration);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Cache miss for key {CacheKey}")]
    public static partial void CacheMiss(
        this ILogger logger,
        string cacheKey);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Failed to process payment {PaymentId}")]
    public static partial void PaymentFailed(
        this ILogger logger,
        Guid paymentId,
        Exception exception);
}
```

---

## 7. CORRELATION

### 7.1 Correlation ID Middleware

```csharp
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        // Serilog enricher ile context'e ekle
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("HttpMethod", context.Request.Method))
        {
            await next(context);
        }
    }
}
```

### 7.2 Log Scopes

```csharp
public async Task<OrderDto> ProcessOrderAsync(Guid orderId, CancellationToken ct)
{
    using (logger.BeginScope(new Dictionary<string, object>
    {
        ["OrderId"] = orderId,
        ["Operation"] = "ProcessOrder"
    }))
    {
        logger.LogInformation("Starting order processing");

        // Bu scope içindeki tüm log'lar OrderId ve Operation içerir
        await ValidateOrderAsync(orderId, ct);
        await ProcessPaymentAsync(orderId, ct);
        await UpdateInventoryAsync(orderId, ct);

        logger.LogInformation("Order processing completed");
    }
}
```

---

## 8. SERILOG CONFIGURATION

### 8.1 Program.cs Configuration

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "Merge.API")
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq("http://localhost:5341")
    .WriteTo.File(
        path: "logs/merge-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        formatter: new JsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();
```

### 8.2 appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "apiKey": ""
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithEnvironmentName"],
    "Properties": {
      "Application": "Merge.API"
    }
  }
}
```

---

## LOGGING CHECKLIST

- [ ] Generic ILogger<T> kullanılıyor
- [ ] Message template syntax kullanılıyor (interpolation değil)
- [ ] Doğru log seviyesi seçilmiş
- [ ] Sensitive data maskeleniyor
- [ ] CorrelationId her log'da var
- [ ] Performance-critical yerlerde IsEnabled check var
- [ ] Exception'lar error level'da loglanıyor
- [ ] Business rule violation'lar warning level'da
- [ ] Success durumları information level'da

---

*Bu kural dosyası, Merge E-Commerce Backend projesi için logging standartlarını belirler.*
