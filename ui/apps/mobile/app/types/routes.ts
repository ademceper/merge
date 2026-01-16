/**
 * Type-safe route definitions for Expo Router
 * This file ensures type safety when navigating between routes
 */

import type { Href } from 'expo-router';

/**
 * Available routes in the mobile app
 */
export type AppRoute = '/onboarding' | '/home' | '/';

/**
 * Type-safe route href helper
 * Use this instead of string literals to ensure type safety
 */
export const Routes = {
  onboarding: '/onboarding' as const,
  home: '/home' as const,
  index: '/' as const,
} as const;

/**
 * Type-safe href for router methods
 */
export type RouteHref = Href<AppRoute>;
