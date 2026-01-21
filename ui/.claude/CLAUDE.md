# Merge - E-commerce Monorepo

## Project Overview
Cross-platform e-commerce application with shared component libraries.
- **Web**: Next.js 16 + React 19 + Turbopack
- **Mobile**: Expo 54 + React Native 0.81.5 + Expo Router 6

## Monorepo Structure
```
apps/web/          → Next.js App Router (Turbopack)
apps/mobile/       → Expo with Server Actions (BFF pattern)
packages/ui/       → 54 Web components (shadcn/ui + Radix + Phosphor Icons)
packages/uim/      → 32 Mobile components (rn-primitives + NativeWind)
packages/api/      → Shared API client + TanStack Query hooks
packages/store/    → Zustand stores (cart, wishlist, auth)
packages/i18n/     → i18next translations (en.json, tr.json)
packages/validators/ → Zod schemas
```

## Commands
```bash
pnpm dev              # Start all apps
pnpm build            # Build all
pnpm lint             # ESLint check
pnpm test             # Vitest + RTL
pnpm test:e2e         # Playwright (web) / Maestro (mobile)
cd apps/mobile && npx expo start  # Mobile dev server
```

## Tech Decisions
- **State**: Zustand (client) + TanStack Query (server)
- **Forms**: React Hook Form + Zod
- **Auth**: Custom JWT (httpOnly cookie web, expo-secure-store mobile)
- **API**: REST with BFF pattern via Server Actions
- **Animation**: Framer Motion LazyMotion (web), Reanimated + Lottie (mobile)
- **Analytics**: Sentry + PostHog
- **Offline**: TanStack Query Persistence + MMKV

## Code Style
- Components: `export function ComponentName() { ... }`
- React 19: No forwardRef needed, refs auto-forwarded
- Styling: Tailwind utilities, use `cn()` for merging
- Icons: Phosphor Icons only (`@phosphor-icons/react`)

## Testing Strategy
- Unit: `packages/*/__tests__/` with Vitest
- Component: React Testing Library
- E2E Web: Playwright
- E2E Mobile: Maestro

## Important Files
- `packages/ui/src/components/` → Web component library (production-ready)
- `packages/uim/src/components/` → Mobile component library (production-ready)
- `apps/web/actions/` → Next.js Server Actions
- `apps/mobile/server/` → Expo Server Functions

## Git Convention
Conventional Commits: `feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:`
Branches: main (prod) → staging → develop → feature/*

---

# Merge Design System

## Design DNA (Both Platforms)
1. **Monochrome**: Black & white only, no colors except destructive red
2. **Minimalist**: Less is more, negative space is design
3. **Geometric**: Sharp, mathematical forms
4. **Flat Design**: No gradients, minimal shadows, no depth
5. **Typography as Hero**: Space Grotesk is the identity
6. **Native Feel**: Platform conventions respected

## Reference Designs
**ALWAYS** check these files before creating new screens:
```
/screens/mobile/                    → Mobile HTML mockups
/screens/web/                       → Web HTML mockups
/screens/MOBILE_STITCH_PROMPT_TEMPLATE.md  → Mobile design guide (detailed)
/screens/WEB_STITCH_PROMPT_TEMPLATE.md     → Web design guide (detailed)
```

## Color Palette

### Light Mode
```
background: #ffffff (pure white)
foreground: #111118 or #000000 (pure black)
muted:      foreground with opacity (70%, 60%, 30%)
border:     foreground/10
```

### Dark Mode
```
background: #101022 or #000000 (OLED optimized)
foreground: #ffffff (pure white)
muted:      white with opacity (70%, 60%, 30%)
border:     white/10
```

### Usage Patterns
```tsx
// Text hierarchy
className="text-primary dark:text-white"           // Primary
className="text-primary/70 dark:text-white/70"     // Secondary/muted
className="text-primary/60 dark:text-white/60"     // Subtle

// Backgrounds
className="bg-white dark:bg-background-dark"
className="bg-primary dark:bg-white"               // Inverted buttons

// Borders
className="border-primary/10 dark:border-white/10"
```

## Typography

### Font Family
- **Web**: `font-display` class, loaded via next/font (Space Grotesk)
- **Mobile**: `@expo-google-fonts/space-grotesk`
- **Weights**: 300 (Light), 400 (Regular), 500 (Medium), 700 (Bold)

### Hierarchy (Both Platforms)
```
H1:      text-3xl/text-4xl font-bold uppercase tracking-tight leading-tight
H2:      text-2xl/text-3xl font-bold tracking-tight
H3:      text-xl font-bold tracking-tight
Body:    text-base font-normal leading-relaxed
Small:   text-sm font-medium
Caption: text-xs
```

## Button Patterns

### Primary CTA
```tsx
// Light: black bg, white text | Dark: white bg, black text
// Web
className="w-full h-14 bg-[#111118] dark:bg-white text-white dark:text-[#111118] rounded-xl font-bold tracking-wider uppercase"

// Mobile (NativeWind)
className="w-full h-14 bg-primary dark:bg-white rounded-xl items-center justify-center"
// Text: className="text-white dark:text-primary font-bold tracking-wider uppercase"
```

### Secondary/Ghost Button
```tsx
className="text-primary/60 dark:text-white/60 hover:text-primary dark:hover:text-white text-sm font-medium"
```

### Icon Button
```tsx
className="flex items-center justify-center rounded-full w-10 h-10 bg-transparent hover:bg-black/5 dark:hover:bg-white/10"
```

## Layout Patterns

### Screen Container (Mobile)
```tsx
<View className="flex-1 flex-col bg-background dark:bg-background-dark">
  {/* Safe area handled by useSafeAreaInsets() */}
