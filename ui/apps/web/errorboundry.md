# 🛡️ Next.js Error Handling Best Practices - Kapsamlı Rehber

## İçindekiler
1. [Next.js Error Handling Temelleri](#1-nextjs-error-handling-temelleri)
2. [App Router Dosya Yapısı](#2-app-router-dosya-yapısı)
3. [error.tsx vs global-error.tsx](#3-errortsx-vs-global-errortsx)
4. [not-found.tsx Kullanımı](#4-not-foundtsx-kullanımı)
5. [Server Actions Error Handling](#5-server-actions-error-handling)
6. [Client Components Error Handling](#6-client-components-error-handling)
7. [Server Components Error Handling](#7-server-components-error-handling)
8. [Sentry Entegrasyonu](#8-sentry-entegrasyonu)
9. [Anti-Patterns ve Yaygın Hatalar](#9-anti-patterns-ve-yaygın-hatalar)
10. [Özet Checklist](#10-özet-checklist)

---

## 1. Next.js Error Handling Temelleri

### Error Handling Türleri

| Hata Türü | Açıklama | Çözüm |
|-----------|----------|-------|
| **Expected Errors** | Beklenen hatalar (validation, auth, 404) | Return value olarak dön |
| **Uncaught Exceptions** | Beklenmeyen runtime hataları | error.tsx ile yakala |
| **Server Errors** | Server Component/Action hataları | try-catch + error.tsx |
| **Client Errors** | Client Component render hataları | error.tsx (Client Component) |

### Error Boundary Sınırlamaları

Next.js'deki error boundary'ler şunları **YAKALAYAMAZ**:

```
❌ Event handlers (onClick, onChange)
❌ Async code (setTimeout, Promise, fetch)
❌ Server-side rendering (initial render)
❌ Aynı segment'teki layout.tsx hataları
❌ Error boundary'nin kendisindeki hatalar
```

### Hata Akış Diyagramı

```
Component Hatası
      │
      ▼
┌─ error.tsx (aynı segment) ─┐
│  Varsa → Yakala            │
│  Yoksa → Yukarı çık        │
└────────────────────────────┘
      │
      ▼
┌─ Parent error.tsx ─┐
│  Varsa → Yakala    │
│  Yoksa → Yukarı    │
└────────────────────┘
      │
      ▼
┌─ Root error.tsx ─┐
│  Varsa → Yakala  │
│  Yoksa → global  │
└──────────────────┘
      │
      ▼
┌─ global-error.tsx ─┐
│  Root layout       │
│  hatalarını yakalar│
└────────────────────┘
```

---

## 2. App Router Dosya Yapısı

### Önerilen Klasör Yapısı

```
app/
├── layout.tsx                 # Root layout
├── error.tsx                  # 🛡️ Root error boundary
├── global-error.tsx           # 🌍 Global error (root layout için)
├── not-found.tsx              # 404 sayfası
├── loading.tsx                # Loading UI
│
├── dashboard/
│   ├── layout.tsx
│   ├── page.tsx
│   ├── error.tsx              # 🛡️ Dashboard error boundary
│   ├── loading.tsx
│   │
│   └── invoices/
│       ├── page.tsx
│       ├── error.tsx          # 🛡️ Invoices error boundary
│       ├── not-found.tsx      # Invoice bulunamadı
│       │
│       └── [id]/
│           ├── page.tsx
│           ├── error.tsx      # 🛡️ Invoice detail error
│           └── not-found.tsx
│
├── api/
│   └── [...]/route.ts         # API Routes
│
└── actions/
    └── invoices.ts            # Server Actions
```

### Dosya Öncelik Sırası

```
1. not-found.tsx    → 404 hataları (en spesifik)
2. error.tsx        → Runtime hataları
3. global-error.tsx → Root layout hataları (en genel)
```

---

## 3. error.tsx vs global-error.tsx

### Karşılaştırma Tablosu

| Özellik | error.tsx | global-error.tsx |
|---------|-----------|------------------|
| **Konum** | Herhangi bir segment | Sadece app/ root |
| **Yakaladığı** | Segment ve alt segment hataları | Root layout/template hataları |
| **Layout** | Parent layout korunur | Layout YERİNE geçer |
| **HTML/Body** | Gerekli değil | `<html>` ve `<body>` GEREKLİ |
| **Kullanım sıklığı** | Sık | Nadir |
| **Client Component** | ✅ Evet | ✅ Evet |

### error.tsx Örneği

```tsx
// app/dashboard/error.tsx
'use client' // ⚠️ ZORUNLU - Error boundary'ler Client Component olmalı

import { useEffect } from 'react'

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  useEffect(() => {
    // Hata logla (Sentry, vs.)
    console.error('Dashboard Error:', error)
  }, [error])

  return (
    <div className="flex flex-col items-center justify-center min-h-[400px] p-8">
      <div className="text-6xl mb-4">📊</div>
      <h2 className="text-2xl font-bold mb-2">Dashboard Yüklenemedi</h2>
      <p className="text-gray-600 mb-4">
        {error.message || 'Bir hata oluştu'}
      </p>
      
      {/* Development'ta digest göster */}
      {process.env.NODE_ENV === 'development' && error.digest && (
        <p className="text-xs text-gray-400 mb-4">Digest: {error.digest}</p>
      )}
      
      <div className="flex gap-4">
        <button
          onClick={() => reset()}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          Tekrar Dene
        </button>
        <a
          href="/"
          className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
        >
          Ana Sayfaya Dön
        </a>
      </div>
    </div>
  )
}
```

### global-error.tsx Örneği

```tsx
// app/global-error.tsx
'use client'

import { useEffect } from 'react'
import * as Sentry from '@sentry/nextjs'

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  useEffect(() => {
    // Production'da Sentry'ye gönder
    Sentry.captureException(error)
  }, [error])

  return (
    // ⚠️ ZORUNLU: html ve body tag'leri
    <html>
      <body>
        <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100">
          <div className="text-8xl mb-6">💥</div>
          <h1 className="text-3xl font-bold mb-4">Kritik Hata</h1>
          <p className="text-gray-600 mb-6">
            Beklenmeyen bir hata oluştu. Lütfen sayfayı yenileyin.
          </p>
          <button
            onClick={() => reset()}
            className="px-6 py-3 bg-red-500 text-white rounded-lg hover:bg-red-600"
          >
            Uygulamayı Yenile
          </button>
        </div>
      </body>
    </html>
  )
}
```

### ⚠️ Önemli Kural: Layout Hataları

```
error.tsx aynı segment'teki layout.tsx hatalarını YAKALAMAZ!
```

```
app/
├── dashboard/
│   ├── layout.tsx    ← Bu hata verirse...
│   ├── error.tsx     ← Bu YAKALAMAZ! ❌
│   └── page.tsx

Çözüm: error.tsx'i parent segment'e taşı veya global-error.tsx kullan
```

---

## 4. not-found.tsx Kullanımı

### Global not-found.tsx

```tsx
// app/not-found.tsx
import Link from 'next/link'

export default function NotFound() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen">
      <h1 className="text-9xl font-bold text-gray-200">404</h1>
      <h2 className="text-2xl font-semibold mb-4">Sayfa Bulunamadı</h2>
      <p className="text-gray-600 mb-8">
        Aradığınız sayfa mevcut değil veya taşınmış olabilir.
      </p>
      <Link
        href="/"
        className="px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
      >
        Ana Sayfaya Dön
      </Link>
    </div>
  )
}
```

### Segment-specific not-found.tsx

```tsx
// app/products/[id]/not-found.tsx
import Link from 'next/link'
import { getPopularProducts } from '@/lib/products'

export default async function ProductNotFound() {
  // Server Component olabilir - data fetch edilebilir
  const popularProducts = await getPopularProducts()

  return (
    <div className="p-8">
      <h1 className="text-2xl font-bold mb-4">Ürün Bulunamadı</h1>
      <p className="text-gray-600 mb-8">
        Aradığınız ürün mevcut değil. Belki bunlar ilginizi çekebilir:
      </p>
      
      <div className="grid grid-cols-3 gap-4">
        {popularProducts.map(product => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
      
      <Link href="/products" className="mt-8 inline-block text-blue-500">
        ← Tüm Ürünlere Dön
      </Link>
    </div>
  )
}
```

### notFound() Fonksiyonu Kullanımı

```tsx
// app/products/[id]/page.tsx
import { notFound } from 'next/navigation'
import { getProduct } from '@/lib/products'

export default async function ProductPage({
  params,
}: {
  params: { id: string }
}) {
  const product = await getProduct(params.id)

  // Ürün yoksa 404
  if (!product) {
    notFound() // En yakın not-found.tsx'i render eder
  }

  return <ProductDetail product={product} />
}
```

### not-found vs error Önceliği

```
notFound() çağrıldığında → not-found.tsx render edilir
throw new Error() → error.tsx render edilir

⚠️ notFound(), error.tsx'ten ÖNCE gelir!
```

---

## 5. Server Actions Error Handling

### ❌ Yanlış Yaklaşım - Hata Fırlatmak

```tsx
// ❌ KÖTÜ - Production'da hata mesajı gizlenir
'use server'

export async function createInvoice(formData: FormData) {
  const amount = formData.get('amount')
  
  if (!amount) {
    throw new Error('Tutar gerekli') // Production'da "An error occurred" olur
  }
  
  await db.invoices.create({ amount })
}
```

### ✅ Doğru Yaklaşım - Return Value Pattern

```tsx
// ✅ İYİ - Hataları data olarak dön
'use server'

type ActionResult<T = void> = 
  | { success: true; data: T }
  | { success: false; error: string }

export async function createInvoice(
  formData: FormData
): Promise<ActionResult<{ id: string }>> {
  try {
    const amount = formData.get('amount') as string

    // Validation
    if (!amount) {
      return { success: false, error: 'Tutar gerekli' }
    }

    if (isNaN(Number(amount))) {
      return { success: false, error: 'Geçersiz tutar formatı' }
    }

    // Database işlemi
    const invoice = await db.invoices.create({ 
      amount: Number(amount) 
    })

    return { success: true, data: { id: invoice.id } }

  } catch (error) {
    console.error('Invoice creation failed:', error)
    return { success: false, error: 'Fatura oluşturulamadı' }
  }
}
```

### useActionState Hook ile Form Handling (React 19)

```tsx
// components/InvoiceForm.tsx
'use client'

import { useActionState } from 'react'
import { createInvoice } from '@/app/actions/invoices'

const initialState = {
  message: '',
  errors: {} as Record<string, string[]>,
}

export function InvoiceForm() {
  const [state, formAction, pending] = useActionState(
    createInvoice,
    initialState
  )

  return (
    <form action={formAction}>
      <div>
        <label htmlFor="amount">Tutar</label>
        <input
          type="number"
          id="amount"
          name="amount"
          aria-describedby="amount-error"
        />
        {state.errors?.amount && (
          <p id="amount-error" className="text-red-500">
            {state.errors.amount[0]}
          </p>
        )}
      </div>

      <div>
        <label htmlFor="description">Açıklama</label>
        <textarea id="description" name="description" />
        {state.errors?.description && (
          <p className="text-red-500">
            {state.errors.description[0]}
          </p>
        )}
      </div>

      {state.message && (
        <p className="text-red-500" aria-live="polite">
          {state.message}
        </p>
      )}

      <button type="submit" disabled={pending}>
        {pending ? 'Kaydediliyor...' : 'Kaydet'}
      </button>
    </form>
  )
}
```

### Server Action with Zod Validation

```tsx
// app/actions/invoices.ts
'use server'

import { z } from 'zod'
import { revalidatePath } from 'next/cache'
import { redirect } from 'next/navigation'

const InvoiceSchema = z.object({
  amount: z.coerce
    .number()
    .positive('Tutar pozitif olmalı')
    .max(1000000, 'Maksimum tutar aşıldı'),
  description: z.string()
    .min(1, 'Açıklama gerekli')
    .max(500, 'Açıklama çok uzun'),
  customerId: z.string().uuid('Geçersiz müşteri'),
})

export type State = {
  message?: string
  errors?: {
    amount?: string[]
    description?: string[]
    customerId?: string[]
  }
}

export async function createInvoice(
  prevState: State,
  formData: FormData
): Promise<State> {
  // Validation
  const validatedFields = InvoiceSchema.safeParse({
    amount: formData.get('amount'),
    description: formData.get('description'),
    customerId: formData.get('customerId'),
  })

  if (!validatedFields.success) {
    return {
      errors: validatedFields.error.flatten().fieldErrors,
      message: 'Eksik alanlar var. Fatura oluşturulamadı.',
    }
  }

  const { amount, description, customerId } = validatedFields.data

  try {
    await db.invoices.create({
      data: { amount, description, customerId },
    })
  } catch (error) {
    return {
      message: 'Veritabanı hatası: Fatura oluşturulamadı.',
    }
  }

  // ⚠️ redirect try-catch dışında olmalı (throw eder)
  revalidatePath('/dashboard/invoices')
  redirect('/dashboard/invoices')
}
```

### redirect() ve notFound() Dikkat Noktaları

```tsx
// ⚠️ redirect() ve notFound() hata fırlatır - try-catch dışında kullan!

export async function updateInvoice(id: string, formData: FormData) {
  let redirectPath: string | null = null

  try {
    // ... işlemler
    redirectPath = `/invoices/${id}`
  } catch (error) {
    return { error: 'Güncelleme başarısız' }
  }

  // try-catch DIŞINDA
  if (redirectPath) {
    revalidatePath(redirectPath)
    redirect(redirectPath)
  }
}
```

---

## 6. Client Components Error Handling

### Event Handler Hataları

```tsx
// components/DeleteButton.tsx
'use client'

import { useState } from 'react'

export function DeleteButton({ id }: { id: string }) {
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const handleDelete = async () => {
    setError(null)
    setLoading(true)

    try {
      const response = await fetch(`/api/items/${id}`, {
        method: 'DELETE',
      })

      if (!response.ok) {
        throw new Error('Silme işlemi başarısız')
      }

      // Başarılı - UI güncelle
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Bir hata oluştu')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <button
        onClick={handleDelete}
        disabled={loading}
        className="bg-red-500 text-white px-4 py-2 rounded"
      >
        {loading ? 'Siliniyor...' : 'Sil'}
      </button>
      {error && <p className="text-red-500 mt-2">{error}</p>}
    </div>
  )
}
```

### useTransition ile Error Boundary'ye Yönlendirme

```tsx
// ⚠️ Next.js 15+ özelliği
'use client'

import { useTransition } from 'react'

export function ActionButton() {
  const [pending, startTransition] = useTransition()

  const handleClick = () => {
    startTransition(() => {
      // Bu hata error.tsx tarafından yakalanır!
      throw new Error('Transition Error')
    })
  }

  return (
    <button onClick={handleClick} disabled={pending}>
      {pending ? 'İşleniyor...' : 'İşlem Yap'}
    </button>
  )
}
```

### react-error-boundary ile Granüler Kontrol

```bash
npm install react-error-boundary
```

```tsx
// components/Dashboard.tsx
'use client'

import { ErrorBoundary } from 'react-error-boundary'

function ErrorFallback({ error, resetErrorBoundary }) {
  return (
    <div className="p-4 bg-red-50 rounded border border-red-200">
      <p className="text-red-600">Widget yüklenemedi</p>
      <button
        onClick={resetErrorBoundary}
        className="mt-2 text-sm text-red-500 underline"
      >
        Tekrar Dene
      </button>
    </div>
  )
}

export function Dashboard() {
  return (
    <div className="grid grid-cols-3 gap-4">
      {/* Her widget izole */}
      <ErrorBoundary FallbackComponent={ErrorFallback}>
        <RevenueChart />
      </ErrorBoundary>

      <ErrorBoundary FallbackComponent={ErrorFallback}>
        <LatestInvoices />
      </ErrorBoundary>

      <ErrorBoundary FallbackComponent={ErrorFallback}>
        <TopCustomers />
      </ErrorBoundary>
    </div>
  )
}
```

---

## 7. Server Components Error Handling

### Fetch Hatalarını Handle Etme

```tsx
// app/dashboard/page.tsx
import { Suspense } from 'react'

async function fetchData() {
  const res = await fetch('https://api.example.com/data', {
    next: { revalidate: 3600 },
  })

  if (!res.ok) {
    // Bu hata error.tsx tarafından yakalanır
    throw new Error('Veri alınamadı')
  }

  return res.json()
}

export default async function DashboardPage() {
  const data = await fetchData()

  return <Dashboard data={data} />
}
```

### Conditional Error UI

```tsx
// app/posts/[id]/page.tsx
export default async function PostPage({
  params,
}: {
  params: { id: string }
}) {
  const res = await fetch(`https://api.example.com/posts/${params.id}`)

  // Hata fırlatmak yerine conditional UI
  if (!res.ok) {
    return (
      <div className="text-center py-12">
        <h2 className="text-xl font-semibold">Post Yüklenemedi</h2>
        <p className="text-gray-600">Lütfen daha sonra tekrar deneyin.</p>
      </div>
    )
  }

  const post = await res.json()
  return <PostDetail post={post} />
}
```

### Parallel Data Fetching with Error Handling

```tsx
// app/dashboard/page.tsx
import { Suspense } from 'react'

async function Revenue() {
  const data = await fetchRevenue() // Hata verirse error.tsx yakalar
  return <RevenueChart data={data} />
}

async function Invoices() {
  const data = await fetchInvoices()
  return <InvoiceList data={data} />
}

export default function DashboardPage() {
  return (
    <div className="grid grid-cols-2 gap-4">
      {/* Her Suspense boundary ayrı error handling */}
      <Suspense fallback={<Loading />}>
        <Revenue />
      </Suspense>

      <Suspense fallback={<Loading />}>
        <Invoices />
      </Suspense>
    </div>
  )
}
```

---

## 8. Sentry Entegrasyonu

### Kurulum

```bash
npx @sentry/wizard@latest -i nextjs
```

### Dosya Yapısı (Wizard Oluşturur)

```
├── sentry.client.config.ts     # Client-side config
├── sentry.server.config.ts     # Server-side config
├── sentry.edge.config.ts       # Edge runtime config
├── next.config.ts              # Sentry wrapper eklenir
├── instrumentation.ts          # Instrumentation hook
└── app/
    └── global-error.tsx        # Sentry entegreli
```

### sentry.client.config.ts

```ts
import * as Sentry from '@sentry/nextjs'

Sentry.init({
  dsn: process.env.NEXT_PUBLIC_SENTRY_DSN,

  // Performance
  tracesSampleRate: process.env.NODE_ENV === 'production' ? 0.1 : 1.0,

  // Session Replay
  replaysSessionSampleRate: 0.1,
  replaysOnErrorSampleRate: 1.0,

  // Debug
  debug: process.env.NODE_ENV === 'development',

  integrations: [
    Sentry.replayIntegration({
      maskAllText: true,
      maskAllInputs: true,
    }),
  ],

  // Environment
  environment: process.env.NODE_ENV,

  // Release tracking
  release: process.env.NEXT_PUBLIC_VERCEL_GIT_COMMIT_SHA,
})

export const onRouterTransitionStart = Sentry.captureRouterTransitionStart
```

### global-error.tsx with Sentry

```tsx
// app/global-error.tsx
'use client'

import * as Sentry from '@sentry/nextjs'
import { useEffect } from 'react'

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  useEffect(() => {
    Sentry.captureException(error, {
      tags: {
        errorBoundary: 'global',
      },
      extra: {
        digest: error.digest,
      },
    })
  }, [error])

  return (
    <html>
      <body>
        <div className="flex flex-col items-center justify-center min-h-screen">
          <h1 className="text-3xl font-bold mb-4">Bir şeyler ters gitti</h1>
          <p className="text-gray-600 mb-6">
            Ekibimiz bilgilendirildi ve sorunu çözmeye çalışıyor.
          </p>
          <button
            onClick={reset}
            className="px-6 py-3 bg-blue-500 text-white rounded"
          >
            Tekrar Dene
          </button>
        </div>
      </body>
    </html>
  )
}
```

### Server Action Error Logging

```tsx
// app/actions/invoices.ts
'use server'

import * as Sentry from '@sentry/nextjs'

export async function createInvoice(formData: FormData) {
  try {
    // ... işlemler
  } catch (error) {
    // Sentry'ye logla
    Sentry.withScope((scope) => {
      scope.setTag('action', 'createInvoice')
      scope.setExtra('formData', Object.fromEntries(formData))
      Sentry.captureException(error)
    })

    return { error: 'Fatura oluşturulamadı' }
  }
}
```

### Custom Error Logging Utility

```tsx
// lib/errorLogger.ts
import * as Sentry from '@sentry/nextjs'

type ErrorContext = {
  component?: string
  action?: string
  userId?: string
  extra?: Record<string, unknown>
}

export function logError(error: Error, context?: ErrorContext) {
  // Development'ta console'a yaz
  if (process.env.NODE_ENV === 'development') {
    console.error('Error:', error)
    console.error('Context:', context)
    return
  }

  // Production'da Sentry'ye gönder
  Sentry.withScope((scope) => {
    if (context?.component) {
      scope.setTag('component', context.component)
    }
    if (context?.action) {
      scope.setTag('action', context.action)
    }
    if (context?.userId) {
      scope.setUser({ id: context.userId })
    }
    if (context?.extra) {
      scope.setExtras(context.extra)
    }

    Sentry.captureException(error)
  })
}
```

---

## 9. Anti-Patterns ve Yaygın Hatalar

### ❌ Anti-Pattern 1: Sadece global-error.tsx Kullanmak

```tsx
// ❌ KÖTÜ - Tek global error boundary
app/
├── global-error.tsx
└── dashboard/
    └── page.tsx  // Herhangi bir hata tüm app'i etkiler

// ✅ İYİ - Katmanlı error boundaries
app/
├── global-error.tsx
├── error.tsx           // Root error
└── dashboard/
    ├── error.tsx       // Dashboard error
    └── page.tsx
```

### ❌ Anti-Pattern 2: Server Action'da throw Kullanmak

```tsx
// ❌ KÖTÜ - Production'da hata mesajı gizlenir
'use server'
export async function submitForm(data: FormData) {
  if (!data.get('email')) {
    throw new Error('Email gerekli') // Client'ta "An error occurred" olur
  }
}

// ✅ İYİ - Return value pattern
'use server'
export async function submitForm(data: FormData) {
  if (!data.get('email')) {
    return { error: 'Email gerekli' }
  }
  return { success: true }
}
```

### ❌ Anti-Pattern 3: error.tsx'te Server Component Kullanmak

```tsx
// ❌ KÖTÜ - error.tsx Server Component olamaz!
// app/error.tsx
export default async function Error({ error }) {
  const data = await fetchData() // ❌ ÇALIŞMAZ
  return <div>{error.message}</div>
}

// ✅ İYİ - Client Component olmalı
// app/error.tsx
'use client'
export default function Error({ error, reset }) {
  return <div>{error.message}</div>
}
```

### ❌ Anti-Pattern 4: redirect() try-catch İçinde

```tsx
// ❌ KÖTÜ - redirect hata fırlatır, catch yakalar
'use server'
export async function createItem(data: FormData) {
  try {
    await db.items.create({ data })
    redirect('/items') // ❌ catch'e düşer!
  } catch (error) {
    return { error: 'Hata oluştu' }
  }
}

// ✅ İYİ - redirect try-catch dışında
'use server'
export async function createItem(data: FormData) {
  try {
    await db.items.create({ data })
  } catch (error) {
    return { error: 'Hata oluştu' }
  }
  
  redirect('/items') // ✅ try-catch dışında
}
```

### ❌ Anti-Pattern 5: global-error.tsx'te html/body Unutmak

```tsx
// ❌ KÖTÜ - Boş beyaz sayfa gösterir
// app/global-error.tsx
'use client'
export default function GlobalError({ error }) {
  return <div>Hata: {error.message}</div>
}

// ✅ İYİ - html ve body tag'leri zorunlu
// app/global-error.tsx
'use client'
export default function GlobalError({ error }) {
  return (
    <html>
      <body>
        <div>Hata: {error.message}</div>
      </body>
    </html>
  )
}
```

### ❌ Anti-Pattern 6: Retry Olmadan Error UI

```tsx
// ❌ KÖTÜ - Kullanıcı çıkmaza girer
export default function Error({ error }) {
  return <div>Hata oluştu</div>
}

// ✅ İYİ - Recovery seçenekleri sun
export default function Error({ error, reset }) {
  return (
    <div>
      <p>Hata oluştu</p>
      <button onClick={reset}>Tekrar Dene</button>
      <a href="/">Ana Sayfaya Dön</a>
    </div>
  )
}
```

---

## 10. Özet Checklist

### ✅ Dosya Yapısı

| # | Dosya | Açıklama | Öncelik |
|---|-------|----------|---------|
| 1 | `app/error.tsx` | Root error boundary | 🔴 Kritik |
| 2 | `app/global-error.tsx` | Root layout hataları | 🔴 Kritik |
| 3 | `app/not-found.tsx` | Global 404 | 🟠 Yüksek |
| 4 | `app/[segment]/error.tsx` | Segment-specific errors | 🟡 Orta |
| 5 | `app/[segment]/not-found.tsx` | Segment-specific 404 | 🟢 Düşük |

### ✅ Error Handling Stratejileri

| Senaryo | Çözüm |
|---------|-------|
| Render hataları | `error.tsx` |
| Root layout hataları | `global-error.tsx` |
| 404 / Kaynak bulunamadı | `not-found.tsx` + `notFound()` |
| Form validation | `useActionState` + return value |
| Server Action hataları | try-catch + return value |
| Event handler hataları | try-catch + useState |
| Async hataları | try-catch + useState |
| API route hataları | try-catch + Response |

### ✅ Best Practices

```
✅ Her kritik segment'e error.tsx ekle
✅ global-error.tsx'te html/body kullan
✅ error.tsx'i 'use client' ile işaretle
✅ Server Action'larda return value pattern kullan
✅ redirect()/notFound() try-catch dışında çağır
✅ Sentry entegrasyonu yap
✅ Error UI'da retry mekanizması sun
✅ Development'ta detaylı, production'da genel mesaj göster
✅ Error logging'i merkezi yap
❌ Sadece global-error.tsx'e güvenme
❌ Server Action'da throw kullanma (expected errors için)
❌ error.tsx'i Server Component yapma
```

### Karar Ağacı

```
Hata nerede oluştu?
│
├─ Server Component render?
│  └─ error.tsx yakalar
│
├─ Client Component render?
│  └─ error.tsx yakalar
│
├─ Server Action?
│  ├─ Expected (validation) → Return value
│  └─ Unexpected (db error) → try-catch + return value
│
├─ Event handler?
│  └─ try-catch + useState
│
├─ Root layout?
│  └─ global-error.tsx
│
└─ Kaynak bulunamadı?
   └─ notFound() + not-found.tsx
```

---

## Sonuç

Next.js App Router'da etkili error handling için:

1. **Katmanlı Boundary'ler**: `error.tsx` + `global-error.tsx` kombinasyonu
2. **Return Value Pattern**: Server Actions'da hataları data olarak dön
3. **Granüler Kontrol**: Her segment için ayrı error boundary
4. **Error Monitoring**: Sentry entegrasyonu ile production tracking
5. **Recovery UI**: Her zaman retry/navigation seçeneği sun

Bu yapı sayesinde kullanıcılar asla beyaz ekran görmez, hatalar izlenir ve recovery mümkün olur.