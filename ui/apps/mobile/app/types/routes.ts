/**
 * Type-safe route definitions for Expo Router
 * This file ensures type safety when navigating between routes
 */

import type { Href } from 'expo-router';

/**
 * Available routes in the mobile app
 */
export type AppRoute =
  | '/onboarding'
  | '/(tabs)/home'
  | '/(tabs)/search'
  | '/(tabs)/favorites'
  | '/(tabs)/cart'
  | '/(tabs)/profile'
  | '/login'
  | '/register'
  | '/forgot-password'
  | '/';

/**
 * Type-safe route href helper
 * Use this instead of string literals to ensure type safety
 */
export const Routes = {
  onboarding: '/onboarding' as const,
  home: '/(tabs)/home' as const,
  search: '/(tabs)/search' as const,
  favorites: '/(tabs)/favorites' as const,
  cart: '/(tabs)/cart' as const,
  profile: '/(tabs)/profile' as const,
  login: '/login' as const,
  register: '/register' as const,
  forgotPassword: '/forgot-password' as const,
  index: '/' as const,
} as const;

/**
 * Type-safe href for router methods
 */
export type RouteHref = Href<AppRoute>;
