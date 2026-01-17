---
title: Database Migration
description: Creates and applies EF Core migration
---

Create EF Core migration:

```bash
# Create
dotnet ef migrations add {Name} \
    --project Merge.Infrastructure \
    --startup-project Merge.API

# Apply
dotnet ef database update \
    --project Merge.Infrastructure \
    --startup-project Merge.API
```

Naming: Add{Table}, Add{Column}To{Table}, Remove{Column}From{Table}

Ask for: Migration name, then execute commands
