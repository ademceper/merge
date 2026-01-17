---
title: Create Domain Entity
description: Scaffolds a complete DDD entity with value objects and events
---

Create a complete DDD entity structure:

**Files to create:**
```
Merge.Domain/{Module}/Entities/
├── {EntityName}.cs
├── Events/
│   ├── {EntityName}CreatedEvent.cs
│   └── {EntityName}UpdatedEvent.cs
└── Specifications/
    └── {EntityName}Specification.cs

Merge.Infrastructure/Data/Configurations/
└── {EntityName}Configuration.cs
```

**Entity Template:**
```csharp
public class {EntityName} : BaseAggregateRoot
{
    // Private setters
    public string Name { get; private set; } = null!;

    // Private constructor (EF Core)
    private {EntityName}() { }

    // Factory method
    public static {EntityName} Create(...)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));

        var entity = new {EntityName}
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        entity.AddDomainEvent(new {EntityName}CreatedEvent(entity.Id));
        return entity;
    }

    // Domain methods
    public void Update(...) { ... }
}
```

**Configuration Template:**
```csharp
public class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.ToTable("{entity_names}");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
```

Ask for: Module, Entity name, Properties, Related entities
