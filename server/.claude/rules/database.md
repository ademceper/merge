---
paths:
  - "Merge.Infrastructure/**/*.cs"
  - "**/Data/**/*.cs"
  - "**/Configurations/**/*.cs"
  - "**/Repositories/**/*.cs"
  - "**/DbContexts/**/*.cs"
  - "**/Migrations/**/*.cs"
  - "**/*DbContext.cs"
  - "**/*Repository.cs"
  - "**/*Configuration.cs"
---

# DATABASE & ENTITY FRAMEWORK CORE KURALLAR (ULTRA KAPSAMLI)

> Bu dosya, Merge E-Commerce Backend projesinde veritabanı ve EF Core kullanımı için
> kapsamlı kurallar ve en iyi uygulamaları içerir.

---

## İÇİNDEKİLER

1. [DbContext Organizasyonu](#1-dbcontext-organizasyonu)
2. [Entity Configuration](#2-entity-configuration)
3. [Query Optimizasyonu](#3-query-optimizasyonu)
4. [Repository Pattern](#4-repository-pattern)
5. [Unit of Work Pattern](#5-unit-of-work-pattern)
6. [Transaction Yönetimi](#6-transaction-yönetimi)
7. [Concurrency Handling](#7-concurrency-handling)
8. [Soft Delete Implementasyonu](#8-soft-delete-implementasyonu)
9. [Indexing Stratejileri](#9-indexing-stratejileri)
10. [Migration Yönetimi](#10-migration-yönetimi)
11. [Connection Pooling](#11-connection-pooling)
12. [Query Interceptors](#12-query-interceptors)
13. [Temporal Tables](#13-temporal-tables)
14. [JSON Columns](#14-json-columns)
15. [PostgreSQL Specific Features](#15-postgresql-specific-features)
16. [Performans Monitoring](#16-performans-monitoring)
17. [Test Veritabanları](#17-test-veritabanları)
18. [Anti-Patterns](#18-anti-patterns)

---

## 1. DBCONTEXT ORGANİZASYONU

### 1.1 Mevcut DbContext'ler

Bu projede **bounded context'lere göre ayrılmış** birden fazla DbContext bulunur:

| DbContext | Schema | Entities | Dosya Yolu |
|-----------|--------|----------|------------|
| `ApplicationDbContext` | `identity` | User, Role, UserRole, AuditLog, OutboxMessage | Infrastructure/Data/ApplicationDbContext.cs |
| `CatalogDbContext` | `catalog` | Product, Category, Brand, Review, ProductVariant, ProductImage | Infrastructure/Data/CatalogDbContext.cs |
| `OrderingDbContext` | `ordering` | Order, OrderItem, Cart, CartItem, Address | Infrastructure/Data/OrderingDbContext.cs |
| `PaymentDbContext` | `payment` | Payment, Transaction, PaymentMethod, Refund | Infrastructure/Data/PaymentDbContext.cs |
| `InventoryDbContext` | `inventory` | Stock, StockMovement, Reservation, Warehouse | Infrastructure/Data/InventoryDbContext.cs |
| `MarketingDbContext` | `marketing` | Coupon, Campaign, FlashSale, Notification | Infrastructure/Data/MarketingDbContext.cs |
| `MarketplaceDbContext` | `marketplace` | SellerProfile, Store, Commission, SellerReview | Infrastructure/Data/MarketplaceDbContext.cs |
| `ShippingDbContext` | `shipping` | Shipment, ShipmentItem, Carrier, TrackingEvent | Infrastructure/Data/ShippingDbContext.cs |

### 1.2 Base DbContext

```csharp
/// <summary>
/// Tüm DbContext'ler için temel sınıf.
/// Ortak davranışları (audit, outbox, soft delete) merkezi olarak yönetir.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IMediator _mediator;

    protected BaseDbContext(
        DbContextOptions options,
        ICurrentUserService currentUser,
        IDateTime dateTime,
        IMediator mediator) : base(options)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
        _mediator = mediator;
    }

    /// <summary>
    /// SaveChanges override - Audit ve Domain Event dispatch
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Audit bilgilerini ayarla
        UpdateAuditableEntities();

        // 2. Soft delete işle
        ProcessSoftDeletes();

        // 3. Domain event'leri topla
        var domainEvents = CollectDomainEvents();

        // 4. SaveChanges
        var result = await base.SaveChangesAsync(ct);

        // 5. Domain event'leri publish et (Outbox pattern ile)
        await DispatchDomainEventsAsync(domainEvents, ct);

        return result;
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var now = _dateTime.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedBy = userId;
                    break;
            }
        }
    }

    private void ProcessSoftDeletes()
    {
        var entries = ChangeTracker.Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = _dateTime.UtcNow;
            entry.Entity.DeletedBy = _currentUser.UserId;
        }
    }

    private IReadOnlyList<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = ChangeTracker.Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        aggregates.ForEach(a => a.ClearDomainEvents());

        return events;
    }

    private async Task DispatchDomainEventsAsync(
        IReadOnlyList<IDomainEvent> events,
        CancellationToken ct)
    {
        foreach (var @event in events)
        {
            // Outbox'a kaydet (transaction içinde)
            var outboxMessage = OutboxMessage.Create(@event);
            Set<OutboxMessage>().Add(outboxMessage);
        }

        // Outbox mesajlarını kaydet
        await base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter for soft delete
        ApplySoftDeleteFilter(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseDbContext).Assembly);
    }

    private void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var filter = Expression.Lambda(
                    Expression.Equal(property, Expression.Constant(false)),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
```

### 1.3 DbContext Registration

```csharp
// Program.cs veya DI Extension
public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Her DbContext için ayrı registration
        services.AddDbContext<ApplicationDbContext>(options =>
            ConfigureDbContext(options, connectionString));

        services.AddDbContext<CatalogDbContext>(options =>
            ConfigureDbContext(options, connectionString));

        services.AddDbContext<OrderingDbContext>(options =>
            ConfigureDbContext(options, connectionString));

        // ... diğer DbContext'ler

        return services;
    }

    private static void ConfigureDbContext(
        DbContextOptionsBuilder options,
        string connectionString)
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Retry on transient failures
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);

            // Migration assembly
            npgsqlOptions.MigrationsAssembly("Merge.Infrastructure");

            // Command timeout
            npgsqlOptions.CommandTimeout(30);

            // Performance optimizations
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // Development: Enable sensitive data logging
        #if DEBUG
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        #endif

        // Add interceptors
        options.AddInterceptors(
            new AuditingInterceptor(),
            new PerformanceInterceptor(),
            new SoftDeleteInterceptor()
        );
    }
}
```

---

## 2. ENTITY CONFIGURATION

### 2.1 Configuration Dosyası Yapısı

Her entity için ayrı configuration dosyası oluşturulmalı:

```
Merge.Infrastructure/
└── Data/
    └── Configurations/
        ├── Catalog/
        │   ├── ProductConfiguration.cs
        │   ├── CategoryConfiguration.cs
        │   ├── BrandConfiguration.cs
        │   └── ReviewConfiguration.cs
        ├── Ordering/
        │   ├── OrderConfiguration.cs
        │   └── OrderItemConfiguration.cs
        └── Identity/
            ├── UserConfiguration.cs
            └── RoleConfiguration.cs
```

### 2.2 Tam Teşekküllü Entity Configuration

```csharp
/// <summary>
/// Product entity'si için EF Core configuration.
/// Tüm property'ler, ilişkiler ve index'ler burada tanımlanır.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // ============================================================
        // TABLE CONFIGURATION
        // ============================================================

        builder.ToTable("products", "catalog");

        builder.HasComment("Ürün ana tablosu - tüm ürün bilgilerini içerir");

        // ============================================================
        // PRIMARY KEY
        // ============================================================

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        // ============================================================
        // REQUIRED PROPERTIES
        // ============================================================

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(200)")
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Ürün adı");

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .IsRequired(false)
            .HasComment("Ürün açıklaması (HTML destekli)");

        builder.Property(p => p.ShortDescription)
            .HasColumnName("short_description")
            .HasColumnType("varchar(500)")
            .IsRequired(false)
            .HasMaxLength(500)
            .HasComment("Kısa açıklama (listeleme için)");

        // ============================================================
        // VALUE OBJECTS
        // ============================================================

        // SKU Value Object
        builder.OwnsOne(p => p.SKU, sku =>
        {
            sku.Property(s => s.Value)
                .HasColumnName("sku")
                .HasColumnType("varchar(50)")
                .IsRequired()
                .HasMaxLength(50);
        });

        // Price Value Object (Money)
        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("price_amount")
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property(m => m.Currency)
                .HasColumnName("price_currency")
                .HasColumnType("varchar(3)")
                .HasMaxLength(3)
                .HasDefaultValue("TRY")
                .IsRequired();
        });

        // Compare At Price (indirim öncesi fiyat)
        builder.OwnsOne(p => p.CompareAtPrice, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("compare_at_price_amount")
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            price.Property(m => m.Currency)
                .HasColumnName("compare_at_price_currency")
                .HasColumnType("varchar(3)")
                .HasMaxLength(3);
        });

        // Cost Price (maliyet)
        builder.OwnsOne(p => p.CostPrice, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("cost_price_amount")
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            price.Property(m => m.Currency)
                .HasColumnName("cost_price_currency")
                .HasColumnType("varchar(3)")
                .HasMaxLength(3);
        });

        // Weight Value Object
        builder.OwnsOne(p => p.Weight, weight =>
        {
            weight.Property(w => w.Value)
                .HasColumnName("weight_value")
                .HasColumnType("decimal(10,3)")
                .HasPrecision(10, 3);

            weight.Property(w => w.Unit)
                .HasColumnName("weight_unit")
                .HasColumnType("varchar(10)")
                .HasMaxLength(10)
                .HasDefaultValue("kg");
        });

        // Dimensions Value Object
        builder.OwnsOne(p => p.Dimensions, dim =>
        {
            dim.Property(d => d.Length)
                .HasColumnName("dimension_length")
                .HasColumnType("decimal(10,2)")
                .HasPrecision(10, 2);

            dim.Property(d => d.Width)
                .HasColumnName("dimension_width")
                .HasColumnType("decimal(10,2)")
                .HasPrecision(10, 2);

            dim.Property(d => d.Height)
                .HasColumnName("dimension_height")
                .HasColumnType("decimal(10,2)")
                .HasPrecision(10, 2);

            dim.Property(d => d.Unit)
                .HasColumnName("dimension_unit")
                .HasColumnType("varchar(10)")
                .HasMaxLength(10)
                .HasDefaultValue("cm");
        });

        // SEO Value Object
        builder.OwnsOne(p => p.SEO, seo =>
        {
            seo.Property(s => s.MetaTitle)
                .HasColumnName("seo_meta_title")
                .HasColumnType("varchar(70)")
                .HasMaxLength(70);

            seo.Property(s => s.MetaDescription)
                .HasColumnName("seo_meta_description")
                .HasColumnType("varchar(160)")
                .HasMaxLength(160);

            seo.Property(s => s.Slug)
                .HasColumnName("seo_slug")
                .HasColumnType("varchar(200)")
                .HasMaxLength(200);

            seo.Property(s => s.CanonicalUrl)
                .HasColumnName("seo_canonical_url")
                .HasColumnType("varchar(500)")
                .HasMaxLength(500);
        });

        // ============================================================
        // SCALAR PROPERTIES
        // ============================================================

        builder.Property(p => p.StockQuantity)
            .HasColumnName("stock_quantity")
            .HasColumnType("integer")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.LowStockThreshold)
            .HasColumnName("low_stock_threshold")
            .HasColumnType("integer")
            .HasDefaultValue(10);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(p => p.IsFeatured)
            .HasColumnName("is_featured")
            .HasColumnType("boolean")
            .HasDefaultValue(false);

        builder.Property(p => p.IsDigital)
            .HasColumnName("is_digital")
            .HasColumnType("boolean")
            .HasDefaultValue(false);

        builder.Property(p => p.RequiresShipping)
            .HasColumnName("requires_shipping")
            .HasColumnType("boolean")
            .HasDefaultValue(true);

        builder.Property(p => p.TaxClassId)
            .HasColumnName("tax_class_id")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(p => p.Barcode)
            .HasColumnName("barcode")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(p => p.ViewCount)
            .HasColumnName("view_count")
            .HasColumnType("integer")
            .HasDefaultValue(0);

        builder.Property(p => p.SoldCount)
            .HasColumnName("sold_count")
            .HasColumnType("integer")
            .HasDefaultValue(0);

        builder.Property(p => p.AverageRating)
            .HasColumnName("average_rating")
            .HasColumnType("decimal(3,2)")
            .HasPrecision(3, 2)
            .HasDefaultValue(0m);

        builder.Property(p => p.ReviewCount)
            .HasColumnName("review_count")
            .HasColumnType("integer")
            .HasDefaultValue(0);

        builder.Property(p => p.SortOrder)
            .HasColumnName("sort_order")
            .HasColumnType("integer")
            .HasDefaultValue(0);

        // ============================================================
        // ENUM PROPERTIES
        // ============================================================

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasConversion<string>()
            .HasDefaultValue(ProductStatus.Draft);

        builder.Property(p => p.Visibility)
            .HasColumnName("visibility")
            .HasColumnType("varchar(20)")
            .HasConversion<string>()
            .HasDefaultValue(ProductVisibility.Visible);

        // ============================================================
        // AUDIT PROPERTIES
        // ============================================================

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid");

        builder.Property(p => p.LastModifiedAt)
            .HasColumnName("last_modified_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(p => p.LastModifiedBy)
            .HasColumnName("last_modified_by")
            .HasColumnType("uuid");

        // ============================================================
        // SOFT DELETE PROPERTIES
        // ============================================================

        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .HasColumnType("boolean")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(p => p.DeletedBy)
            .HasColumnName("deleted_by")
            .HasColumnType("uuid");

        // ============================================================
        // CONCURRENCY TOKEN
        // ============================================================

        builder.Property(p => p.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion()
            .IsConcurrencyToken();

        // ============================================================
        // RELATIONSHIPS
        // ============================================================

        // Category (Many-to-One)
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_products_category");

        // Brand (Many-to-One)
        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_products_brand");

        // Seller (Many-to-One) - Marketplace
        builder.HasOne(p => p.Seller)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_products_seller");

        // Variants (One-to-Many)
        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_product_variants_product");

        // Images (One-to-Many)
        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_product_images_product");

        // Reviews (One-to-Many)
        builder.HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_product_reviews_product");

        // Tags (Many-to-Many)
        builder.HasMany(p => p.Tags)
            .WithMany(t => t.Products)
            .UsingEntity<ProductTag>(
                "product_tags",
                l => l.HasOne<Tag>().WithMany().HasForeignKey(pt => pt.TagId),
                r => r.HasOne<Product>().WithMany().HasForeignKey(pt => pt.ProductId)
            );

        // Related Products (Self-referencing Many-to-Many)
        builder.HasMany(p => p.RelatedProducts)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "related_products",
                j => j.HasOne<Product>().WithMany().HasForeignKey("related_product_id"),
                j => j.HasOne<Product>().WithMany().HasForeignKey("product_id")
            );

        // ============================================================
        // INDEXES
        // ============================================================

        // Unique indexes
        builder.HasIndex(p => p.SKU)
            .IsUnique()
            .HasDatabaseName("ix_products_sku")
            .HasFilter("is_deleted = false");

        builder.HasIndex(p => p.Barcode)
            .IsUnique()
            .HasDatabaseName("ix_products_barcode")
            .HasFilter("barcode IS NOT NULL AND is_deleted = false");

        builder.HasIndex(p => new { p.SellerId, p.SKU })
            .IsUnique()
            .HasDatabaseName("ix_products_seller_sku")
            .HasFilter("is_deleted = false");

        // Foreign key indexes
        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("ix_products_category_id");

        builder.HasIndex(p => p.BrandId)
            .HasDatabaseName("ix_products_brand_id");

        builder.HasIndex(p => p.SellerId)
            .HasDatabaseName("ix_products_seller_id");

        // Composite indexes for common queries
        builder.HasIndex(p => new { p.IsActive, p.Status, p.CreatedAt })
            .HasDatabaseName("ix_products_active_status_created")
            .HasFilter("is_deleted = false");

        builder.HasIndex(p => new { p.CategoryId, p.IsActive, p.SortOrder })
            .HasDatabaseName("ix_products_category_active_sort")
            .HasFilter("is_deleted = false");

        builder.HasIndex(p => new { p.IsFeatured, p.IsActive })
            .HasDatabaseName("ix_products_featured_active")
            .HasFilter("is_deleted = false");

        builder.HasIndex(p => new { p.IsActive, p.AverageRating })
            .HasDatabaseName("ix_products_active_rating")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");

        builder.HasIndex(p => new { p.IsActive, p.SoldCount })
            .HasDatabaseName("ix_products_active_sold")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");

        // Full-text search index (PostgreSQL)
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("ix_products_name_fulltext")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        // Price range index
        builder.HasIndex("price_amount")
            .HasDatabaseName("ix_products_price")
            .HasFilter("is_deleted = false");
    }
}
```

### 2.3 Value Object Configuration

```csharp
/// <summary>
/// Money Value Object configuration.
/// Birden fazla entity'de kullanılır.
/// </summary>
public static class MoneyConfiguration
{
    public static void ConfigureMoney<TEntity>(
        this OwnedNavigationBuilder<TEntity, Money> builder,
        string columnPrefix) where TEntity : class
    {
        builder.Property(m => m.Amount)
            .HasColumnName($"{columnPrefix}_amount")
            .HasColumnType("decimal(18,2)")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(m => m.Currency)
            .HasColumnName($"{columnPrefix}_currency")
            .HasColumnType("varchar(3)")
            .HasMaxLength(3)
            .HasDefaultValue("TRY")
            .IsRequired();
    }
}

// Kullanımı
builder.OwnsOne(p => p.Price, price => price.ConfigureMoney("price"));
builder.OwnsOne(p => p.CostPrice, price => price.ConfigureMoney("cost_price"));
```

---

## 3. QUERY OPTİMİZASYONU

### 3.1 AsNoTracking() Kuralları

```csharp
// ✅ DOĞRU: Read-only sorgu
public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct)
{
    return await _context.Products
        .AsNoTracking()  // Tracking kapalı
        .Where(p => p.Id == id)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price.Amount,
            CategoryName = p.Category.Name
        })
        .FirstOrDefaultAsync(ct);
}

// ✅ DOĞRU: Update işlemi - tracking gerekli
public async Task<Product> GetForUpdateAsync(Guid id, CancellationToken ct)
{
    return await _context.Products
        // AsNoTracking YOK - entity'yi güncelleyeceğiz
        .FirstOrDefaultAsync(p => p.Id == id, ct)
        ?? throw new NotFoundException($"Product {id} not found");
}

// ❌ YANLIŞ: Read-only sorguda tracking açık
public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct)
{
    var products = await _context.Products
        .ToListAsync(ct);  // Tracking açık - gereksiz memory kullanımı

    return _mapper.Map<List<ProductDto>>(products);
}
```

### 3.2 AsSplitQuery() Kuralları

```csharp
// ✅ DOĞRU: Çoklu Include ile SplitQuery
public async Task<Order?> GetOrderWithDetailsAsync(Guid id, CancellationToken ct)
{
    return await _context.Orders
        .AsNoTracking()
        .AsSplitQuery()  // Cartesian explosion'ı önle
        .Include(o => o.Items)
            .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images.Take(1))  // İlk resmi al
        .Include(o => o.User)
        .Include(o => o.ShippingAddress)
        .Include(o => o.BillingAddress)
        .Include(o => o.Payments)
        .FirstOrDefaultAsync(o => o.Id == id, ct);
}

// ❌ YANLIŞ: SplitQuery olmadan çoklu Include
public async Task<Order?> GetOrderBadAsync(Guid id, CancellationToken ct)
{
    // Bu sorgu cartesian explosion'a neden olabilir
    // Örnek: 10 item * 5 payment * 3 address = 150 satır
    return await _context.Orders
        .Include(o => o.Items)
        .Include(o => o.Payments)
        .Include(o => o.Addresses)
        .FirstOrDefaultAsync(o => o.Id == id, ct);
}
```

### 3.3 Projection (Select) Best Practices

```csharp
// ✅ DOĞRU: Sadece gerekli kolonları çek
public async Task<PagedResult<ProductListDto>> GetProductListAsync(
    int page, int pageSize, CancellationToken ct)
{
    var query = _context.Products
        .AsNoTracking()
        .Where(p => p.IsActive);

    var totalCount = await query.CountAsync(ct);

    var items = await query
        .OrderByDescending(p => p.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.SEO.Slug,
            Price = p.Price.Amount,
            Currency = p.Price.Currency,
            ImageUrl = p.Images
                .Where(i => i.IsPrimary)
                .Select(i => i.Url)
                .FirstOrDefault(),
            CategoryName = p.Category.Name,
            AverageRating = p.AverageRating,
            ReviewCount = p.ReviewCount,
            IsInStock = p.StockQuantity > 0
        })
        .ToListAsync(ct);

    return new PagedResult<ProductListDto>(items, totalCount, page, pageSize);
}

// ❌ YANLIŞ: Tüm entity'yi çekip sonra map
public async Task<List<ProductListDto>> GetProductListBadAsync(CancellationToken ct)
{
    // Tüm kolonlar çekiliyor, gereksiz memory kullanımı
    var products = await _context.Products
        .Include(p => p.Category)
        .Include(p => p.Images)
        .ToListAsync(ct);

    return products.Select(p => new ProductListDto
    {
        Id = p.Id,
        Name = p.Name,
        // ...
    }).ToList();
}
```

### 3.4 Compiled Queries

```csharp
/// <summary>
/// Sık kullanılan sorgular için compiled query kullanımı.
/// İlk çalıştırmadan sonra query plan cache'lenir.
/// </summary>
public static class ProductQueries
{
    // Compiled query - 15-20% daha hızlı
    public static readonly Func<CatalogDbContext, Guid, CancellationToken, Task<Product?>>
        GetByIdAsync = EF.CompileAsyncQuery(
            (CatalogDbContext context, Guid id, CancellationToken ct) =>
                context.Products
                    .AsNoTracking()
                    .FirstOrDefault(p => p.Id == id));

    public static readonly Func<CatalogDbContext, string, CancellationToken, Task<Product?>>
        GetBySkuAsync = EF.CompileAsyncQuery(
            (CatalogDbContext context, string sku, CancellationToken ct) =>
                context.Products
                    .AsNoTracking()
                    .FirstOrDefault(p => p.SKU.Value == sku));

    public static readonly Func<CatalogDbContext, Guid, CancellationToken, Task<bool>>
        ExistsAsync = EF.CompileAsyncQuery(
            (CatalogDbContext context, Guid id, CancellationToken ct) =>
                context.Products.Any(p => p.Id == id));
}

// Kullanımı
var product = await ProductQueries.GetByIdAsync(_context, productId, ct);
```

### 3.5 Raw SQL Queries

```csharp
/// <summary>
/// Kompleks sorgular için raw SQL kullanımı.
/// Performans kritik durumlarda tercih edilir.
/// </summary>
public async Task<List<ProductSearchResult>> SearchProductsAsync(
    string searchTerm,
    CancellationToken ct)
{
    // PostgreSQL full-text search
    var sql = """
        SELECT
            p.id,
            p.name,
            p.price_amount as "PriceAmount",
            p.seo_slug as "Slug",
            ts_rank(
                to_tsvector('turkish', p.name || ' ' || COALESCE(p.description, '')),
                plainto_tsquery('turkish', @searchTerm)
            ) as "Rank"
        FROM catalog.products p
        WHERE
            p.is_deleted = false
            AND p.is_active = true
            AND (
                p.name ILIKE @pattern
                OR to_tsvector('turkish', p.name || ' ' || COALESCE(p.description, ''))
                   @@ plainto_tsquery('turkish', @searchTerm)
            )
        ORDER BY "Rank" DESC, p.sold_count DESC
        LIMIT 50
        """;

    return await _context.Database
        .SqlQueryRaw<ProductSearchResult>(
            sql,
            new NpgsqlParameter("@searchTerm", searchTerm),
            new NpgsqlParameter("@pattern", $"%{searchTerm}%"))
        .ToListAsync(ct);
}
```

### 3.6 Batch Operations

```csharp
/// <summary>
/// Toplu güncelleme işlemleri için ExecuteUpdate/ExecuteDelete.
/// EF Core 7.0+ feature.
/// </summary>
public async Task<int> DeactivateExpiredProductsAsync(CancellationToken ct)
{
    // ExecuteUpdate - tek SQL sorgusu ile toplu güncelleme
    return await _context.Products
        .Where(p => p.ExpirationDate < DateTime.UtcNow && p.IsActive)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.IsActive, false)
            .SetProperty(p => p.Status, ProductStatus.Expired)
            .SetProperty(p => p.LastModifiedAt, DateTime.UtcNow),
            ct);
}

public async Task<int> DeleteOldAuditLogsAsync(CancellationToken ct)
{
    var cutoffDate = DateTime.UtcNow.AddMonths(-6);

    // ExecuteDelete - tek SQL sorgusu ile toplu silme
    return await _context.AuditLogs
        .Where(a => a.CreatedAt < cutoffDate)
        .ExecuteDeleteAsync(ct);
}

// Dikkat: Bu işlemler SaveChangesAsync çağırmaz,
// direkt veritabanında çalışır ve change tracker'ı bypass eder
```

---

## 4. REPOSITORY PATTERN

### 4.1 Generic Repository Interface

```csharp
/// <summary>
/// Generic repository interface.
/// Tüm entity'ler için temel CRUD operasyonları.
/// </summary>
public interface IRepository<T> where T : BaseEntity, IAggregateRoot
{
    // Query
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> GetBySpecAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<List<T>> ListAsync(CancellationToken ct = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);

    // Command
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
}

/// <summary>
/// Read-only repository interface (CQRS Query side).
/// Sadece okuma operasyonları.
/// </summary>
public interface IReadRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> GetBySpecAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<List<T>> ListAsync(CancellationToken ct = default);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);

    // Projection support
    Task<TResult?> GetBySpecAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken ct = default);
    Task<List<TResult>> ListAsync<TResult>(
        ISpecification<T, TResult> spec,
        CancellationToken ct = default);
}
```

### 4.2 Generic Repository Implementation

```csharp
/// <summary>
/// Generic repository implementation.
/// Specification pattern destekli.
/// </summary>
public class Repository<T>(DbContext context) : IRepository<T>
    where T : BaseEntity, IAggregateRoot
{
    protected readonly DbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync([id], ct);
    }

    public virtual async Task<T?> GetBySpecAsync(
        ISpecification<T> spec,
        CancellationToken ct = default)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync(ct);
    }

    public virtual async Task<List<T>> ListAsync(CancellationToken ct = default)
    {
        return await _dbSet.ToListAsync(ct);
    }

    public virtual async Task<List<T>> ListAsync(
        ISpecification<T> spec,
        CancellationToken ct = default)
    {
        return await ApplySpecification(spec).ToListAsync(ct);
    }

    public virtual async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _dbSet.CountAsync(ct);
    }

    public virtual async Task<int> CountAsync(
        ISpecification<T> spec,
        CancellationToken ct = default)
    {
        return await ApplySpecification(spec, true).CountAsync(ct);
    }

    public virtual async Task<bool> AnyAsync(
        ISpecification<T> spec,
        CancellationToken ct = default)
    {
        return await ApplySpecification(spec, true).AnyAsync(ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Specification'ı IQueryable'a uygular.
    /// </summary>
    protected IQueryable<T> ApplySpecification(
        ISpecification<T> spec,
        bool countOnly = false)
    {
        return SpecificationEvaluator<T>.GetQuery(
            _dbSet.AsQueryable(),
            spec,
            countOnly);
    }
}
```

### 4.3 Specialized Repository

```csharp
/// <summary>
/// Product repository - özel sorgular içerir.
/// Generic repository'yi extend eder.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<Product?> GetWithVariantsAsync(Guid id, CancellationToken ct = default);
    Task<List<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<List<Product>> GetFeaturedAsync(int count, CancellationToken ct = default);
    Task<List<Product>> SearchAsync(string term, int maxResults, CancellationToken ct = default);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeId, CancellationToken ct = default);
}

public class ProductRepository(CatalogDbContext context)
    : Repository<Product>(context), IProductRepository
{
    private readonly CatalogDbContext _catalogContext = context;

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        return await _catalogContext.Products
            .FirstOrDefaultAsync(p => p.SKU.Value == sku, ct);
    }

    public async Task<Product?> GetWithVariantsAsync(Guid id, CancellationToken ct = default)
    {
        return await _catalogContext.Products
            .AsSplitQuery()
            .Include(p => p.Variants)
                .ThenInclude(v => v.OptionValues)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken ct = default)
    {
        return await _catalogContext.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Product>> GetFeaturedAsync(int count, CancellationToken ct = default)
    {
        return await _catalogContext.Products
            .AsNoTracking()
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderByDescending(p => p.SoldCount)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<Product>> SearchAsync(
        string term,
        int maxResults,
        CancellationToken ct = default)
    {
        var normalizedTerm = term.ToLowerInvariant();

        return await _catalogContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive && (
                EF.Functions.ILike(p.Name, $"%{normalizedTerm}%") ||
                EF.Functions.ILike(p.SKU.Value, $"%{normalizedTerm}%") ||
                EF.Functions.ILike(p.Description, $"%{normalizedTerm}%")))
            .OrderByDescending(p => p.SoldCount)
            .Take(maxResults)
            .ToListAsync(ct);
    }

    public async Task<bool> IsSkuUniqueAsync(
        string sku,
        Guid? excludeId,
        CancellationToken ct = default)
    {
        var query = _catalogContext.Products.Where(p => p.SKU.Value == sku);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return !await query.AnyAsync(ct);
    }
}
```

---

## 5. UNIT OF WORK PATTERN

### 5.1 Unit of Work Interface

```csharp
/// <summary>
/// Unit of Work interface.
/// Transaction yönetimi ve repository erişimi sağlar.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    IUserRepository Users { get; }
    // ... diğer repository'ler

    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);

    // Execution strategy (retry logic)
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken ct = default);
}
```

### 5.2 Unit of Work Implementation

```csharp
/// <summary>
/// Unit of Work implementation.
/// Lazy loading ile repository initialization.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private IDbContextTransaction? _currentTransaction;

    // Lazy repository instances
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private IOrderRepository? _orders;
    private IUserRepository? _users;

    public UnitOfWork(
        ApplicationDbContext context,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    // Repository accessors (lazy initialization)
    public IProductRepository Products =>
        _products ??= _serviceProvider.GetRequiredService<IProductRepository>();

    public ICategoryRepository Categories =>
        _categories ??= _serviceProvider.GetRequiredService<ICategoryRepository>();

    public IOrderRepository Orders =>
        _orders ??= _serviceProvider.GetRequiredService<IOrderRepository>();

    public IUserRepository Users =>
        _users ??= _serviceProvider.GetRequiredService<IUserRepository>();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No transaction in progress.");

        try
        {
            await _currentTransaction.CommitAsync(ct);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction == null)
            throw new InvalidOperationException("No transaction in progress.");

        try
        {
            await _currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken ct = default)
    {
        // Execution strategy ile retry logic
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var result = await operation();
                await transaction.CommitAsync(ct);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

---

## 6. TRANSACTION YÖNETİMİ

### 6.1 Transaction Best Practices

```csharp
/// <summary>
/// Transaction kullanım örnekleri.
/// </summary>
public class OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
{
    // ✅ DOĞRU: Explicit transaction management
    public async Task<OrderDto> CreateOrderAsync(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 1. Stok kontrolü ve rezervasyonu
            foreach (var item in command.Items)
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId, ct)
                    ?? throw new NotFoundException($"Product {item.ProductId} not found");

                product.ReserveStock(item.Quantity);
                unitOfWork.Products.Update(product);
            }

            // 2. Sipariş oluştur
            var order = Order.Create(
                command.UserId,
                command.ShippingAddress,
                command.BillingAddress);

            foreach (var item in command.Items)
            {
                order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
            }

            await unitOfWork.Orders.AddAsync(order, ct);

            // 3. Kupon uygula (varsa)
            if (command.CouponCode != null)
            {
                var coupon = await ApplyCouponAsync(order, command.CouponCode, ct);
                order.ApplyDiscount(coupon.DiscountAmount);
            }

            // 4. Kaydet (Outbox'a domain event'ler de yazılır)
            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation(
                "Order {OrderId} created for user {UserId} with {ItemCount} items",
                order.Id, command.UserId, command.Items.Count);

            return _mapper.Map<OrderDto>(order);
        }, ct);
    }

    // ✅ DOĞRU: Distributed transaction için Saga pattern
    public async Task<PaymentResult> ProcessPaymentAsync(
        ProcessPaymentCommand command,
        CancellationToken ct)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(command.OrderId, ct)
            ?? throw new NotFoundException($"Order {command.OrderId} not found");

        try
        {
            // 1. Ödeme işlemi (external service)
            var paymentResult = await _paymentGateway.ProcessAsync(
                command.PaymentMethod,
                order.TotalAmount,
                ct);

            if (!paymentResult.IsSuccess)
            {
                order.MarkPaymentFailed(paymentResult.ErrorMessage);
                await unitOfWork.SaveChangesAsync(ct);
                return paymentResult;
            }

            // 2. Sipariş durumunu güncelle
            order.MarkPaymentCompleted(paymentResult.TransactionId);

            // 3. Stok düş
            await DeductStockAsync(order, ct);

            await unitOfWork.SaveChangesAsync(ct);

            return paymentResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Payment processing failed for order {OrderId}",
                command.OrderId);

            // Compensating transaction
            order.MarkPaymentFailed(ex.Message);
            await unitOfWork.SaveChangesAsync(ct);

            throw;
        }
    }
}
```

### 6.2 Distributed Transactions (Outbox Pattern)

```csharp
/// <summary>
/// Outbox pattern ile reliable event publishing.
/// Domain event'ler transactional olarak kaydedilir.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? Error { get; private set; }
    public OutboxMessageStatus Status { get; private set; }

    public static OutboxMessage Create(IDomainEvent @event)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = @event.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(@event, @event.GetType()),
            CreatedAt = DateTime.UtcNow,
            Status = OutboxMessageStatus.Pending
        };
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Status = OutboxMessageStatus.Processed;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
        RetryCount++;
        Status = RetryCount >= 5
            ? OutboxMessageStatus.Failed
            : OutboxMessageStatus.Pending;
    }
}

/// <summary>
/// Background service ile outbox processing.
/// </summary>
public class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var messages = await context.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(100)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType)!;
                var @event = (IDomainEvent)JsonSerializer.Deserialize(
                    message.Payload,
                    eventType)!;

                await mediator.Publish(@event, ct);

                message.MarkAsProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to process outbox message {MessageId}",
                    message.Id);
                message.MarkAsFailed(ex.Message);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
```

---

## 7. CONCURRENCY HANDLING

### 7.1 Optimistic Concurrency

```csharp
/// <summary>
/// Optimistic concurrency ile eş zamanlı erişim kontrolü.
/// RowVersion property kullanılır.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    /// <summary>
    /// Concurrency token - her update'te otomatik artar.
    /// </summary>
    public byte[] RowVersion { get; protected set; } = null!;
}

// Entity Configuration
builder.Property(e => e.RowVersion)
    .IsRowVersion()
    .IsConcurrencyToken();

// Handler'da kullanımı
public async Task<ProductDto> Handle(
    UpdateProductCommand request,
    CancellationToken ct)
{
    var product = await _repository.GetByIdAsync(request.Id, ct)
        ?? throw new NotFoundException($"Product {request.Id} not found");

    // Client'tan gelen RowVersion ile karşılaştır
    if (!product.RowVersion.SequenceEqual(request.RowVersion))
    {
        throw new ConcurrencyException(
            "The product was modified by another user. Please refresh and try again.");
    }

    product.Update(request.Name, request.Description, request.Price);

    try
    {
        await _unitOfWork.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        _logger.LogWarning(ex,
            "Concurrency conflict updating product {ProductId}",
            request.Id);

        throw new ConcurrencyException(
            "The product was modified by another user. Please refresh and try again.");
    }

    return _mapper.Map<ProductDto>(product);
}
```

### 7.2 Pessimistic Locking

```csharp
/// <summary>
/// Pessimistic locking - kritik operasyonlar için.
/// PostgreSQL'de SELECT FOR UPDATE kullanır.
/// </summary>
public async Task<int> ReserveStockWithLockAsync(
    Guid productId,
    int quantity,
    CancellationToken ct)
{
    // Row-level lock
    var sql = """
        SELECT stock_quantity
        FROM catalog.products
        WHERE id = @productId
        FOR UPDATE NOWAIT
        """;

    try
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var transaction = await connection.BeginTransactionAsync(ct);

        var currentStock = await connection.QuerySingleAsync<int>(
            sql,
            new { productId },
            transaction);

        if (currentStock < quantity)
        {
            throw new InsufficientStockException(productId, quantity, currentStock);
        }

        var updateSql = """
            UPDATE catalog.products
            SET stock_quantity = stock_quantity - @quantity,
                last_modified_at = @now
            WHERE id = @productId
            """;

        await connection.ExecuteAsync(
            updateSql,
            new { productId, quantity, now = DateTime.UtcNow },
            transaction);

        await transaction.CommitAsync(ct);

        return currentStock - quantity;
    }
    catch (PostgresException ex) when (ex.SqlState == "55P03") // lock_not_available
    {
        throw new ConcurrencyException(
            "Product is being modified by another process. Please try again.");
    }
}
```

---

## 8. SOFT DELETE IMPLEMENTASYONU

### 8.1 Soft Delete Interface ve Base Entity

```csharp
/// <summary>
/// Soft delete interface.
/// Silinen entity'ler fiziksel olarak silinmez, işaretlenir.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}

public abstract class SoftDeletableEntity : BaseEntity, ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public virtual void SoftDelete(Guid deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
```

### 8.2 Global Query Filter

```csharp
// DbContext.OnModelCreating içinde
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Tüm soft deletable entity'lere global filter uygula
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeletedProperty = Expression.Property(parameter, "IsDeleted");
            var condition = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
```

### 8.3 Soft Delete ile Çalışma

```csharp
// Normal sorgular - otomatik olarak IsDeleted = false filtresi uygulanır
var activeProducts = await _context.Products.ToListAsync(ct);

// Silinen entity'leri de dahil et
var allProducts = await _context.Products
    .IgnoreQueryFilters()
    .ToListAsync(ct);

// Sadece silinen entity'ler
var deletedProducts = await _context.Products
    .IgnoreQueryFilters()
    .Where(p => p.IsDeleted)
    .ToListAsync(ct);

// Soft delete işlemi
public async Task DeleteProductAsync(Guid id, CancellationToken ct)
{
    var product = await _repository.GetByIdAsync(id, ct)
        ?? throw new NotFoundException($"Product {id} not found");

    // Domain method ile soft delete
    product.SoftDelete(_currentUser.UserId);

    // Ya da direkt Remove (SaveChanges'da intercept edilir)
    // _context.Products.Remove(product);

    await _unitOfWork.SaveChangesAsync(ct);
}

// Restore işlemi
public async Task RestoreProductAsync(Guid id, CancellationToken ct)
{
    var product = await _context.Products
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted, ct)
        ?? throw new NotFoundException($"Deleted product {id} not found");

    product.Restore();

    await _unitOfWork.SaveChangesAsync(ct);
}
```

---

## 9. INDEXING STRATEJİLERİ

### 9.1 Index Türleri ve Kullanım Alanları

```csharp
public class IndexExamplesConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // ============================================================
        // UNIQUE INDEX - Benzersiz değerler için
        // ============================================================

        builder.HasIndex(p => p.SKU)
            .IsUnique()
            .HasDatabaseName("ix_products_sku_unique");

        // Partial unique index (soft delete ile)
        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email_unique")
            .HasFilter("is_deleted = false");  // PostgreSQL

        // ============================================================
        // COMPOSITE INDEX - Çoklu kolon sorguları için
        // ============================================================

        // Sıralama önemli! En seçici kolon önce
        builder.HasIndex(p => new { p.CategoryId, p.IsActive, p.CreatedAt })
            .HasDatabaseName("ix_products_category_active_created");

        // Covering index (include columns)
        builder.HasIndex(p => new { p.CategoryId, p.IsActive })
            .IncludeProperties(p => new { p.Name, p.Price, p.ImageUrl })
            .HasDatabaseName("ix_products_category_covering");

        // ============================================================
        // DESCENDING INDEX - ORDER BY DESC sorguları için
        // ============================================================

        builder.HasIndex(p => new { p.IsActive, p.CreatedAt })
            .IsDescending(false, true)  // IsActive ASC, CreatedAt DESC
            .HasDatabaseName("ix_products_active_created_desc");

        // ============================================================
        // FILTERED INDEX - Belirli veri alt kümeleri için
        // ============================================================

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("ix_products_category_active_only")
            .HasFilter("is_active = true AND is_deleted = false");

        // ============================================================
        // FULL-TEXT INDEX (PostgreSQL GIN)
        // ============================================================

        // Trigram extension gerekli: CREATE EXTENSION pg_trgm;
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("ix_products_name_gin")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        // Full-text search için
        // CREATE INDEX ix_products_fts ON catalog.products
        // USING gin(to_tsvector('turkish', name || ' ' || description));

        // ============================================================
        // BTREE INDEX (Default) - Range ve equality queries
        // ============================================================

        builder.HasIndex(p => p.Price)
            .HasDatabaseName("ix_products_price");

        // ============================================================
        // HASH INDEX - Sadece equality queries için (daha az yer kaplar)
        // ============================================================

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_orders_user_hash")
            .HasMethod("hash");
    }
}
```

### 9.2 Index Performans Kuralları

```csharp
/*
 * INDEX BEST PRACTICES
 * ====================
 *
 * 1. Foreign key'lere MUTLAKA index ekle
 *    - EF Core otomatik eklemez!
 *    - builder.HasIndex(p => p.CategoryId);
 *
 * 2. WHERE koşullarında sık kullanılan kolonlara index ekle
 *    - Seçicilik önemli: Benzersiz değerler daha iyi
 *    - Boolean kolonlar tek başına index'e değmez
 *
 * 3. ORDER BY kolonlarına index ekle
 *    - Sıralama yönüne dikkat (DESC için .IsDescending)
 *
 * 4. Composite index'te kolon sırası önemli
 *    - En seçici kolon önce
 *    - WHERE kolonu ORDER BY kolonundan önce
 *
 * 5. Covering index ile extra lookup'tan kaçın
 *    - .IncludeProperties ile SELECT kolonlarını ekle
 *
 * 6. Gereksiz index'lerden kaçın
 *    - Her index INSERT/UPDATE'i yavaşlatır
 *    - Çok fazla index = bakım maliyeti
 *
 * 7. Index kullanımını EXPLAIN ANALYZE ile kontrol et
 *    - Sequential scan görüyorsan index eksik
 *    - Index scan görüyorsan OK
 */

// Index kullanım analizi
public async Task AnalyzeQueryPlanAsync(CancellationToken ct)
{
    var sql = """
        EXPLAIN ANALYZE
        SELECT * FROM catalog.products
        WHERE category_id = 'xxx' AND is_active = true
        ORDER BY created_at DESC
        LIMIT 20
        """;

    var plan = await _context.Database
        .SqlQueryRaw<string>(sql)
        .ToListAsync(ct);

    // Plan'ı analiz et:
    // - "Seq Scan" = Index yok veya kullanılmıyor
    // - "Index Scan" = Index kullanılıyor
    // - "Index Only Scan" = Covering index (en iyi)
}
```

---

## 10. MIGRATION YÖNETİMİ

### 10.1 Migration Komutları

```bash
# Yeni migration oluştur
dotnet ef migrations add AddProductRating \
    --project Merge.Infrastructure \
    --startup-project Merge.API \
    --context CatalogDbContext

# Migration uygula (development)
dotnet ef database update \
    --project Merge.Infrastructure \
    --startup-project Merge.API \
    --context CatalogDbContext

# Belirli migration'a güncelle
dotnet ef database update AddProductRating \
    --project Merge.Infrastructure \
    --startup-project Merge.API

# Migration geri al
dotnet ef database update PreviousMigrationName \
    --project Merge.Infrastructure \
    --startup-project Merge.API

# Son migration'ı sil (uygulanmamışsa)
dotnet ef migrations remove \
    --project Merge.Infrastructure \
    --startup-project Merge.API

# SQL script oluştur (production için)
dotnet ef migrations script \
    --project Merge.Infrastructure \
    --startup-project Merge.API \
    --idempotent \
    --output migrations.sql

# İki migration arası script
dotnet ef migrations script FromMigration ToMigration \
    --project Merge.Infrastructure \
    --startup-project Merge.API \
    --output update.sql
```

### 10.2 Migration Best Practices

```csharp
/// <summary>
/// Migration örneği - kompleks değişiklik.
/// </summary>
public partial class AddProductRating : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Yeni kolon ekle (nullable olarak)
        migrationBuilder.AddColumn<decimal>(
            name: "average_rating",
            schema: "catalog",
            table: "products",
            type: "decimal(3,2)",
            nullable: true);

        // 2. Default değer ile güncelle
        migrationBuilder.Sql("""
            UPDATE catalog.products
            SET average_rating = 0
            WHERE average_rating IS NULL
            """);

        // 3. NOT NULL constraint ekle
        migrationBuilder.AlterColumn<decimal>(
            name: "average_rating",
            schema: "catalog",
            table: "products",
            type: "decimal(3,2)",
            nullable: false,
            defaultValue: 0m);

        // 4. Index ekle
        migrationBuilder.CreateIndex(
            name: "ix_products_rating",
            schema: "catalog",
            table: "products",
            columns: new[] { "is_active", "average_rating" },
            descending: new[] { false, true },
            filter: "is_deleted = false");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_products_rating",
            schema: "catalog",
            table: "products");

        migrationBuilder.DropColumn(
            name: "average_rating",
            schema: "catalog",
            table: "products");
    }
}

