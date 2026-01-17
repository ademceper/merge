---
description: Generate conventional commit message from git diff
allowed-tools:
  - Bash(git status)
  - Bash(git diff)
  - Bash(git add)
  - Bash(git commit)
---

# Smart Commit

Generate a meaningful conventional commit message based on current changes.

## Steps

1. **Get git status**: `git status`
2. **Get diff**: `git diff HEAD --stat` and `git diff HEAD` for details
3. **Analyze changes** - Understand what was modified
4. **Generate commit message** following Conventional Commits:

## Conventional Commit Format

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Types
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation only
- **style**: Formatting, missing semicolons, etc.
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **perf**: Performance improvement
- **test**: Adding tests
- **chore**: Maintenance tasks
- **ci**: CI/CD changes
- **build**: Build system changes

### Scope (from this project)
- **api**: Controller, middleware changes
- **app**: Application layer (commands, queries, services)
- **domain**: Domain entities, value objects, events
- **infra**: Infrastructure (DbContext, repositories)
- **test**: Test files

### Examples
```
feat(api): add PATCH endpoint for products

fix(domain): handle negative stock quantity edge case

refactor(app): split large mapping profile into modules

test(domain): add unit tests for Order aggregate

docs(readme): update development setup instructions
```

## Guidelines

- **Subject line**: Max 50 characters, imperative mood ("add" not "added")
- **Body**: Wrap at 72 characters, explain what and why (not how)
- **Footer**: Reference issues (Fixes #123, Closes #456)

## Actions

After generating the message:
1. Stage all changes: `git add -A`
2. Create commit with the generated message
3. Show the commit result