</View>
```

### Page Container (Web)
```tsx
<main className="min-h-screen bg-white dark:bg-background-dark">
  <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
    {/* Content */}
  </div>
</main>
```

### Centered Content
```tsx
<View className="flex flex-col items-center justify-center px-6 flex-1">
  {/* Centered content */}
</View>
```

### Sticky Bottom Actions
```tsx
<View className="w-full px-6 pb-8 pt-4 space-y-4">
  <Button>Primary Action</Button>
  <Button variant="ghost">Secondary</Button>
</View>
```

## Hero Icon Pattern
```tsx
// Large icon with subtle border
<View className="flex items-center justify-center p-6 rounded-full border border-primary/10 dark:border-white/10 bg-white dark:bg-white/5">
  <Icon size={64} />
</View>

// Geometric variant with corner accents
<View className="relative flex h-32 w-32 items-center justify-center rounded-2xl border-[1.5px] border-primary dark:border-white">
  <Icon size={64} />
  <View className="absolute -right-2 -top-2 h-4 w-4 border-r-[1.5px] border-t-[1.5px]" />
</View>
```

## Feature List Pattern
```tsx
<View className="w-full max-w-xs space-y-4">
  <View className="flex items-center gap-4">
    <Icon size={24} />
    <Text className="text-base font-medium">Feature description</Text>
  </View>
</View>
```

## Page Indicators
```tsx
<View className="flex flex-row items-center justify-center gap-3">
  {pages.map((_, idx) => (
    <View
      key={idx}
      className={cn(
        "h-2.5 w-2.5 rounded-full",
        idx === currentPage
          ? "bg-[#111118] dark:bg-white"
          : "border border-[#111118]/30 dark:border-white/30"
      )}
    />
  ))}
</View>
```

## Responsive Breakpoints (Web)
```
Mobile:  < 768px   → Single column, bottom nav, full-width CTAs
Tablet:  768-1024px → 2 columns, collapsible sidebar
Desktop: > 1024px  → Multi-column, persistent sidebar, hover states
```

## Spacing System
- **Base unit**: 8px
- **Padding**: p-4 (16px), p-6 (24px), p-8 (32px)
- **Gap**: gap-4 (16px), gap-6 (24px)
- **Touch targets**: min 44×44 (iOS), 48×48 (Android)

## Animation Guidelines

### Web (Framer Motion)
```tsx
// Subtle, quick transitions
<motion.div
  initial={{ opacity: 0, y: 8 }}
  animate={{ opacity: 1, y: 0 }}
  transition={{ duration: 0.2, ease: "easeOut" }}
/>
```

### Mobile (Reanimated)
```tsx
// Use spring physics for native feel
const animatedStyle = useAnimatedStyle(() => ({
  transform: [{ scale: withSpring(pressed.value ? 0.96 : 1) }],
}));
```

## Card Pattern
```tsx
// Web
<div className="rounded-xl border border-primary/10 dark:border-white/10 bg-white dark:bg-white/5 p-4">