/*
 * MIGRATION RULES
 * ===============
 *
 * 1. Her migration TEK bir değişiklik yapmalı
 *    - AddProductRating ✓
 *    - UpdateProductsAndOrders ✗
 *
 * 2. Breaking change'leri adımlara böl
 *    - Kolon sil: Önce kod güncelle, sonra migration
 *    - Kolon ekle (NOT NULL): Önce nullable ekle, data migrate et, sonra NOT NULL
 *
 * 3. Data migration için SQL kullan
 *    - migrationBuilder.Sql("UPDATE ...")
 *    - Büyük tablolarda batch kullan
 *
 * 4. Index ekleme/silme dikkatli yap
 *    - CONCURRENTLY (PostgreSQL) - production için
 *
 * 5. Down metodunu MUTLAKA yaz
 *    - Geri alınamayan migration = tehlike
 *
 * 6. Production'da --idempotent kullan
 *    - Aynı script birden fazla çalışabilmeli
 */
```

### 10.3 Database Seeding

```csharp
/// <summary>
/// Seed data - ilk kurulum için.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Transaction içinde seed
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    Role.Create("Admin", "System administrator"),
                    Role.Create("Seller", "Marketplace seller"),
                    Role.Create("Customer", "Regular customer")
                };
                await context.Roles.AddRangeAsync(roles);
            }

            // Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = GetSeedCategories();
                await context.Categories.AddRangeAsync(categories);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static List<Category> GetSeedCategories()
    {
        return
        [
            Category.Create("Electronics", "electronic-devices", null),
            Category.Create("Clothing", "clothing", null),
            Category.Create("Home & Garden", "home-garden", null)
        ];
    }
}

