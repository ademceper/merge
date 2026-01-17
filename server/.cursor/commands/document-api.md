---
title: Document API
description: Generates OpenAPI documentation and XML comments
---

Generate comprehensive API documentation:

## 1. Controller Documentation

```csharp
/// <summary>
/// Manages product operations.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase
{
    /// <summary>
    /// Gets all products with pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns the paginated list</response>
    /// <response code="400">Invalid pagination parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        // ...
    }
}
```

## 2. DTO Documentation

```csharp
/// <summary>
/// Product data transfer object.
/// </summary>
public record ProductDto
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Product name.
    /// </summary>
    /// <example>iPhone 15 Pro</example>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Price in the default currency.
    /// </summary>
    /// <example>1299.99</example>
    public decimal Price { get; init; }
}
```

## 3. OpenAPI Extensions

```csharp
// Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Merge E-Commerce API",
        Version = "v1",
        Description = "Backend API for Merge E-Commerce platform",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "api@merge.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    // JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});
```

## 4. Generate Documentation

```bash
# Enable XML documentation in .csproj
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>

# Generate OpenAPI spec
dotnet swagger tofile --output swagger.json Merge.API.dll v1
```

Analyze endpoint and add missing documentation.
