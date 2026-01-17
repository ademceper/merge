---
description: Create new CQRS query with handler
allowed-tools:
  - Read
  - Write
---

# Create CQRS Query

Scaffold a complete CQRS query with handler.

## Required Input
- Module name (e.g., Product, Order, User)
- Query name (e.g., GetProductById, GetAllOrders, SearchProducts)
- Query parameters

## File Structure

```
Merge.Application/{Module}/Queries/{QueryName}/
├── {QueryName}Query.cs
├── {QueryName}QueryHandler.cs
└── {QueryName}QueryValidator.cs (optional)
```

## Templates

### Query
```csharp
namespace Merge.Application.{Module}.Queries.{QueryName};

public record {QueryName}Query(
    {Parameters}
) : IRequest<{ReturnType}>;
```

### Handler (Single Item)
```csharp
namespace Merge.Application.{Module}.Queries.{QueryName};

public class {QueryName}QueryHandler(
    IRepository<{Entity}> repository,
    IMapper mapper,
    ICacheService cache,
    ILogger<{QueryName}QueryHandler> logger
) : IRequestHandler<{QueryName}Query, {ReturnType}?>
{
    public async Task<{ReturnType}?> Handle({QueryName}Query request, CancellationToken ct)
    {
        // 1. Try cache first
        var cacheKey = $"{module}_{request.Id}";
        var cached = await cache.GetAsync<{ReturnType}>(cacheKey, ct);
        if (cached != null)
            return cached;

        // 2. Query database (with specification)
        var spec = new {Entity}ByIdSpec(request.Id);
        var entity = await repository.GetBySpecAsync(spec, ct);

        if (entity == null)
            return null;

        // 3. Map to DTO
        var dto = mapper.Map<{ReturnType}>(entity);

        // 4. Cache result
        await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15), ct);

        return dto;
    }
}
```

### Handler (List with Pagination)
```csharp
namespace Merge.Application.{Module}.Queries.{QueryName};

public class {QueryName}QueryHandler(
    IRepository<{Entity}> repository,
    IMapper mapper,
    ICacheService cache
) : IRequestHandler<{QueryName}Query, PagedResult<{ReturnType}>>
{
    public async Task<PagedResult<{ReturnType}>> Handle({QueryName}Query request, CancellationToken ct)
    {
        // 1. Try cache
        var cacheKey = $"{module}_page_{request.Page}_{request.PageSize}";
        var cached = await cache.GetAsync<PagedResult<{ReturnType}>>(cacheKey, ct);
        if (cached != null)
            return cached;

        // 2. Create specification
        var spec = new {Entity}PaginatedSpec(request.Page, request.PageSize);

        // 3. Get data and count
        var items = await repository.ListAsync(spec, ct);
        var totalCount = await repository.CountAsync(new {Entity}CountSpec(), ct);

        // 4. Create result
        var dtos = mapper.Map<List<{ReturnType}>>(items);
        var result = new PagedResult<{ReturnType}>(
            Items: dtos,
            TotalCount: totalCount,
            Page: request.Page,
            PageSize: request.PageSize
        );

        // 5. Cache
        await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);

        return result;
    }
}
```

## Query Types

### GetById
```csharp
public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDto?>;
```

### GetAll with Pagination
```csharp
public record GetAllProductsQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<ProductDto>>;
```

### Search/Filter
```csharp
public record SearchProductsQuery(
    string? SearchTerm,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductDto>>;
```

## Steps

1. Ask for query type (GetById, GetAll, Search)
2. Ask for parameters
3. Generate query and handler
4. Add specification if needed