// Program.cs'de kullanım
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DatabaseSeeder.SeedAsync(context);
}
```

---

## 11. CONNECTION POOLING

### 11.1 Connection Pool Configuration

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=merge_db;Username=postgres;Password=xxx;Pooling=true;MinPoolSize=5;MaxPoolSize=100;ConnectionIdleLifetime=300;ConnectionPruningInterval=10"
  }
}

// Programmatic configuration
services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Connection resiliency
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);

        // Connection timeout
        npgsqlOptions.CommandTimeout(30);
    });
});

/*
 * CONNECTION POOL SETTINGS
 * ========================
 *
 * MinPoolSize: Minimum açık bağlantı sayısı
 *   - Development: 1-5
 *   - Production: 10-20
 *
 * MaxPoolSize: Maksimum açık bağlantı sayısı
 *   - Default: 100
 *   - Production: CPU core sayısı * 4 (önerilen)
 *
 * ConnectionIdleLifetime: Boşta bağlantı ömrü (saniye)
 *   - Default: 300
 *
 * ConnectionPruningInterval: Pool temizleme aralığı (saniye)
 *   - Default: 10
 */
```

### 11.2 Connection Health Check

```csharp
/// <summary>
/// Database health check.
/// </summary>
public class DatabaseHealthCheck(ApplicationDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            // Basit bir sorgu ile bağlantı kontrolü
            await context.Database.ExecuteSqlRawAsync("SELECT 1", ct);

            // Pool istatistikleri
            var poolStats = await GetPoolStatisticsAsync(ct);

            return HealthCheckResult.Healthy("Database is healthy", poolStats);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }

    private async Task<Dictionary<string, object>> GetPoolStatisticsAsync(
        CancellationToken ct)
    {
        var sql = """
            SELECT
                count(*) as total_connections,
                count(*) FILTER (WHERE state = 'active') as active_connections,
                count(*) FILTER (WHERE state = 'idle') as idle_connections
            FROM pg_stat_activity
            WHERE datname = current_database()
            """;

        var stats = await context.Database
            .SqlQueryRaw<PoolStats>(sql)
            .FirstOrDefaultAsync(ct);

        return new Dictionary<string, object>
        {
            ["totalConnections"] = stats?.TotalConnections ?? 0,
            ["activeConnections"] = stats?.ActiveConnections ?? 0,
            ["idleConnections"] = stats?.IdleConnections ?? 0
        };
    }
}
```

