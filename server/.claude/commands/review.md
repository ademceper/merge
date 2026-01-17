---
description: Comprehensive code review following project standards
allowed-tools:
  - Read
  - Bash(git diff)
---

# Code Review

Perform comprehensive code review on staged/modified files.

## Review Categories

### 1. Correctness
- [ ] Logic errors
- [ ] Edge cases handled
- [ ] Null checks present
- [ ] Exception handling appropriate
- [ ] CancellationToken propagated

### 2. Architecture (Clean Architecture)
- [ ] Domain has no external dependencies
- [ ] Application depends only on Domain
- [ ] Infrastructure implements interfaces
- [ ] No circular dependencies

### 3. CQRS Patterns
- [ ] Commands change state, queries read only
- [ ] Factory methods used for entity creation
- [ ] Domain events raised appropriately
- [ ] Validators present for commands

### 4. Performance
- [ ] AsNoTracking() for read queries
- [ ] AsSplitQuery() for multiple includes
- [ ] No N+1 queries
- [ ] Pagination for lists
- [ ] Cache invalidation after writes

### 5. Security
- [ ] No hardcoded secrets
- [ ] No sensitive data in logs
- [ ] Input validation present
- [ ] Authorization on endpoints
- [ ] IDOR protection

### 6. Code Style
- [ ] Primary constructors used (C# 12)
- [ ] Records for DTOs
- [ ] Structured logging
- [ ] Async/await properly used
- [ ] Naming conventions followed

### 7. Testing
- [ ] Unit tests for new code
- [ ] Edge cases covered
- [ ] Mocks properly configured

## Review Process

1. Get changed files: `git diff --name-only HEAD`
2. Read each changed file
3. Check against each category
4. Provide actionable feedback

## Output Format

```markdown
## Code Review Summary

### Overall: ✅ APPROVED / ⚠️ NEEDS CHANGES / ❌ REJECTED

### Issues Found

#### 🔴 Critical
- [File:Line] Description of critical issue

#### 🟠 High
- [File:Line] Description of high-priority issue

#### 🟡 Medium
- [File:Line] Description of medium-priority issue

#### 🟢 Suggestions
- [File:Line] Optional improvement suggestion

### Positive Aspects
- Good use of X
- Well-structured Y

### Recommended Actions
1. Fix critical issues
2. Address high-priority issues
3. Consider suggestions
```
