---
description: Create new DDD entity with factory method and domain events
allowed-tools:
  - Read
  - Write
  - Glob
---

# Create DDD Entity

Scaffold a complete DDD entity with factory method, domain events, and value objects.

## Required Input
- Entity name (e.g., Product, Order, User)
- Properties with types
- Value objects to use (Money, Email, Address, etc.)
- Parent aggregate (if sub-entity)

## File Structure

```
Merge.Domain/Entities/{EntityName}.cs
Merge.Domain/Events/{EntityName}CreatedEvent.cs
Merge.Infrastructure/Data/Configurations/{EntityName}Configuration.cs
```

## Templates

### Entity
```csharp
namespace Merge.Domain.Entities;

/// <summary>
/// {EntityName} aggregate root.
/// </summary>
public sealed class {EntityName} : BaseAggregateRoot
{
    // ==========================================
    // PROPERTIES
    // ==========================================

    public string Name { get; private set; } = null!;
    // Add more properties

    // ==========================================
    // NAVIGATION PROPERTIES
    // ==========================================

    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    // ==========================================
    // COLLECTIONS
    // ==========================================

    private readonly List<{Child}> _{children} = [];
    public IReadOnlyCollection<{Child}> {Children} => _{children}.AsReadOnly();

    // ==========================================
    // CONSTRUCTORS
    // ==========================================

    private {EntityName}() { } // EF Core

    // ==========================================
    // FACTORY METHOD
    // ==========================================

    public static {EntityName} Create(
        string name,
        // other parameters
        Guid categoryId)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.Default(categoryId, nameof(categoryId));

        var entity = new {EntityName}
        {
            Id = Guid.NewGuid(),
            Name = name,
            CategoryId = categoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        entity.AddDomainEvent(new {EntityName}CreatedEvent(entity.Id, entity.Name));

        return entity;
    }

    // ==========================================
    // DOMAIN METHODS
    // ==========================================

    public void SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));

        if (Name == name) return;

        var oldName = Name;
        Name = name;

        AddDomainEvent(new {EntityName}NameChangedEvent(Id, oldName, name));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        AddDomainEvent(new {EntityName}DeactivatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        AddDomainEvent(new {EntityName}ActivatedEvent(Id));
    }
}
```

### Domain Event
```csharp
namespace Merge.Domain.Events;

/// <summary>
/// {EntityName} created domain event.
/// </summary>
public sealed record {EntityName}CreatedEvent(
    Guid {EntityName}Id,
    string {EntityName}Name
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

### Entity Configuration
```csharp
namespace Merge.Infrastructure.Data.Configurations;

public class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        // Table
        builder.ToTable("{table_name}", "{schema}");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Value Objects
        builder.OwnsOne(e => e.Price, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("price_amount")
                .HasPrecision(18, 2);
            price.Property(m => m.Currency)
                .HasColumnName("price_currency")
                .HasMaxLength(3);
        });

        // Relationships
        builder.HasOne(e => e.Category)
            .WithMany(c => c.{Entities})
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.CategoryId);

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
```

## Steps

1. Ask for entity name
2. Ask for properties (name, type, required?, value object?)
3. Ask for relationships
4. Ask for schema name
5. Generate Entity file
6. Generate Domain Events
7. Generate Configuration file
8. List generated files

## Guidelines

- Use factory method pattern (no public constructor)
- Add Guard clauses for validation
- Raise domain events for state changes
- Use value objects for complex types
- Configure soft delete filter
- Add appropriate indexes
