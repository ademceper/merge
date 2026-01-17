---
title: Fix Architecture Violations
description: Detects and fixes Clean Architecture rule violations
---

Analyze and fix Clean Architecture violations:

## Layer Dependency Rules

```
API → Application → Domain ← Infrastructure
        ↑                        ↑
        └────────────────────────┘
```

## Violation Detection

### Domain Layer Violations
```csharp
// FORBIDDEN in Merge.Domain
using Microsoft.EntityFrameworkCore;  // ❌ Infrastructure
using FluentValidation;               // ❌ Application
using AutoMapper;                     // ❌ Application
using Merge.Application.*;            // ❌ Application
using Merge.Infrastructure.*;         // ❌ Infrastructure
```

### Application Layer Violations
```csharp
// FORBIDDEN in Merge.Application
using Microsoft.AspNetCore.Mvc;       // ❌ API
using Merge.API.*;                    // ❌ API
using Merge.Infrastructure.Data.*;    // ❌ Direct Infrastructure
```

### Infrastructure Returning Domain
```csharp
// Repository should return domain entities, not DTOs
public async Task<ProductDto> GetByIdAsync(Guid id)  // ❌
public async Task<Product?> GetByIdAsync(Guid id)   // ✅
```

## Fix Patterns

### 1. Move to correct layer
```bash
# File in wrong layer
mv Merge.Domain/Services/EmailService.cs Merge.Infrastructure/Services/
```

### 2. Create interface
```csharp
// Application layer - interface
public interface IEmailService { }

// Infrastructure layer - implementation
public class EmailService : IEmailService { }
```

### 3. Use MediatR for cross-cutting
```csharp
// Instead of direct service call
await _emailService.SendAsync(email);

// Use domain event
AddDomainEvent(new OrderCreatedEvent(Id));
// Handler in Infrastructure sends email
```

## Commands

```bash
# Find EF Core in Domain
grep -r "EntityFrameworkCore" Merge.Domain/

# Find API in Application
grep -r "Microsoft.AspNetCore" Merge.Application/

# Find Infrastructure imports
grep -r "Merge.Infrastructure" Merge.Domain/ Merge.Application/
```

Analyze current file and suggest fixes following Clean Architecture rules.
