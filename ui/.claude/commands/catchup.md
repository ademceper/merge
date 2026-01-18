---
description: Load uncommitted changes into context after /clear
---

Reload the current work-in-progress into context:

1. **Check Git Status**
   ```bash
   git status
   git diff --stat
   ```

2. **Read Modified Files**
   - Read all modified and staged files
   - Understand what changes have been made
   - Note any unfinished work

3. **Check Recent Commits**
   ```bash
   git log --oneline -5
   ```

4. **Identify Current Task**
   - What was being worked on?
   - What remains to be done?
   - Any blockers or issues?

5. **Summary**
   Provide a brief summary of:
   - Files changed
   - Current state of work
   - Next steps to complete the task