// Mobile
<View className="rounded-xl border border-primary/10 dark:border-white/10 bg-white dark:bg-white/5 p-4">
```

## Input Pattern
```tsx
// Both platforms
className="h-12 w-full rounded-lg border border-primary/10 dark:border-white/10 bg-transparent px-4 text-base placeholder:text-primary/40 dark:placeholder:text-white/40 focus:border-primary dark:focus:border-white"
```

## Design DO's
- ✓ Use monochrome palette only
- ✓ Use Space Grotesk font everywhere
- ✓ Use uppercase for headings and CTAs
- ✓ Use generous negative space
- ✓ Use subtle opacity (10%, 30%, 60%, 70%) for hierarchy
- ✓ Check /screens/ directory before creating new screens
- ✓ Follow platform conventions (iOS/Android/Web)

## Design DON'Ts
- ✗ No gradients
- ✗ No colored accents (except destructive red for errors)
- ✗ No heavy shadows (use border or subtle bg instead)
- ✗ No rounded-full on rectangular buttons (use rounded-lg/xl)
- ✗ No decorative elements
- ✗ No depth/3D effects

---

# UIM Package Usage (CRITICAL - MANDATORY)

## ⚠️ HIGHEST PRIORITY RULE: ALWAYS use @merge/uim components and color tokens

**This rule has the HIGHEST PRIORITY. NEVER use native React Native elements or hardcoded colors when @merge/uim alternatives exist.**

## Mobile (@merge/uim) - MANDATORY Usage

### Components (ALWAYS use these)
```tsx
// ❌ FORBIDDEN - Native React Native
import { Text, Pressable, TextInput } from "react-native"
<Text>Hello</Text>
<Pressable onPress={handlePress}><Text>Click</Text></Pressable>
<TextInput placeholder="Email" />

// ✅ REQUIRED - @merge/uim components
import { Text } from "@merge/uim/components/text"
import { Button } from "@merge/uim/components/button"
import { Input } from "@merge/uim/components/input"
import { Card } from "@merge/uim/components/card"
import { Badge } from "@merge/uim/components/badge"
import { Icon } from "@merge/uim/components/icon"
```

### UIM Color Tokens (ALWAYS use these via NativeWind className)
```tsx
// ❌ FORBIDDEN - Hardcoded colors
style={{ backgroundColor: isDark ? "#ffffff" : "#111118" }}
style={{ color: "#666666" }}
className="bg-[#111118]"
className="text-[#ffffff]"

// ✅ REQUIRED - UIM color tokens
className="bg-primary"              // Primary background
className="bg-background"           // Main background
className="bg-secondary"            // Secondary background
className="bg-muted"                // Muted background
className="bg-card"                 // Card background
className="bg-destructive"          // Error/danger background

className="text-foreground"         // Primary text
className="text-primary-foreground" // Text on primary bg
className="text-muted-foreground"   // Muted/secondary text
className="text-destructive"        // Error text

className="border-border"           // Standard border
className="border-input"            // Input border
className="border-primary"          // Primary border

// With opacity
className="bg-primary/10"           // 10% opacity
className="text-foreground/80"      // 80% opacity
```

### ONLY Exception for Hex Values
Only use hex values when a library component (like SVG) REQUIRES them:
```tsx
// SVG components that require hex values
import Svg, { Circle } from "react-native-svg"

// Define colors based on design system
const colors = {
  destructive: "#ef4444",
  primary: isDark ? "#ffffff" : "#111118",
  border: isDark ? "rgba(255, 255, 255, 0.2)" : "#e5e5e5",
};

<Circle stroke={colors.primary} /> // Only when SVG requires it
```

## Web (@merge/ui) - MANDATORY Usage
```tsx
// ❌ FORBIDDEN - Native HTML
<button onClick={handleClick}>Click</button>
<input type="email" />

// ✅ REQUIRED - @merge/ui components
import { Button } from "@merge/ui/components/button"
import { Input } from "@merge/ui/components/input"
import { Card } from "@merge/ui/components/card"
```

## Pre-commit Checklist (MANDATORY)
Before EVERY commit, verify:
- [ ] ✅ ALL Text → `@merge/uim/components/text`
- [ ] ✅ ALL Buttons → `@merge/uim/components/button`
- [ ] ✅ ALL Inputs → `@merge/uim/components/input`
- [ ] ✅ ALL Cards → `@merge/uim/components/card`
- [ ] ✅ ALL Badges → `@merge/uim/components/badge`
- [ ] ✅ ALL Icons → `@merge/uim/components/icon`
- [ ] ✅ ALL colors → UIM tokens (`bg-primary`, `text-foreground`, etc.)
- [ ] ❌ NO native React Native Text, Pressable, TextInput
- [ ] ❌ NO hardcoded hex colors
- [ ] ❌ NO inline style colors when className works
