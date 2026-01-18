---
description: Create a new component following project conventions
---

Create a new component named: $ARGUMENTS

Follow these patterns:

**For Web (packages/ui):**
```tsx
import * as React from 'react'
import { cn } from '@merge/ui/lib/utils'
import { cva, type VariantProps } from 'class-variance-authority'

export const componentVariants = cva('base-classes', {
  variants: {
    variant: { default: '', secondary: '' },
    size: { sm: '', md: '', lg: '' },
  },
  defaultVariants: { variant: 'default', size: 'md' },
})

interface ComponentProps
  extends React.ComponentProps<'div'>,
    VariantProps<typeof componentVariants> {}

export function Component({ className, variant, size, ...props }: ComponentProps) {
  return (
    <div className={cn(componentVariants({ variant, size }), className)} {...props} />
  )
}
```

**For Mobile (packages/uim):**
- Use NativeWind classes
- Import from @merge/uim/components/text for typography
- Use rn-primitives for accessibility

**Steps:**
1. Create the component file
2. Add to package exports (package.json)
3. Create test file in `__tests__/`
4. Add to component index if exists
