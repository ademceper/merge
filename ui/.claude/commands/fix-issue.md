---
description: Analyze and fix a GitHub issue
---

Fix GitHub issue: $ARGUMENTS

**Workflow:**

1. **Understand the Issue**
   - Read the issue description and comments
   - Identify affected files and components
   - Understand expected vs actual behavior

2. **Reproduce the Problem**
   - Find relevant code
   - Understand the root cause
   - Check for related issues

3. **Implement the Fix**
   - Make minimal, focused changes
   - Follow project conventions
   - Don't introduce new issues

4. **Verify the Fix**
   - Run relevant tests: `pnpm test`
   - Run type check: `pnpm lint`
   - Test manually if needed

5. **Create Commit**
   - Use conventional commit format
   - Reference the issue: `fix: description (#ISSUE_NUMBER)`

6. **Document if Needed**
   - Update CLAUDE.md if behavior changes
   - Add comments for complex fixes
