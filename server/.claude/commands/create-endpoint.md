---
description: Create new API endpoint with controller action
allowed-tools:
  - Read
  - Write
  - Glob
---

# Create API Endpoint

Add a new endpoint to an existing controller or create a new controller.

## Required Input
- Resource name (e.g., products, orders, users)
- HTTP method (GET, POST, PUT, PATCH, DELETE)
- Action name (if not CRUD)
- Route template
- Request/Response types

## Endpoint Types

### CRUD Endpoints
```
GET    /api/v1/{resource}           -> GetAll
GET    /api/v1/{resource}/{id}      -> GetById
POST   /api/v1/{resource}           -> Create
PUT    /api/v1/{resource}/{id}      -> Update
PATCH  /api/v1/{resource}/{id}      -> Patch
DELETE /api/v1/{resource}/{id}      -> Delete
```

### Custom Action Endpoints
```
POST   /api/v1/{resource}/{id}/activate     -> Activate
POST   /api/v1/{resource}/{id}/deactivate   -> Deactivate
POST   /api/v1/{resource}/{id}/publish      -> Publish
POST   /api/v1/orders/{id}/cancel           -> CancelOrder
POST   /api/v1/orders/{id}/ship             -> ShipOrder
```

### Nested Resource Endpoints
```
GET    /api/v1/{resource}/{id}/reviews      -> GetReviews
POST   /api/v1/{resource}/{id}/reviews      -> AddReview
GET    /api/v1/{resource}/{id}/images       -> GetImages
POST   /api/v1/{resource}/{id}/images       -> UploadImage
```

## Templates

### GET (List)
```csharp
/// <summary>
/// Get all {resources} with pagination.
/// </summary>
[HttpGet]
[AllowAnonymous]
[ResponseCache(Duration = 60, VaryByQueryKeys = ["page", "pageSize"])]
[ProducesResponseType(typeof(PagedResult<{Resource}Dto>), StatusCodes.Status200OK)]
public async Task<ActionResult<PagedResult<{Resource}Dto>>> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null,
    [FromQuery] string sortBy = "createdAt",
    [FromQuery] string sortOrder = "desc",
    CancellationToken ct = default)
{
    var query = new Get{Resources}Query(page, pageSize, search, sortBy, sortOrder);
    var result = await Mediator.Send(query, ct);

    Response.AddPaginationHeaders(result);
    return Ok(result);
}
```

### GET (Single)
```csharp
/// <summary>
/// Get {resource} by ID.
/// </summary>
[HttpGet("{id:guid}")]
[AllowAnonymous]
[ProducesResponseType(typeof({Resource}Dto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<{Resource}Dto>> GetById(
    Guid id,
    CancellationToken ct = default)
{
    var query = new Get{Resource}ByIdQuery(id);
    var result = await Mediator.Send(query, ct);

    if (result is null)
        return NotFound();

    return Ok(result);
}
```

### POST
```csharp
/// <summary>
/// Create a new {resource}.
/// </summary>
[HttpPost]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof({Resource}Dto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public async Task<ActionResult<{Resource}Dto>> Create(
    [FromBody] Create{Resource}Command command,
    CancellationToken ct = default)
{
    var result = await Mediator.Send(command, ct);

    return CreatedAtAction(
        nameof(GetById),
        new { id = result.Id },
        result);
}
```

### PUT
```csharp
/// <summary>
/// Update {resource} (full replacement).
/// </summary>
[HttpPut("{id:guid}")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof({Resource}Dto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<{Resource}Dto>> Update(
    Guid id,
    [FromBody] Update{Resource}Command command,
    CancellationToken ct = default)
{
    if (id != command.Id)
        return BadRequest("ID mismatch");

    var result = await Mediator.Send(command, ct);
    return Ok(result);
}
```

### PATCH
```csharp
/// <summary>
/// Partially update {resource}.
/// </summary>
[HttpPatch("{id:guid}")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof({Resource}Dto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<{Resource}Dto>> Patch(
    Guid id,
    [FromBody] Patch{Resource}Command command,
    CancellationToken ct = default)
{
    command = command with { Id = id };
    var result = await Mediator.Send(command, ct);
    return Ok(result);
}
```

### DELETE
```csharp
/// <summary>
/// Delete {resource}.
/// </summary>
[HttpDelete("{id:guid}")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(
    Guid id,
    CancellationToken ct = default)
{
    await Mediator.Send(new Delete{Resource}Command(id), ct);
    return NoContent();
}
```

### Custom Action
```csharp
/// <summary>
/// {ActionDescription}
/// </summary>
[HttpPost("{id:guid}/{action}")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof({Resource}Dto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
public async Task<ActionResult<{Resource}Dto>> {Action}(
    Guid id,
    CancellationToken ct = default)
{
    var result = await Mediator.Send(new {Action}{Resource}Command(id), ct);
    return Ok(result);
}
```

## Steps

1. Ask for resource name
2. Ask for endpoint type (CRUD/Custom/Nested)
3. Ask for HTTP method
4. Ask for authorization requirements
5. Determine if controller exists or needs creation
6. Generate endpoint code
7. Add to controller file

## Checklist

- [ ] Proper HTTP method used
- [ ] Route follows REST conventions
- [ ] Authorization attributes added
- [ ] Response types documented
- [ ] CancellationToken parameter included
- [ ] Appropriate status codes returned