---

## 12. QUERY INTERCEPTORS

### 12.1 Custom Interceptors

```csharp
/// <summary>
/// Audit logging interceptor.
/// Tüm sorguları loglar.
/// </summary>
public class AuditingInterceptor : DbCommandInterceptor
{
    private readonly ILogger<AuditingInterceptor> _logger;

    public AuditingInterceptor(ILogger<AuditingInterceptor> logger)
    {
        _logger = logger;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        LogCommand(command, eventData);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken ct = default)
    {
        LogCommand(command, eventData);
        return base.ReaderExecutingAsync(command, eventData, result, ct);
    }

    private void LogCommand(DbCommand command, CommandEventData eventData)
    {
        _logger.LogDebug(
            "Executing DbCommand [{CommandId}]: {CommandText}",
            eventData.CommandId,
            command.CommandText);
    }
}

/// <summary>
/// Performance interceptor - yavaş sorguları tespit eder.
/// </summary>
public class PerformanceInterceptor : DbCommandInterceptor
{
    private readonly ILogger<PerformanceInterceptor> _logger;
    private readonly TimeSpan _slowQueryThreshold = TimeSpan.FromMilliseconds(500);

    public PerformanceInterceptor(ILogger<PerformanceInterceptor> logger)
    {
        _logger = logger;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        CheckSlowQuery(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    private void CheckSlowQuery(DbCommand command, CommandExecutedEventData eventData)
    {
        if (eventData.Duration > _slowQueryThreshold)
        {
            _logger.LogWarning(
                "Slow query detected [{CommandId}]. Duration: {Duration}ms. SQL: {Sql}",
                eventData.CommandId,
                eventData.Duration.TotalMilliseconds,
                command.CommandText);
        }
    }
}

/// <summary>
/// Soft delete interceptor - DELETE'i UPDATE'e çevirir.
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDeleteToSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        ConvertDeleteToSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void ConvertDeleteToSoftDelete(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;
        }
    }
}
```

