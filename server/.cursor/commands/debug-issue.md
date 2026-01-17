---
title: Debug Issue
description: Helps debug and trace issues in the codebase
---

Debug the described issue systematically:

## 1. Gather Information

Ask for:
- Error message or unexpected behavior
- Steps to reproduce
- Expected vs actual behavior
- Related code files

## 2. Analysis Steps

### Check Logs
```bash
# Recent logs
tail -100 logs/merge-*.log | grep -i error

# Filter by correlation ID
grep "correlation-id-here" logs/*.log
```

### Trace Request Flow
```
HTTP Request
  → Controller (validate request)
    → MediatR Handler (business logic)
      → Domain Entity (state change)
        → Repository (persistence)
          → Database
```

### Common Issues

**1. NullReferenceException**
- Check null guards
- Verify navigation property loaded
- Check factory method called

**2. Validation Errors**
- Check FluentValidation rules
- Verify request DTO mapping
- Check guard clauses

**3. Database Errors**
- Check connection string
- Verify migration applied
- Check constraint violations

**4. 401/403 Errors**
- Check JWT token valid
- Verify authorization policy
- Check claims/roles

**5. N+1 Queries**
- Add .Include() for navigation
- Use AsSplitQuery()
- Check lazy loading

## 3. Debugging Tools

```csharp
// Add temporary logging
logger.LogDebug("Checkpoint 1: {Value}", variable);

// Check EF queries
_context.Database.Log = Console.WriteLine;

// Check request/response
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Path}");
    await next();
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});
```

## 4. Fix and Verify

1. Identify root cause
2. Propose fix
3. Add test for the bug
4. Verify fix locally
