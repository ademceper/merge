---
title: Create CQRS Query
description: Scaffolds query with handler
---

Create a complete CQRS query:

**Files to create:**
```
Merge.Application/{Module}/Queries/{Name}/
├── {Name}Query.cs
└── {Name}QueryHandler.cs
```

**Template:**
- Query: `public record {Name}Query(...) : IRequest<{Dto}>`
- Handler: Uses cache, specification pattern, AsNoTracking

Query types:
- GetById: Returns single item or null
- GetAll: Returns PagedResult<T>
- Search: Returns filtered PagedResult<T>

Ask for: Module, Query type, Parameters
