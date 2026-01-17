---
title: Create Value Object
description: Scaffolds an immutable value object with validation
---

Create a complete value object:

**File to create:**
```
Merge.Domain/Common/ValueObjects/{Name}.cs
```

**Value Object Template:**
```csharp
/// <summary>
/// Value object representing {description}.
/// Immutable and self-validating.
/// </summary>
public sealed record {Name} : IComparable<{Name}>
{
    public {Type} Value { get; }

    private {Name}({Type} value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new {Name} instance.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static {Name} Create({Type} value)
    {
        Guard.AgainstNullOrEmpty(value, nameof(value));

        // Add custom validation
        if (!IsValid(value))
            throw new DomainException("{Name} format is invalid");

        return new {Name}(Normalize(value));
    }

    /// <summary>
    /// Tries to create a {Name} without throwing exceptions.
    /// </summary>
    public static bool TryCreate({Type} value, out {Name}? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value) || !IsValid(value))
            return false;

        result = new {Name}(Normalize(value));
        return true;
    }

    private static bool IsValid({Type} value)
    {
        // Validation logic
        return true;
    }

    private static {Type} Normalize({Type} value)
    {
        // Normalization logic
        return value.Trim();
    }

    public int CompareTo({Name}? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    public override string ToString() => Value;

    // Implicit conversion
    public static implicit operator {Type}({Name} obj) => obj.Value;
}
```

**EF Core Value Conversion:**
```csharp
// In entity configuration
builder.Property(x => x.{PropertyName})
    .HasConversion(
        v => v.Value,
        v => {Name}.Create(v))
    .HasMaxLength(100);
```

Common Value Objects:
- Email, Phone, Money, Address
- SKU, Slug, Percentage
- DateRange, QuantityRange

Ask for: Name, Underlying type, Validation rules