---

## 13. TEMPORAL TABLES

### 13.1 Temporal Table Configuration (EF Core 6.0+)

```csharp
/// <summary>
/// Temporal table - otomatik history tracking.
/// Audit log yerine kullanılabilir.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Temporal table configuration
        builder.ToTable("products", "catalog", tableBuilder =>
        {
            tableBuilder.IsTemporal(temporalBuilder =>
            {
                temporalBuilder.HasPeriodStart("valid_from");
                temporalBuilder.HasPeriodEnd("valid_to");
                temporalBuilder.UseHistoryTable("products_history", "catalog");
            });
        });
    }
}

// Temporal query örnekleri
public class ProductRepository
{
    // Belirli bir tarihteki durum
    public async Task<Product?> GetAsOfAsync(
        Guid id,
        DateTime asOf,
        CancellationToken ct)
    {
        return await _context.Products
            .TemporalAsOf(asOf)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    // Tarih aralığındaki tüm versiyonlar
    public async Task<List<Product>> GetHistoryAsync(
        Guid id,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        return await _context.Products
            .TemporalBetween(from, to)
            .Where(p => p.Id == id)
            .OrderBy(p => EF.Property<DateTime>(p, "valid_from"))
            .ToListAsync(ct);
    }

    // Tüm history
    public async Task<List<Product>> GetAllHistoryAsync(
        Guid id,
        CancellationToken ct)
    {
        return await _context.Products
            .TemporalAll()
            .Where(p => p.Id == id)
            .OrderByDescending(p => EF.Property<DateTime>(p, "valid_from"))
            .ToListAsync(ct);
    }
}
```

