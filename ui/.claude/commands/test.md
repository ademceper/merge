---
description: Run tests and fix any failures
---

Execute the test suite and fix any failures:

1. **Run Tests**
   ```bash
   pnpm test
   ```

2. **If tests fail:**
   - Read the error messages carefully
   - Identify the root cause
   - Fix the issue in the source code
   - Re-run tests to verify

3. **Test Coverage Check**
   ```bash
   pnpm test:coverage
   ```

4. **For new code without tests:**
   - Create test file in `__tests__/` directory
   - Use Vitest + React Testing Library
   - Follow this pattern:
   ```tsx
   import { describe, it, expect } from 'vitest'
   import { render, screen } from '@testing-library/react'
   import userEvent from '@testing-library/user-event'
   import { Component } from '../component'

   describe('Component', () => {
     it('renders correctly', () => {
       render(<Component>Test</Component>)
       expect(screen.getByRole('button')).toBeInTheDocument()
     })
   })
   ```

5. **E2E Tests (if needed):**
   - Web: `pnpm test:e2e` (Playwright)
   - Mobile: Maestro flows in `apps/mobile/.maestro/`
