---
description: Analyze code quality, architecture, and suggest improvements
allowed-tools:
  - Read
  - Glob
  - Grep
  - Bash(dotnet build)
---

# Analyze Code

Perform comprehensive code analysis for quality, architecture compliance, and best practices.

## Analysis Types

### 1. Architecture Analysis
Check Clean Architecture compliance:
- Domain has no external dependencies
- Application depends only on Domain
- Infrastructure implements interfaces
- API depends on Application

### 2. DDD Compliance
Check Domain-Driven Design patterns:
- Factory methods instead of constructors
- Domain methods instead of property setters
- Domain events for state changes
- Value objects for complex values
- Guard clauses for validation

### 3. CQRS Compliance
Check Command/Query separation:
- Commands change state
- Queries don't change state
- Proper return types
- Validation on commands

### 4. Code Quality
Check general quality:
- Naming conventions
- Method length (max 50 lines)
- Class size (max 500 lines)
- Cyclomatic complexity
- Code duplication

### 5. Security Analysis
Check security concerns:
- No hardcoded secrets
- Input validation
- Authorization checks
- SQL injection prevention
- XSS prevention

### 6. Performance Analysis
Check performance patterns:
- AsNoTracking() for reads
- AsSplitQuery() for includes
- Proper indexing
- N+1 query detection
- Caching usage

## Checklist Templates

### Entity Checklist
- [ ] Factory method exists
- [ ] Private constructor
- [ ] Domain events for state changes
- [ ] Guard clauses for validation
- [ ] No public setters
- [ ] Value objects for complex types
- [ ] Soft delete implementation

### Handler Checklist
- [ ] Uses CancellationToken
- [ ] Proper error handling
- [ ] Logging included
- [ ] Cache invalidation
- [ ] Returns DTO (not entity)

### Controller Checklist
- [ ] Proper HTTP methods
- [ ] Authorization attributes
- [ ] ProducesResponseType attributes
- [ ] CancellationToken parameter
- [ ] Validation handling

### Repository Checklist
- [ ] AsNoTracking() for reads
- [ ] Specification pattern
- [ ] No SaveChanges (in UoW)
- [ ] Proper includes

## Commands

```bash
# Build check
dotnet build

# Find large files (>500 lines)
find . -name "*.cs" -exec wc -l {} + | sort -rn | head -20

# Find TODO/FIXME comments
grep -rn "TODO\|FIXME\|HACK" --include="*.cs"

# Find public setters in Domain
grep -rn "{ get; set; }" ./Merge.Domain/Entities/

# Find direct constructor calls (should use factory)
grep -rn "new Product(" --include="*.cs" | grep -v "Test"

# Find missing CancellationToken
grep -rn "async Task" --include="*.cs" | grep -v "CancellationToken"
```

## Output Format

```markdown
# Code Analysis Report

## Summary
- Total files analyzed: X
- Issues found: Y
- Severity: High/Medium/Low

## Architecture Issues
1. [HIGH] Domain.Entity depends on Infrastructure.Service
   - File: Domain/Entities/Product.cs:45
   - Fix: Move dependency to Application layer

## DDD Issues
1. [MEDIUM] Entity uses public setter instead of method
   - File: Domain/Entities/Order.cs:23
   - Fix: Replace `public string Status { get; set; }` with `SetStatus()` method

## Security Issues
1. [HIGH] Potential SQL injection
   - File: Infrastructure/Repositories/ProductRepository.cs:67
   - Fix: Use parameterized query

## Performance Issues
1. [MEDIUM] Missing AsNoTracking()
   - File: Application/Queries/GetProductsQueryHandler.cs:34
   - Fix: Add `.AsNoTracking()` to query

## Recommendations
1. Add 15 more unit tests for 60% coverage
2. Split ProductService (847 lines) into smaller services
3. Add missing indexes on Order.UserId
```

## Steps

1. Determine analysis scope (file/folder/project)
2. Run build to check for errors
3. Check architecture compliance
4. Check DDD patterns
5. Check code quality metrics
6. Check security issues
7. Check performance patterns
8. Generate report with findings
9. Prioritize issues by severity
10. Suggest fixes for each issue
