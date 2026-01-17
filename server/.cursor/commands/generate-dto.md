---
title: Generate DTOs
description: Creates request/response DTOs with AutoMapper profile
---

Generate complete DTO structure for an entity:

**Files to create:**
```
Merge.Application/{Module}/Dtos/
├── {Entity}Dto.cs
├── {Entity}ListDto.cs
├── Create{Entity}Request.cs
├── Update{Entity}Request.cs
└── {Entity}MappingProfile.cs
```

**Templates:**

### Response DTO (full details)
```csharp
public record {Entity}Dto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    // Nested DTOs for related entities
    public CategoryDto? Category { get; init; }
    public IReadOnlyList<ImageDto> Images { get; init; } = [];
}
```

### List DTO (minimal, for listings)
```csharp
public record {Entity}ListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string? ThumbnailUrl { get; init; }
}
```

### Request DTOs
```csharp
public record Create{Entity}Request(
    string Name,
    decimal Price,
    Guid CategoryId);

public record Update{Entity}Request(
    string? Name,
    decimal? Price);
```

### AutoMapper Profile
```csharp
public class {Entity}MappingProfile : Profile
{
    public {Entity}MappingProfile()
    {
        CreateMap<{Entity}, {Entity}Dto>()
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Amount));

        CreateMap<{Entity}, {Entity}ListDto>()
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(
                s => s.Images.FirstOrDefault(i => i.IsPrimary)!.Url));

        CreateMap<Create{Entity}Request, Create{Entity}Command>();
        CreateMap<Update{Entity}Request, Update{Entity}Command>();
    }
}
```

Ask for: Entity name, Properties to include, Nested entities
