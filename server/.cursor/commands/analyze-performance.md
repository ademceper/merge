---
title: Analyze Performance
description: Analyzes code for performance issues and N+1 queries
---

Analyze the current file or selection for performance issues:

## Check List

### 1. Database Queries
- [ ] N+1 query detection (`.Include()` missing)
- [ ] AsNoTracking for read-only queries
- [ ] AsSplitQuery for multiple includes
- [ ] Projection instead of full entity loading
- [ ] Unnecessary ToList() before filtering

### 2. Async Patterns
- [ ] Async all the way (no .Result or .Wait())
- [ ] ConfigureAwait(false) in library code
- [ ] CancellationToken propagation
- [ ] Parallel processing for independent operations

### 3. Memory
- [ ] Large object allocations
- [ ] String concatenation in loops (use StringBuilder)
- [ ] Span<T> for string operations
- [ ] ArrayPool for large buffers
- [ ] IAsyncEnumerable for streaming

### 4. Caching
- [ ] Cache for frequently accessed data
- [ ] Appropriate cache duration
- [ ] Cache invalidation strategy

### 5. LINQ
- [ ] Any() instead of Count() > 0
- [ ] FirstOrDefault() instead of Where().First()
- [ ] Late materialization (ToList at the end)

## Analysis Commands

```bash
# EF Core query logging
dotnet run -- --verbose

# Memory profiling
dotnet-counters monitor --process-id <PID>

# HTTP benchmarking
bombardier -c 100 -n 10000 https://localhost:5001/api/v1/products
```

Output format:
- **Critical**: Performance bug, immediate fix required
- **Warning**: Potential issue, review needed
- **Info**: Optimization opportunity
