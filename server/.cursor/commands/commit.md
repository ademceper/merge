---
title: Generate Commit
description: Creates conventional commit message from git changes
---

Generate a meaningful conventional commit message based on current changes:

1. Analyze `git diff HEAD --stat` to understand what changed
2. Use conventional commits format:
   - Types: feat, fix, docs, style, refactor, perf, test, chore
   - Scopes: api, app, domain, infra, test
   - Format: `type(scope): description`
3. Subject: Max 50 chars, imperative mood
4. Stage all changes and create commit
