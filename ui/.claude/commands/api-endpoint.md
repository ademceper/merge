---
description: Create API integration with TanStack Query
---

Create API integration for: $ARGUMENTS

**Structure:**
```
packages/api/
├── client.ts         # Base fetch wrapper
├── hooks/
│   └── use-[name].ts # TanStack Query hooks
└── types/
    └── [name].ts     # Zod schemas + types
```

**1. Define Zod Schema (packages/validators/):**
```tsx
import { z } from 'zod'

export const productSchema = z.object({
  id: z.string(),
  name: z.string(),
  price: z.number(),
})

export type Product = z.infer<typeof productSchema>
```

**2. Create API Function (packages/api/):**
```tsx
import { apiClient } from './client'
import { productSchema } from '@merge/validators'

export async function getProducts() {
  const res = await apiClient.get('/products')
  return productSchema.array().parse(res.data)
}
```

**3. Create Query Hook:**
```tsx
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../products'

export function useProducts() {
  return useQuery({
    queryKey: ['products'],
    queryFn: getProducts,
    staleTime: 2 * 60 * 1000, // 2 minutes
  })
}
```

**4. Create Server Action (BFF):**
```tsx
'use server'
import { getProducts } from '@merge/api'

export async function fetchProducts() {
  return getProducts()
}
```
