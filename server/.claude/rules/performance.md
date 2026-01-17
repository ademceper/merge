---
paths:
  - "**/*.cs"
---

# PERFORMANS KURALLARI (ULTRA KAPSAMLI)

> Bu dosya, Merge E-Commerce Backend projesinde performans optimizasyonu için
> kapsamlı kurallar ve en iyi uygulamaları içerir.

---

## İÇİNDEKİLER

1. [Async/Await Best Practices](#1-asyncawait-best-practices)
2. [Memory Management](#2-memory-management)
3. [Database Performance](#3-database-performance)
4. [Caching Strategies](#4-caching-strategies)
5. [HTTP Performance](#5-http-performance)
6. [String Handling](#6-string-handling)
7. [Collection Performance](#7-collection-performance)
8. [LINQ Optimizations](#8-linq-optimizations)
9. [Pooling](#9-pooling)
10. [Profiling & Monitoring](#10-profiling--monitoring)

---

## 1. ASYNC/AWAIT BEST PRACTICES

### 1.1 Async All The Way

```csharp
// ✅ DOĞRU: Async zinciri kırılmıyor
public async Task<ProductDto> GetProductAsync(Guid id, CancellationToken ct)
{
    var product = await _repository.GetByIdAsync(id, ct);
    var dto = await _mapper.MapAsync<ProductDto>(product, ct);
    return dto;
}

// ❌ YANLIŞ: .Result ile blocking
public ProductDto GetProduct(Guid id)
{
    var product = _repository.GetByIdAsync(id).Result; // DEADLOCK riski!
    return _mapper.Map<ProductDto>(product);
}

// ❌ YANLIŞ: .Wait() ile blocking
public void ProcessOrder(Guid orderId)
{
    _orderService.ProcessAsync(orderId).Wait(); // DEADLOCK riski!
}
```

### 1.2 ConfigureAwait

```csharp
// Library code'da context capture'ı önle
public async Task<T> GetCachedAsync<T>(string key, CancellationToken ct)
{
    var cached = await _cache.GetStringAsync(key, ct).ConfigureAwait(false);

    if (cached is null) return default!;

    return JsonSerializer.Deserialize<T>(cached)!;
}

// ASP.NET Core controller'larda ConfigureAwait KULLANMA
// HttpContext'e erişim gerekebilir
public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct)
{
    var product = await _service.GetByIdAsync(id, ct); // ConfigureAwait yok
    return Ok(product);
}
```

### 1.3 ValueTask Kullanımı

```csharp
// Cache hit durumunda allocation'ı önle
public ValueTask<Product?> GetByIdAsync(Guid id, CancellationToken ct)
{
    // Cache'de varsa senkron dön
    if (_memoryCache.TryGetValue<Product>($"product:{id}", out var cached))
    {
        return ValueTask.FromResult<Product?>(cached);
    }

    // Cache'de yoksa async çalış
    return new ValueTask<Product?>(GetFromDatabaseAsync(id, ct));
}

private async Task<Product?> GetFromDatabaseAsync(Guid id, CancellationToken ct)
{
    var product = await _context.Products.FindAsync([id], ct);

    if (product is not null)
    {
        _memoryCache.Set($"product:{id}", product, TimeSpan.FromMinutes(5));
    }

    return product;
}
```

### 1.4 Paralel İşlemler

```csharp
// ✅ DOĞRU: Bağımsız işlemleri paralel çalıştır
public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct)
{
    // Paralel başlat
    var ordersTask = _orderService.GetRecentOrdersAsync(ct);
    var productsTask = _productService.GetTopProductsAsync(ct);
    var statsTask = _statsService.GetDailyStatsAsync(ct);

    // Hepsini bekle
    await Task.WhenAll(ordersTask, productsTask, statsTask);

    return new DashboardDto
    {
        RecentOrders = await ordersTask,
        TopProducts = await productsTask,
        DailyStats = await statsTask
    };
}

// ✅ DOĞRU: Sınırlı paralellik ile batch işlem
public async Task ProcessOrdersAsync(IEnumerable<Order> orders, CancellationToken ct)
{
    var options = new ParallelOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        CancellationToken = ct
    };

    await Parallel.ForEachAsync(orders, options, async (order, token) =>
    {
        await ProcessSingleOrderAsync(order, token);
    });
}
```

### 1.5 CancellationToken Kullanımı

```csharp
// Her async method CancellationToken almalı
public async Task<PagedResult<ProductDto>> GetProductsAsync(
    int page,
    int pageSize,
    CancellationToken ct) // ZORUNLU
{
    ct.ThrowIfCancellationRequested(); // Erken çıkış

    var query = _context.Products
        .AsNoTracking()
        .Where(p => p.IsActive);

    var total = await query.CountAsync(ct);

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);

    return new PagedResult<ProductDto>(items, total, page, pageSize);
}
```

---

## 2. MEMORY MANAGEMENT

### 2.1 Span<T> ve Memory<T>

```csharp
// ✅ DOĞRU: Span ile allocation-free substring
public static bool IsValidSku(ReadOnlySpan<char> sku)
{
    if (sku.Length < 5 || sku.Length > 20)
        return false;

    foreach (var c in sku)
    {
        if (!char.IsLetterOrDigit(c) && c != '-')
            return false;
    }

    return true;
}

// ✅ DOĞRU: stackalloc ile küçük buffer'lar
public string GenerateCode()
{
    Span<char> buffer = stackalloc char[8];

    for (int i = 0; i < buffer.Length; i++)
    {
        buffer[i] = _chars[Random.Shared.Next(_chars.Length)];
    }

    return new string(buffer);
}

// ✅ DOĞRU: ArrayPool kullanımı
public async Task ProcessLargeDataAsync(Stream stream, CancellationToken ct)
{
    byte[] buffer = ArrayPool<byte>.Shared.Rent(81920);

    try
    {
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, ct)) > 0)
        {
            ProcessChunk(buffer.AsSpan(0, bytesRead));
        }
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
```

### 2.2 Object Pooling

```csharp
// StringBuilder pooling
public class StringBuilderPool
{
    private static readonly ObjectPool<StringBuilder> Pool =
        new DefaultObjectPoolProvider().CreateStringBuilderPool();

    public static string Build(Action<StringBuilder> builder)
    {
        var sb = Pool.Get();
        try
        {
            builder(sb);
            return sb.ToString();
        }
        finally
        {
            sb.Clear();
            Pool.Return(sb);
        }
    }
}

// Kullanımı
var result = StringBuilderPool.Build(sb =>
{
    sb.Append("Hello, ");
    sb.Append(name);
    sb.Append("!");
});
```

### 2.3 Dispose Pattern

```csharp
// IAsyncDisposable implementasyonu
public class ProductImporter : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Stream _fileStream;
    private bool _disposed;

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _httpClient.Dispose();

        if (_fileStream is not null)
            await _fileStream.DisposeAsync();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

// using ile kullanım
await using var importer = new ProductImporter();
await importer.ImportAsync(ct);
```

---

## 3. DATABASE PERFORMANCE

### 3.1 Query Optimization

```csharp
// ✅ DOĞRU: AsNoTracking + Projection
public async Task<List<ProductListDto>> GetActiveProductsAsync(CancellationToken ct)
{
    return await _context.Products
        .AsNoTracking()
        .Where(p => p.IsActive)
        .Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price.Amount,
            ImageUrl = p.Images
                .Where(i => i.IsPrimary)
                .Select(i => i.Url)
                .FirstOrDefault()
        })
        .ToListAsync(ct);
}

// ✅ DOĞRU: Compiled queries
private static readonly Func<ApplicationDbContext, Guid, CancellationToken, Task<Product?>>
    GetByIdQuery = EF.CompileAsyncQuery(
        (ApplicationDbContext ctx, Guid id, CancellationToken ct) =>
            ctx.Products.FirstOrDefault(p => p.Id == id));

public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
    => GetByIdQuery(_context, id, ct);
```

### 3.2 Batch Operations

```csharp
// ✅ DOĞRU: ExecuteUpdate ile toplu güncelleme
public async Task<int> DeactivateExpiredProductsAsync(CancellationToken ct)
{
    return await _context.Products
        .Where(p => p.ExpirationDate < DateTime.UtcNow && p.IsActive)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.IsActive, false)
            .SetProperty(p => p.LastModifiedAt, DateTime.UtcNow),
            ct);
}

// ✅ DOĞRU: ExecuteDelete ile toplu silme
public async Task<int> PurgeOldLogsAsync(DateTime cutoff, CancellationToken ct)
{
    return await _context.AuditLogs
        .Where(l => l.CreatedAt < cutoff)
        .ExecuteDeleteAsync(ct);
}

// ✅ DOĞRU: Chunk ile batch insert
public async Task InsertProductsAsync(IEnumerable<Product> products, CancellationToken ct)
{
    foreach (var chunk in products.Chunk(1000))
    {
        await _context.Products.AddRangeAsync(chunk, ct);
        await _context.SaveChangesAsync(ct);
    }
}
```

### 3.3 Index Kullanımı

```csharp
// Her sorguda index kullanıldığından emin ol
// EXPLAIN ANALYZE ile kontrol et

/*
 * Index Önerileri:
 *
 * 1. WHERE koşullarındaki kolonlar
 * 2. JOIN kolonları (foreign key'ler)
 * 3. ORDER BY kolonları
 * 4. SELECT'te sık kullanılan kolonlar (covering index)
 *
 * İYİ: CategoryId + IsActive + CreatedAt composite index
 *      → WHERE CategoryId = x AND IsActive = true ORDER BY CreatedAt
 *
 * KÖTÜ: Her kolon için ayrı index
 *       → INSERT/UPDATE yavaşlar
 */

// Entity configuration'da index tanımla
builder.HasIndex(p => new { p.CategoryId, p.IsActive, p.CreatedAt })
    .HasDatabaseName("ix_products_category_active_created")
    .HasFilter("is_deleted = false");
```

---

## 4. CACHING STRATEGIES

### 4.1 Multi-Level Caching

```csharp
/// <summary>
/// L1: In-Memory Cache (ms latency)
/// L2: Distributed Cache - Redis (ms-10ms latency)
/// L3: Database (10-100ms latency)
/// </summary>
public class MultiLevelCache<T> where T : class
{
    private readonly IMemoryCache _l1;
    private readonly IDistributedCache _l2;
    private readonly TimeSpan _l1Duration = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _l2Duration = TimeSpan.FromMinutes(10);

    public async Task<T?> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        CancellationToken ct)
    {
        // L1: Memory cache
        if (_l1.TryGetValue<T>(key, out var cached))
            return cached;

        // L2: Redis cache
        var l2Key = $"cache:{key}";
        var l2Value = await _l2.GetStringAsync(l2Key, ct);

        if (l2Value is not null)
        {
            var fromL2 = JsonSerializer.Deserialize<T>(l2Value);
            _l1.Set(key, fromL2, _l1Duration);
            return fromL2;
        }

        // L3: Database
        var value = await factory(ct);

        if (value is not null)
        {
            // L2'ye yaz
            await _l2.SetStringAsync(
                l2Key,
                JsonSerializer.Serialize(value),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _l2Duration
                },
                ct);

            // L1'e yaz
            _l1.Set(key, value, _l1Duration);
        }

        return value;
    }
}
```

### 4.2 Cache Stampede Prevention

```csharp
/// <summary>
/// Cache stampede'i önlemek için lock mekanizması.
/// Aynı key için aynı anda sadece bir factory çalışır.
/// </summary>
public class StampedeProtectedCache(IDistributedCache cache)
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan expiration,
        CancellationToken ct)
    {
        // Önce cache'e bak
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<T>(cached);

        // Lock al
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);

        try
        {
            // Double-check
            cached = await cache.GetStringAsync(key, ct);
            if (cached is not null)
                return JsonSerializer.Deserialize<T>(cached);

            // Factory çalıştır
            var value = await factory(ct);

            if (value is not null)
            {
                await cache.SetStringAsync(
                    key,
                    JsonSerializer.Serialize(value),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration
                    },
                    ct);
            }

            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
```

### 4.3 Cache Invalidation

```csharp
/// <summary>
/// Pattern-based cache invalidation.
/// </summary>
public class CacheInvalidator(IDistributedCache cache, IConnectionMultiplexer redis)
{
    public async Task InvalidateByPatternAsync(string pattern, CancellationToken ct)
    {
        var server = redis.GetServer(redis.GetEndPoints().First());

        // Redis'ten pattern'e uyan key'leri bul
        var keys = server.Keys(pattern: pattern).ToList();

        if (keys.Count == 0) return;

        var db = redis.GetDatabase();

        // Batch delete
        var tasks = keys.Select(key => db.KeyDeleteAsync(key));
        await Task.WhenAll(tasks);
    }

    // Kullanımı
    // await _invalidator.InvalidateByPatternAsync("products:*", ct);  // Tüm ürün cache'i
    // await _invalidator.InvalidateByPatternAsync($"products:{categoryId}:*", ct);  // Kategori bazlı
}
```

---

## 5. HTTP PERFORMANCE

### 5.1 Response Compression

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/json", "text/plain", "text/html"]);
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

app.UseResponseCompression();
```

### 5.2 Response Caching

```csharp
// Output caching (server-side)
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromMinutes(5)));

    options.AddPolicy("Products", builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("page", "pageSize", "category")
        .Tag("products"));
});

// Controller'da kullanım
[HttpGet]
[OutputCache(PolicyName = "Products")]
public async Task<IActionResult> GetProducts([FromQuery] int page = 1)
{
    // ...
}

// Cache invalidation
[HttpPost]
public async Task<IActionResult> CreateProduct(...)
{
    await _cache.EvictByTagAsync("products", ct);
    // ...
}
```

### 5.3 HTTP Client Optimization

```csharp
// Typed HttpClient with Polly
builder.Services.AddHttpClient<IPaymentGateway, PaymentGateway>(client =>
{
    client.BaseAddress = new Uri("https://api.payment.com");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
    MaxConnectionsPerServer = 100
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100));

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
```

---

## 6. STRING HANDLING

### 6.1 String Interpolation vs StringBuilder

```csharp
// ✅ DOĞRU: Küçük string'ler için interpolation
var message = $"Product {product.Name} created with ID {product.Id}";

// ✅ DOĞRU: Çok sayıda concatenation için StringBuilder
var sb = new StringBuilder();
foreach (var item in items)
{
    sb.AppendLine($"- {item.Name}: {item.Price:C}");
}
var report = sb.ToString();

// ✅ DOĞRU: string.Create ile allocation-efficient
var result = string.Create(prefix.Length + id.Length + suffix.Length, (prefix, id, suffix),
    (span, state) =>
    {
        state.prefix.AsSpan().CopyTo(span);
        state.id.AsSpan().CopyTo(span[state.prefix.Length..]);
        state.suffix.AsSpan().CopyTo(span[(state.prefix.Length + state.id.Length)..]);
    });
```

### 6.2 String Comparison

```csharp
// ✅ DOĞRU: Case-insensitive comparison
if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
{
    // ...
}

// ✅ DOĞRU: Dictionary key comparison
var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

// ❌ YANLIŞ: ToLower() ile comparison (allocation)
if (a.ToLower() == b.ToLower()) // GC pressure!
{
    // ...
}
```

---

## 7. COLLECTION PERFORMANCE

### 7.1 Doğru Collection Seçimi

```csharp
// List<T>: Index-based access, sıralı iteration
// HashSet<T>: Unique items, hızlı contains check
// Dictionary<K,V>: Key-value lookup
// Queue<T>: FIFO operations
// Stack<T>: LIFO operations
// LinkedList<T>: Frequent insert/remove in middle

// ✅ DOĞRU: Contains kontrolü için HashSet
var processedIds = new HashSet<Guid>();
foreach (var order in orders)
{
    if (processedIds.Contains(order.Id)) continue;
    processedIds.Add(order.Id);
    // Process order
}

// ✅ DOĞRU: Capacity belirleme
var products = new List<Product>(expectedCount);
var lookup = new Dictionary<Guid, Product>(expectedCount);
```

### 7.2 IEnumerable vs IList

```csharp
// ✅ DOĞRU: Tek iteration için IEnumerable
public IEnumerable<Product> GetActiveProducts()
{
    return _products.Where(p => p.IsActive);
}

// ✅ DOĞRU: Multiple iteration veya Count gerekiyorsa materialize et
public async Task ProcessProductsAsync(IEnumerable<Product> products, CancellationToken ct)
{
    var productList = products as IList<Product> ?? products.ToList();

    _logger.LogInformation("Processing {Count} products", productList.Count);

    foreach (var product in productList)
    {
        await ProcessAsync(product, ct);
    }
}
```

---

## 8. LINQ OPTIMIZATIONS

### 8.1 Deferred Execution

```csharp
// ✅ DOĞRU: Sorguyu materialize etmeden filtrele
var activeProducts = _context.Products
    .Where(p => p.IsActive)
    .Where(p => p.Price.Amount > 100); // Hala IQueryable

// Materialize
var list = await activeProducts.ToListAsync(ct);

// ❌ YANLIŞ: Erken materialize
var allProducts = await _context.Products.ToListAsync(ct); // Tüm data!
var filtered = allProducts.Where(p => p.IsActive); // Memory'de filtreleme
```

### 8.2 Any vs Count

```csharp
// ✅ DOĞRU: Varlık kontrolü için Any
if (await _context.Products.AnyAsync(p => p.SKU == sku, ct))
{
    throw new DuplicateSkuException(sku);
}

// ❌ YANLIŞ: Count ile varlık kontrolü
if (await _context.Products.CountAsync(p => p.SKU == sku, ct) > 0) // Tüm satırları sayar!
{
    throw new DuplicateSkuException(sku);
}
```

### 8.3 First vs Single

```csharp
// FirstOrDefault: 0 veya daha fazla sonuç bekleniyor, ilkini al
var product = await _context.Products
    .FirstOrDefaultAsync(p => p.CategoryId == categoryId, ct);

// SingleOrDefault: Tam olarak 0 veya 1 sonuç bekleniyor
var user = await _context.Users
    .SingleOrDefaultAsync(u => u.Email.Value == email, ct);
// Birden fazla sonuç varsa exception fırlatır

// Find: Primary key ile lookup (önce cache'e bakar)
var product = await _context.Products.FindAsync([id], ct);
```

---

## 9. POOLING

### 9.1 DbContext Pooling

```csharp
// Program.cs - DbContext pooling
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
}, poolSize: 128); // Default: 1024
```

### 9.2 HttpClient Factory

```csharp
// Program.cs - HttpClient factory (pooling built-in)
builder.Services.AddHttpClient<IExternalService, ExternalService>();

// ❌ YANLIŞ: Her istekte yeni HttpClient
public class BadService
{
    public async Task<string> GetAsync(string url)
    {
        using var client = new HttpClient(); // Socket exhaustion riski!
        return await client.GetStringAsync(url);
    }
}
```

---

## 10. PROFILING & MONITORING

### 10.1 MiniProfiler

```csharp
// Program.cs
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
    options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
    options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
}).AddEntityFramework();

app.UseMiniProfiler();

// Controller'da custom timing
[HttpGet]
public async Task<IActionResult> GetProducts(CancellationToken ct)
{
    using (MiniProfiler.Current.Step("Get products from database"))
    {
        var products = await _repository.GetAllAsync(ct);
    }

    using (MiniProfiler.Current.Step("Map to DTOs"))
    {
        var dtos = _mapper.Map<List<ProductDto>>(products);
    }

    return Ok(dtos);
}
```

### 10.2 Application Insights

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Custom metrics
public class ProductService(TelemetryClient telemetry)
{
    public async Task<ProductDto> CreateAsync(CreateProductCommand cmd, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await CreateInternalAsync(cmd, ct);

            telemetry.TrackMetric("ProductCreation.Duration", stopwatch.ElapsedMilliseconds);
            telemetry.TrackEvent("ProductCreated", new Dictionary<string, string>
            {
                ["ProductId"] = result.Id.ToString(),
                ["Category"] = cmd.CategoryId.ToString()
            });

            return result;
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
```

### 10.3 Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database")
    .AddRedis(redisConnection, name: "redis")
    .AddCheck<CustomHealthCheck>("custom");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});
```

---

## PERFORMANS CHECKLIST

### Query Performance
- [ ] AsNoTracking() kullanılıyor
- [ ] AsSplitQuery() multiple include'da var
- [ ] Projection ile sadece gerekli kolonlar çekiliyor
- [ ] Compiled queries kullanılıyor
- [ ] Index'ler tanımlı

### Caching
- [ ] Multi-level cache implementasyonu var
- [ ] Cache stampede koruması var
- [ ] Cache invalidation stratejisi belirli
- [ ] TTL değerleri optimize edilmiş

### Memory
- [ ] Large object'ler için pooling kullanılıyor
- [ ] Dispose pattern doğru implement edilmiş
- [ ] String concatenation optimize edilmiş

### Async
- [ ] Async all the way
- [ ] CancellationToken kullanılıyor
- [ ] ValueTask uygun yerlerde kullanılıyor
- [ ] Paralel işlemler optimize edilmiş

---

*Bu kural dosyası, Merge E-Commerce Backend projesi için performans optimizasyon standartlarını belirler.*
