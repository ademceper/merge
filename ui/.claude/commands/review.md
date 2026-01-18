---
description: Comprehensive code review for recent changes
---

Perform a thorough code review of the recent changes:

1. **TypeScript & Type Safety**
   - Check for `any` types or unsafe assertions
   - Verify proper interface/type definitions
   - Ensure strict mode compliance

2. **Component Architecture**
   - Verify `export function` pattern (not const)
   - Check React 19 patterns (no unnecessary forwardRef)
   - Ensure proper use of CVA for variants
   - Verify data-slot attributes where needed

3. **Performance**
   - Check for missing React.memo on heavy components
   - Verify proper dependency arrays in hooks
   - Look for unnecessary re-renders
   - Check image optimization (next/image for web)

4. **Security (OWASP Top 10)**
   - No dangerouslySetInnerHTML with user input
   - Proper input validation with Zod
   - No exposed API keys or secrets

5. **Accessibility**
   - aria-labels on icon buttons
   - Proper heading hierarchy
   - Focus management

6. **Testing Considerations**
   - What tests should be added?
   - Edge cases to consider?

Provide specific file:line references for all findings.
