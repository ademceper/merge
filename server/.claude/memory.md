# Claude Code Memory Bank

Bu dosya Claude Code'un projeni daha iyi anlaması için kullanılır.
Her oturumda otomatik olarak okunur.

## Proje Özeti

**Proje:** Merge E-Commerce Backend
**Teknolojiler:** .NET 9.0, C# 12, PostgreSQL, Redis, Docker
**Mimari:** Clean Architecture + DDD + CQRS + MediatR

## Katman Yapısı

```
Merge.API/           → Controllers, Middleware, Filters
Merge.Application/   → Commands, Queries, Handlers, DTOs, Validators
Merge.Domain/        → Entities, ValueObjects, Events, Specifications
Merge.Infrastructure/→ DbContext, Repositories, External Services
Merge.Tests/         → Unit, Integration, API Tests
```

## Önemli Kurallar

1. **Domain katmanı hiçbir şeye bağımlı OLMAMALI**
2. **Entity'lerde public setter YOK** - Factory method kullan
3. **Query'ler state değiştirmemeli** - AsNoTracking kullan
4. **Command'lar DTO dönmeli** - Entity değil

## Sık Kullanılan Komutlar

```bash
# Build
dotnet build Merge.sln

# Test
dotnet test Merge.Tests

# Migration
dotnet ef migrations add {Name} --project Merge.Infrastructure --startup-project Merge.API

# Run
dotnet run --project Merge.API
```

## Öğrenilen Kalıplar

<!-- Claude Code buraya öğrendiği kalıpları ekleyecek -->

### Entity Oluşturma
```csharp
public class Entity : BaseEntity
{
    private Entity() { } // EF Core için

    public static Entity Create(...)
    {
        var entity = new Entity { ... };
        entity.AddDomainEvent(new EntityCreatedEvent(entity.Id));
        return entity;
    }
}
```

### Command Handler
```csharp
public class CreateEntityCommandHandler(
    IRepository<Entity> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<CreateEntityCommand, EntityDto>
{
    public async Task<EntityDto> Handle(CreateEntityCommand request, CancellationToken ct)
    {
        var entity = Entity.Create(...);
        await repository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return mapper.Map<EntityDto>(entity);
    }
}
```

### Query Handler
```csharp
public class GetEntityQueryHandler(
    ApplicationDbContext context,
    IMapper mapper
) : IRequestHandler<GetEntityQuery, EntityDto?>
{
    public async Task<EntityDto?> Handle(GetEntityQuery request, CancellationToken ct)
    {
        return await context.Entities
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .ProjectTo<EntityDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }
}
```

## Son Değişiklikler

<!-- En son yapılan önemli değişiklikler -->

---
*Bu dosya Claude Code tarafından otomatik güncellenir*