---

## 14. JSON COLUMNS

### 14.1 JSON Column Configuration (PostgreSQL)

```csharp
/// <summary>
/// JSON column - kompleks/dinamik veri için.
/// PostgreSQL jsonb type kullanır.
/// </summary>
public class Product : BaseAggregateRoot
{
    // JSON olarak saklanacak property
    public ProductAttributes Attributes { get; private set; } = new();
    public List<string> Tags { get; private set; } = [];
    public Dictionary<string, string> Metadata { get; private set; } = new();
}

public class ProductAttributes
{
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
}

// Configuration
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // JSON column (PostgreSQL jsonb)
        builder.Property(p => p.Attributes)
            .HasColumnName("attributes")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<ProductAttributes>(v, JsonOptions)!);

        builder.Property(p => p.Tags)
            .HasColumnName("tags")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions)!);

        builder.Property(p => p.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions)!);

        // JSON index (PostgreSQL GIN)
        builder.HasIndex(p => p.Attributes)
            .HasMethod("gin")
            .HasDatabaseName("ix_products_attributes_gin");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}

// JSON query örnekleri (PostgreSQL)
public async Task<List<Product>> GetByAttributeAsync(
    string color,
    CancellationToken ct)
{
    return await _context.Products
        .Where(p => EF.Functions.JsonContains(
            p.Attributes,
            new { color }))
        .ToListAsync(ct);
}

// Raw SQL ile JSON query
public async Task<List<Product>> SearchByTagAsync(
    string tag,
    CancellationToken ct)
{
    return await _context.Products
        .FromSqlRaw("""
            SELECT * FROM catalog.products
            WHERE tags @> @tag::jsonb
            """,
            new NpgsqlParameter("@tag", $"[\"{tag}\"]"))
        .ToListAsync(ct);
}
```

