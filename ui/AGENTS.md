# Merge E-commerce Monorepo

## Quick Reference

### Structure
- `apps/web/` - Next.js 16 + React 19
- `apps/mobile/` - Expo 54 + React Native
- `packages/ui/` - 54 Web components (shadcn/ui)
- `packages/uim/` - 32 Mobile components (rn-primitives)
- `packages/api/` - Shared API client
- `packages/store/` - Zustand stores
- `packages/validators/` - Zod schemas
- `packages/i18n/` - Translations

### Commands
```bash
pnpm dev          # Start all
pnpm build        # Build all
pnpm lint         # ESLint
pnpm test         # Vitest
```

### Key Patterns
- Components: `export function Name() { ... }` (not const)
- Styling: Tailwind + cn() utility
- Icons: Phosphor Icons only
- State: Zustand (client) + TanStack Query (server)
- API: BFF pattern with Server Actions

### Don't
- Use `any` type
- Use `export const` for components
- Use forwardRef (React 19 auto-forwards)
- Hardcode user-facing strings (use i18n)
