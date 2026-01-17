---
title: Refactor Code
description: Applies C# 12 and .NET 9 modern patterns to legacy code
---

Modernize code using C# 12 and .NET 9 patterns:

## Refactoring Patterns

### 1. Primary Constructors
```csharp
// Before
public class ProductService
{
    private readonly IRepository<Product> _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IRepository<Product> repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// After
public class ProductService(
    IRepository<Product> repository,
    ILogger<ProductService> logger)
{
    // Use parameters directly
}
```

### 2. Collection Expressions
```csharp
// Before
var list = new List<int> { 1, 2, 3 };
var array = new int[] { 1, 2, 3 };
var empty = Enumerable.Empty<int>();

// After
List<int> list = [1, 2, 3];
int[] array = [1, 2, 3];
int[] empty = [];
```

### 3. Pattern Matching
```csharp
// Before
if (obj != null && obj is Product && ((Product)obj).IsActive)

// After
if (obj is Product { IsActive: true } product)
```

### 4. Raw String Literals
```csharp
// Before
var json = "{\n  \"name\": \"value\"\n}";

// After
var json = """
    {
      "name": "value"
    }
    """;
```

### 5. Required Members
```csharp
// Before
public class Product
{
    public string Name { get; set; } = null!;
}

// After
public class Product
{
    public required string Name { get; init; }
}
```

### 6. File-Scoped Types
```csharp
// Before
internal class Helper { }

// After
file class Helper { }
```

### 7. Switch Expressions
```csharp
// Before
string GetStatus(OrderStatus status)
{
    switch (status)
    {
        case OrderStatus.Pending: return "Bekliyor";
        case OrderStatus.Shipped: return "Kargoya verildi";
        default: return "Bilinmiyor";
    }
}

// After
string GetStatus(OrderStatus status) => status switch
{
    OrderStatus.Pending => "Bekliyor",
    OrderStatus.Shipped => "Kargoya verildi",
    _ => "Bilinmiyor"
};
```

### 8. Target-Typed New
```csharp
// Before
Dictionary<string, List<Product>> dict = new Dictionary<string, List<Product>>();

// After
Dictionary<string, List<Product>> dict = new();
```

## Apply Refactoring

1. Analyze current file for old patterns
2. Suggest modern alternatives
3. Apply changes preserving behavior
4. Run tests to verify

Ask: Which patterns to apply? (all/specific)
