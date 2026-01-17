---
title: Code Review
description: Performs comprehensive code review
---

Review staged changes for:

1. **Correctness**: Logic errors, null checks, edge cases
2. **Architecture**: Clean Architecture rules, CQRS patterns
3. **Performance**: AsNoTracking, AsSplitQuery, N+1 queries
4. **Security**: No secrets, no PII in logs, authorization
5. **Code Style**: C# 12 features, naming conventions

Output format:
- Critical: Must fix before merge
- High: Should fix
- Medium: Consider fixing
- Suggestions: Optional improvements
