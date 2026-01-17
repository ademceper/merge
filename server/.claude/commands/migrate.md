---
description: Create and apply EF Core database migration
allowed-tools:
  - Bash(dotnet ef *)
  - Read
---

# Database Migration

Create and apply Entity Framework Core migrations.

## Commands

### Create Migration
```bash
dotnet ef migrations add {MigrationName} \
    --project Merge.Infrastructure \
    --startup-project Merge.API
```

### Apply Migrations
```bash
dotnet ef database update \
    --project Merge.Infrastructure \
    --startup-project Merge.API
```

### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove \
    --project Merge.Infrastructure \
    --startup-project Merge.API
```

### Generate SQL Script
```bash
dotnet ef migrations script \
    --project Merge.Infrastructure \
    --startup-project Merge.API \
    --idempotent \
    -o migration.sql
```

### List Migrations
```bash
dotnet ef migrations list \
    --project Merge.Infrastructure \
    --startup-project Merge.API
```

## Migration Naming Convention

| Change Type | Format | Example |
|-------------|--------|---------|
| Add Table | Add{TableName} | AddProducts |
| Add Column | Add{Column}To{Table} | AddRatingToProducts |
| Remove Column | Remove{Column}From{Table} | RemoveLegacyFieldFromOrders |
| Add Index | AddIndex{Columns}To{Table} | AddIndexSkuToProducts |
| Add FK | Add{Table1}{Table2}Relation | AddProductCategoryRelation |
| Rename | Rename{Old}To{New} | RenameSkuToProductCode |

## Steps

1. Ask for migration name
2. Validate naming convention
3. Run `dotnet ef migrations add`
4. Show created migration file
5. Ask if should apply migration
6. If yes, run `dotnet ef database update`

## Safety Checks

Before applying:
- [ ] Backup database (production)
- [ ] Review migration file
- [ ] Check for data loss operations
- [ ] Test on development first

## Rollback

To rollback to previous migration:
```bash
dotnet ef database update {PreviousMigrationName} \
    --project Merge.Infrastructure \
    --startup-project Merge.API
```
