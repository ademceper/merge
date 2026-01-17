---
description: Create new CQRS command with handler and validator
allowed-tools:
  - Read
  - Write
---

# Create CQRS Command

Scaffold a complete CQRS command with handler and validator.

## Required Input
- Module name (e.g., Product, Order, User)
- Command name (e.g., CreateProduct, UpdatePrice, DeleteOrder)
- Properties for the command

## File Structure

```
Merge.Application/{Module}/Commands/{CommandName}/
├── {CommandName}Command.cs
├── {CommandName}CommandHandler.cs
└── {CommandName}CommandValidator.cs
```

## Templates

### Command
```csharp
namespace Merge.Application.{Module}.Commands.{CommandName};

public record {CommandName}Command(
    // Add properties here
    {Properties}
) : IRequest<{ReturnType}>;
```

### Handler
```csharp
namespace Merge.Application.{Module}.Commands.{CommandName};

public class {CommandName}CommandHandler(
    IRepository<{Entity}> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICacheService cache,
    ILogger<{CommandName}CommandHandler> logger
) : IRequestHandler<{CommandName}Command, {ReturnType}>
{
    public async Task<{ReturnType}> Handle({CommandName}Command request, CancellationToken ct)
    {
        // 1. Create/Get domain entity
        // 2. Apply business logic
        // 3. Persist changes
        await unitOfWork.SaveChangesAsync(ct);

        // 4. Invalidate cache
        await cache.RemoveByPrefixAsync("{module}_", ct);

        // 5. Log
        logger.LogInformation("{CommandName} executed: {Id}", result.Id);

        // 6. Return DTO
        return mapper.Map<{ReturnType}>(entity);
    }
}
```

### Validator
```csharp
namespace Merge.Application.{Module}.Commands.{CommandName};

public class {CommandName}CommandValidator : AbstractValidator<{CommandName}Command>
{
    public {CommandName}CommandValidator()
    {
        // Add validation rules
        RuleFor(x => x.PropertyName)
            .NotEmpty().WithMessage("PropertyName is required");
    }
}
```

## Steps

1. Ask for module name if not provided
2. Ask for command name if not provided
3. Ask for properties with types
4. Ask for return type (usually a DTO)
5. Generate all three files
6. Add using statements as needed

## Example

Input: "Create a command to update product price"

Output:
- `Merge.Application/Product/Commands/UpdateProductPrice/UpdateProductPriceCommand.cs`
- `Merge.Application/Product/Commands/UpdateProductPrice/UpdateProductPriceCommandHandler.cs`
- `Merge.Application/Product/Commands/UpdateProductPrice/UpdateProductPriceCommandValidator.cs`
