---
description: Analyze and fix build or runtime errors
allowed-tools:
  - Bash(dotnet build)
  - Bash(dotnet test)
  - Read
  - Edit
  - Glob
  - Grep
---

# Fix Error

Analyze compilation errors, test failures, or runtime exceptions and fix them.

## Error Types

### 1. Build Errors (CS*)
```bash
dotnet build 2>&1 | head -100
```

Common errors:
- CS0103: Name does not exist
- CS0246: Type or namespace not found
- CS0266: Cannot implicitly convert type
- CS0535: Interface member not implemented
- CS1061: Type does not contain definition
- CS7036: Required argument not provided

### 2. Test Failures
```bash
dotnet test --no-build --filter "FullyQualifiedName~TestName" -v d
```

### 3. Runtime Exceptions
- NullReferenceException
- InvalidOperationException
- DbUpdateException
- ValidationException

## Analysis Steps

1. **Get error details**
   - Read full error message
   - Note file and line number
   - Identify error code

2. **Understand context**
   - Read the file with error
   - Check related files
   - Review recent changes

3. **Identify root cause**
   - Missing using statement?
   - Type mismatch?
   - Missing implementation?
   - Null reference?

4. **Fix the error**
   - Make minimal change
   - Preserve existing behavior
   - Follow project patterns

5. **Verify fix**
   - Run build again
   - Run related tests
   - Check for new errors

## Common Fixes

### Missing Using Statement
```csharp
// Error: CS0246 - The type 'Money' could not be found
// Fix: Add using statement
using Merge.Domain.ValueObjects;
```

### Missing Interface Implementation
```csharp
// Error: CS0535 - 'Class' does not implement interface member
// Fix: Implement missing method
public async Task<T> GetByIdAsync(Guid id, CancellationToken ct)
{
    // Implementation
}
```

### Null Reference
```csharp
// Error: NullReferenceException
// Fix: Add null check
var product = await _repository.GetByIdAsync(id, ct)
    ?? throw new NotFoundException($"Product {id} not found");
```

### Type Mismatch
```csharp
// Error: CS0266 - Cannot convert 'decimal' to 'Money'
// Fix: Use proper conversion
var price = Money.Create(command.Price, command.Currency);
```

### Missing Primary Constructor Parameter
```csharp
// Error: CS7036 - There is no argument given that corresponds to the required parameter
// Fix: Add missing parameter to constructor
public class Handler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork,  // Add missing parameter
    IMapper mapper)
```

## Workflow

```
1. Run: dotnet build
   ↓
2. If error → Read error message
   ↓
3. Open file at line number
   ↓
4. Analyze context
   ↓
5. Apply fix
   ↓
6. Run: dotnet build again
   ↓
7. If more errors → Repeat from step 2
   ↓
8. If no errors → Run tests
```

## Guidelines

- Fix one error at a time
- Some errors cause cascading errors - fix the first one
- Don't suppress errors without understanding
- Follow existing code patterns
- Run tests after fixing build errors
