# Merge UI Monorepo

A Turborepo monorepo setup with React Native and Next.js applications sharing UI components.

## What's inside?

This Turborepo includes the following packages/apps:

### Apps

- `apps/mobile`: a [React Native](https://reactnative.dev/) app built with [Expo](https://docs.expo.dev/) and [Expo Router](https://docs.expo.dev/router/introduction/)
- `apps/web`: a [Next.js](https://nextjs.org/) app with [shadcn/ui](https://ui.shadcn.com/) components

### Packages

- `packages/uim`: React Native component library for mobile applications
  - Components are imported directly from source (no build step)
  - Tree-shaking enabled - only used components are bundled
  - Usage: `import { Button } from "@merge/uim/components/button"`
  
- `packages/ui`: Next.js/Web component library with shadcn/ui
  - Usage: `import { Button } from "@merge/ui/components/button"`

- `packages/ts-cfg`: Shared TypeScript configurations
- `packages/lint-cfg`: Shared ESLint configurations

Each package/app is 100% [TypeScript](https://www.typescriptlang.org/).

## Getting Started

### Install dependencies

```sh
pnpm install
```

### Run development servers

```sh
# Run all apps in development mode
pnpm dev

# Run specific app
cd apps/mobile && pnpm dev
cd apps/web && pnpm dev
```

### Build

```sh
# Build all apps and packages
pnpm build
```

## Tech Stack

- [Turborepo](https://turbo.build/repo) - Monorepo build system
- [Expo](https://docs.expo.dev/) - React Native framework
- [Expo Router](https://docs.expo.dev/router/introduction/) - File-based routing for Expo
- [Next.js](https://nextjs.org/) - React framework for web
- [TypeScript](https://www.typescriptlang.org/) - Static type checking
- [pnpm](https://pnpm.io/) - Fast, disk space efficient package manager