---

## 15. POSTGRESQL SPECIFIC FEATURES

### 15.1 PostgreSQL Extensions ve Özel Tipler

```csharp
// Migration'da extension aktifleştirme
public partial class EnableExtensions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // UUID generation
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\"");

        // Full-text search (Turkish)
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"unaccent\"");

        // Trigram similarity (LIKE/ILIKE optimizasyonu)
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"pg_trgm\"");

        // Geometric types
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"postgis\"");
    }
}

// PostgreSQL specific queries
public class ProductSearchService
{
    // Full-text search with ranking
    public async Task<List<ProductSearchResult>> FullTextSearchAsync(
        string query,
        CancellationToken ct)
    {
        var sql = """
            SELECT
                p.id,
                p.name,
                p.price_amount,
                ts_rank_cd(
                    to_tsvector('turkish', p.name || ' ' || COALESCE(p.description, '')),
                    plainto_tsquery('turkish', @query)
                ) AS rank
            FROM catalog.products p
            WHERE
                p.is_deleted = false
                AND p.is_active = true
                AND to_tsvector('turkish', p.name || ' ' || COALESCE(p.description, ''))
                    @@ plainto_tsquery('turkish', @query)
            ORDER BY rank DESC
            LIMIT 50
            """;

        return await _context.Database
            .SqlQueryRaw<ProductSearchResult>(sql, new NpgsqlParameter("@query", query))
            .ToListAsync(ct);
    }

    // Similarity search (fuzzy matching)
    public async Task<List<Product>> SimilaritySearchAsync(
        string term,
        CancellationToken ct)
    {
        return await _context.Products
            .Where(p => EF.Functions.TrigramsSimilarity(p.Name, term) > 0.3)
            .OrderByDescending(p => EF.Functions.TrigramsSimilarity(p.Name, term))
            .Take(20)
            .ToListAsync(ct);
    }

    // Array operations
    public async Task<List<Product>> GetByTagsAsync(
        string[] tags,
        CancellationToken ct)
    {
        return await _context.Products
            .Where(p => p.Tags.Any(t => tags.Contains(t)))
            .ToListAsync(ct);
    }
}
```

### 15.2 PostgreSQL Partitioning

```csharp
// Migration'da partition oluşturma
public partial class PartitionOrdersTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Partitioned table oluştur
        migrationBuilder.Sql("""
            CREATE TABLE ordering.orders_partitioned (
                id uuid NOT NULL,
                user_id uuid NOT NULL,
                total_amount decimal(18,2) NOT NULL,
                status varchar(20) NOT NULL,
                created_at timestamp with time zone NOT NULL,
                -- diğer kolonlar...
                PRIMARY KEY (id, created_at)
            ) PARTITION BY RANGE (created_at);
            """);

        // Aylık partition'lar oluştur
        migrationBuilder.Sql("""
            CREATE TABLE ordering.orders_2024_01
                PARTITION OF ordering.orders_partitioned
                FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');

            CREATE TABLE ordering.orders_2024_02
                PARTITION OF ordering.orders_partitioned
                FOR VALUES FROM ('2024-02-01') TO ('2024-03-01');

            -- Diğer aylar...
            """);
    }
}

// Otomatik partition oluşturma job
public class PartitionMaintenanceJob(ApplicationDbContext context) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        // Gelecek 3 ayın partition'larını oluştur
        var sql = """
            DO $$
            DECLARE
                start_date date;
                end_date date;
                partition_name text;
            BEGIN
                FOR i IN 0..2 LOOP
                    start_date := date_trunc('month', CURRENT_DATE + (i || ' months')::interval);
                    end_date := start_date + '1 month'::interval;
                    partition_name := 'orders_' || to_char(start_date, 'YYYY_MM');

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_tables
                        WHERE schemaname = 'ordering' AND tablename = partition_name
                    ) THEN
                        EXECUTE format(
                            'CREATE TABLE ordering.%I PARTITION OF ordering.orders_partitioned
                             FOR VALUES FROM (%L) TO (%L)',
                            partition_name, start_date, end_date
                        );
                    END IF;
                END LOOP;
            END $$;
            """;

        await context.Database.ExecuteSqlRawAsync(sql, ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

---

## 16. PERFORMANS MONITORING

### 16.1 Query Performance Monitoring

```csharp
/// <summary>
/// Query performance metrics toplama.
/// </summary>
public class QueryPerformanceService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ILogger<QueryPerformanceService> logger)
{
    // Yavaş sorguları bul
    public async Task<List<SlowQuery>> GetSlowQueriesAsync(
        int topN = 20,
        CancellationToken ct = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var sql = """
            SELECT
                queryid,
                query,
                calls,
                total_exec_time / calls as avg_time_ms,
                total_exec_time,
                rows / calls as avg_rows,
                shared_blks_hit,
                shared_blks_read
            FROM pg_stat_statements
            ORDER BY total_exec_time DESC
            LIMIT @topN
            """;

        return await context.Database
            .SqlQueryRaw<SlowQuery>(sql, new NpgsqlParameter("@topN", topN))
            .ToListAsync(ct);
    }

