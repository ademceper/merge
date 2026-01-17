---
title: Create CQRS Command
description: Scaffolds command with handler and validator
---

Create a complete CQRS command:

**Files to create:**
```
Merge.Application/{Module}/Commands/{Name}/
├── {Name}Command.cs
├── {Name}CommandHandler.cs
└── {Name}CommandValidator.cs
```

**Template:**
- Command: `public record {Name}Command(...) : IRequest<{Dto}>`
- Handler: Primary constructor with IRepository, IUnitOfWork, IMapper
- Validator: FluentValidation rules

Ask for: Module, Command name, Properties, Return type