    // Table bloat kontrolü
    public async Task<List<TableBloat>> CheckTableBloatAsync(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var sql = """
            SELECT
                schemaname,
                tablename,
                pg_size_pretty(pg_total_relation_size(schemaname || '.' || tablename)) as total_size,
                pg_size_pretty(pg_table_size(schemaname || '.' || tablename)) as table_size,
                pg_size_pretty(pg_indexes_size(schemaname || '.' || tablename)) as index_size,
                n_dead_tup as dead_tuples,
                n_live_tup as live_tuples,
                round(100.0 * n_dead_tup / NULLIF(n_live_tup, 0), 2) as bloat_ratio
            FROM pg_stat_user_tables
            WHERE n_dead_tup > 1000
            ORDER BY n_dead_tup DESC
            """;

        return await context.Database
            .SqlQueryRaw<TableBloat>(sql)
            .ToListAsync(ct);
    }

    // Index kullanım istatistikleri
    public async Task<List<IndexUsage>> GetIndexUsageAsync(CancellationToken ct)
    {
        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var sql = """
            SELECT
                schemaname,
                tablename,
                indexname,
                idx_scan as index_scans,
                idx_tup_read as tuples_read,
                idx_tup_fetch as tuples_fetched,
                pg_size_pretty(pg_relation_size(indexrelid)) as index_size
            FROM pg_stat_user_indexes
            ORDER BY idx_scan ASC
            LIMIT 50
            """;

        return await context.Database
            .SqlQueryRaw<IndexUsage>(sql)
            .ToListAsync(ct);
    }
}

// Health check endpoint'inde kullanım
app.MapGet("/health/db-performance", async (QueryPerformanceService service, CancellationToken ct) =>
{
    var slowQueries = await service.GetSlowQueriesAsync(10, ct);
    var bloat = await service.CheckTableBloatAsync(ct);
    var unusedIndexes = await service.GetIndexUsageAsync(ct);

    return Results.Ok(new
    {
        slowQueries,
        tableBloat = bloat.Where(b => b.BloatRatio > 20),
        unusedIndexes = unusedIndexes.Where(i => i.IndexScans == 0)
    });
});
```

---

## 17. TEST VERİTABANLARI

### 17.1 Test Container Configuration

```csharp
/// <summary>
/// Integration test'ler için PostgreSQL container.
/// </summary>
public class PostgresTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public PostgresTestContainerFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("merge_test")
            .WithUsername("test")
            .WithPassword("test")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Migration uygula
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

/// <summary>
/// Integration test base class.
/// </summary>
[Collection("Database")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgresTestContainerFixture Fixture;
    protected ApplicationDbContext Context = null!;

    protected IntegrationTestBase(PostgresTestContainerFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(Fixture.ConnectionString)
            .Options;

        Context = new ApplicationDbContext(options);

        // Her test için temiz database
        await CleanDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    private async Task CleanDatabaseAsync()
    {
        // Tüm tabloları temizle (cascade)
        await Context.Database.ExecuteSqlRawAsync("""
            DO $$
            DECLARE
                r RECORD;
            BEGIN
                FOR r IN (SELECT tablename, schemaname FROM pg_tables
                          WHERE schemaname NOT IN ('pg_catalog', 'information_schema'))
                LOOP
                    EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.schemaname) || '.' ||
                            quote_ident(r.tablename) || ' CASCADE';
                END LOOP;
            END $$;
            """);
    }
}
```

### 17.2 In-Memory Database (Unit Tests)

```csharp
/// <summary>
/// Unit test'ler için in-memory database.
/// Dikkat: Production davranışını tam yansıtmaz!
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public static ApplicationDbContext CreateSqlite()
    {
        // SQLite - daha gerçekçi test
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}

// Kullanım
[Fact]
public async Task CreateProduct_ShouldPersist()
{
    // Arrange
    await using var context = TestDbContextFactory.CreateSqlite();
    var repository = new ProductRepository(context);

    var product = Product.Create("Test", "Desc", SKU.Create("TEST-001"),
        Money.Create(99.99m, "TRY"), 100, Guid.NewGuid());

    // Act
    await repository.AddAsync(product);
    await context.SaveChangesAsync();

    // Assert
    var saved = await repository.GetByIdAsync(product.Id);
    saved.Should().NotBeNull();
    saved!.Name.Should().Be("Test");
}
```

---

## 18. ANTI-PATTERNS

### 18.1 Kaçınılması Gereken Hatalar

```csharp
// ❌ YANLIŞ 1: N+1 Query Problem
public async Task<List<OrderDto>> GetOrdersBad()
{
    var orders = await _context.Orders.ToListAsync();

    foreach (var order in orders)
    {
        // Her order için ayrı sorgu!
        var items = await _context.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();
    }
}

// ✅ DOĞRU: Eager loading
public async Task<List<OrderDto>> GetOrdersGood()
{
    return await _context.Orders
        .Include(o => o.Items)
        .ToListAsync();
}


// ❌ YANLIŞ 2: Memory'de filtreleme
public async Task<List<Product>> GetActiveProductsBad()
{
    // Tüm ürünleri çekip memory'de filtrele
    var allProducts = await _context.Products.ToListAsync();
    return allProducts.Where(p => p.IsActive).ToList();
}

// ✅ DOĞRU: Database'de filtreleme
public async Task<List<Product>> GetActiveProductsGood()
{
    return await _context.Products
        .Where(p => p.IsActive)
        .ToListAsync();
}


// ❌ YANLIŞ 3: DbContext'i singleton olarak kullanmak
services.AddSingleton<ApplicationDbContext>(); // YANLIŞ!

// ✅ DOĞRU: Scoped lifetime
services.AddDbContext<ApplicationDbContext>(options => ...);


// ❌ YANLIŞ 4: Repository'de SaveChanges çağırmak
public class BadRepository
{
    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync(); // YANLIŞ!
    }
}

// ✅ DOĞRU: UnitOfWork pattern
public class GoodRepository
{
    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        // SaveChanges UnitOfWork'te çağrılır
    }
}


// ❌ YANLIŞ 5: Tracking'i yanlış kullanmak
public async Task UpdateProductBad(Guid id, string name)
{
    var product = await _context.Products
        .AsNoTracking() // Tracking kapalı
        .FirstOrDefaultAsync(p => p.Id == id);

    product!.SetName(name);
    await _context.SaveChangesAsync(); // Değişiklik kaydedilmez!
}

// ✅ DOĞRU: Update için tracking açık
public async Task UpdateProductGood(Guid id, string name)
{
    var product = await _context.Products
        .FirstOrDefaultAsync(p => p.Id == id);

    product!.SetName(name);
    await _context.SaveChangesAsync();
}


// ❌ YANLIŞ 6: Lazy loading'e güvenmek
public class Order
{
    public virtual ICollection<OrderItem> Items { get; set; } // Lazy loading
}

// ✅ DOĞRU: Explicit loading
public async Task<Order> GetOrderWithItems(Guid id)
{
    return await _context.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == id);
}


// ❌ YANLIŞ 7: String interpolation SQL injection
var name = userInput;
var sql = $"SELECT * FROM products WHERE name = '{name}'"; // SQL INJECTION!

// ✅ DOĞRU: Parameterized query
var sql = "SELECT * FROM products WHERE name = @name";
await _context.Products.FromSqlRaw(sql, new NpgsqlParameter("@name", name));


// ❌ YANLIŞ 8: Büyük transaction'lar
public async Task ProcessBigOrderBad()
{
    await using var transaction = await _context.Database.BeginTransactionAsync();

    // 10,000 ürün güncelle - çok uzun transaction
    foreach (var product in products)
    {
        product.UpdatePrice(...);
    }

    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}

// ✅ DOĞRU: Batch işlem
public async Task ProcessBigOrderGood()
{
    const int batchSize = 100;

    for (int i = 0; i < products.Count; i += batchSize)
    {
        var batch = products.Skip(i).Take(batchSize);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        foreach (var product in batch)
        {
            product.UpdatePrice(...);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
```

---

## CHECKLIST

### Query Yazarken
- [ ] Read-only sorgularda `AsNoTracking()` kullandım
- [ ] Çoklu Include'da `AsSplitQuery()` kullandım
- [ ] Sadece gerekli kolonları `Select` ile çektim
- [ ] Pagination uyguladım
- [ ] N+1 query kontrolü yaptım

### Entity Configuration
- [ ] Primary key tanımladım
- [ ] Foreign key'lere index ekledim
- [ ] Value Object'leri `OwnsOne` ile yapılandırdım
- [ ] Concurrency token (RowVersion) ekledim
- [ ] Soft delete filter'ı var

### Transaction
- [ ] `ExecuteInTransactionAsync` veya explicit transaction kullandım
- [ ] Hata durumunda rollback yapılıyor
- [ ] Outbox pattern ile event'ler kaydediliyor

### Performance
- [ ] Slow query threshold'u ayarlandı
- [ ] Index kullanım istatistiklerini kontrol ettim
- [ ] Connection pool ayarları yapıldı
- [ ] Compiled query'ler tanımlandı

---

*Bu kural dosyası, Merge E-Commerce Backend projesi için veritabanı ve EF Core kullanım standartlarını belirler.*
