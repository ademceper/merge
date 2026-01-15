# 🎨 STITCH PROMPT ŞABLONU - MERGE UI WEB EKRAN TASARIMI

Bu dokümantasyon, Stitch AI ile **WEB** ekran tasarımları oluştururken kullanılacak standart prompt şablonunu içerir. Her ekran için bu şablonu kullanarak tutarlı ve detaylı tasarımlar elde edebilirsiniz.

---

## 📋 PROMPT YAPISI

### 1. **EKRAN BİLGİLERİ & BAĞLAM**

```
Ekran Adı: [Ekranın tam adı]
Platform: Web (Desktop + Tablet + Mobile Responsive)
Framework: Next.js 16.1.2 (App Router)
Bölüm: [Müşteri Tarafı - Pazaryeri / Satıcı Sitesi / Satıcı Paneli / Admin Paneli]
Kategori: [Onboarding / Ana Sayfa / Ürün / Sepet / Profil / vb.]
Routing: Next.js App Router (app/ dizini)
```

### 2. **TEMEL ÖZELLİKLER & AMAÇ**

```
Ekranın Amacı:
- [Ana fonksiyon 1]
- [Ana fonksiyon 2]
- [Ana fonksiyon 3]

Kullanıcı Hedefi:
- [Kullanıcı ne yapmak istiyor?]
- [Hangi sorunu çözüyor?]

Merge İçin Özel Değer:
- Hibrit platform: Pazaryeri + kendi site ürünleri
- Multi-vendor: Farklı satıcıların ürünleri
- Personalization: AI-driven öneriler
- B2B/B2C hibrit model
```

### 3. **TEKNOLOJİ STACK & YAPILANDIRMA**

```
Framework & Build:
- Next.js 16.1.2 (App Router)
- React 19.2.3
- TypeScript 5.9.3
- Turbopack (dev mode)
- Server Components (default) + Client Components ("use client")

UI Kütüphanesi:
- @merge/ui (workspace package)
- Radix UI primitives (via @merge/ui)
- Phosphor Icons (@phosphor-icons/react)
- shadcn/ui style (radix-nova)

Stil Sistemi:
- Tailwind CSS 4.1.11
- PostCSS (@tailwindcss/postcss)
- CSS Variables (oklch color space)
- Dark mode: next-themes (class-based)

Font:
- Space Grotesk (Google Fonts via next/font/google)
- Weights: 300 (Light), 400 (Regular), 500 (Medium), 700 (Bold)
- Variable: --font-sans
- CSS Variable: var(--font-sans)

State Management:
- React Server Components (default)
- React hooks (useState, useEffect, etc.)
- TanStack Query (if needed for data fetching)

Routing:
- Next.js App Router
- File-based routing (app/ dizini)
- Dynamic routes: [param]
- Route groups: (group)
- Layouts: layout.tsx
- Loading: loading.tsx
- Error: error.tsx
- Not Found: not-found.tsx
```

### 4. **TASARIM PRENSİPLERİ**

```
Tasarım DNA'sı:
- Minimalist: Monochrome palette (siyah-beyaz)
- Geometric: Keskin, matematiksel formlar
- Negative Space: Boşluk = tasarım
- Flat Design: Gradyan yok, shadow minimal, depth yok
- Typography as Hero: Yazı tipi kimlik
- Border radius: Minimal (0.45rem default)

Referans Stiller:
- Apple: Clarity, Deference, Depth (minimal), Consistency
- Vercel: Speed, Precision, Simplicity, Technical
- Merge Uyarlaması: Monochrome-first, geometric precision
```

### 5. **EKRAN DÜZENİ & LAYOUT**

```
Responsive Breakpoints:
- Mobile: < 768px (1 kolon, vertical stack)
- Tablet: 768px - 1024px (2 kolon, grid)
- Desktop: > 1024px (3-4 kolon, max-width container)

Layout Grid (Desktop - 1440px standard):
├─ Container: Max-width 1200px, centered, padding 24px
├─ Header Zone: 80px yükseklik (sticky)
│  ├─ Logo: 32x32px (sol)
│  ├─ Navigation: Mega menu (merkez)
│  ├─ Search: 400px genişlik (merkez)
│  └─ Actions: Sepet + Profil (sağ, 120px)
├─ Content Zone: Flexible height
│  ├─ Section spacing: 48px vertical gap
│  ├─ Horizontal padding: 24px
│  └─ Grid gap: 24px
├─ Footer Zone: 200px yükseklik
└─ Safe areas: Handled by CSS

Layout Grid (Tablet - 768px):
├─ Container: Max-width 100%, padding 16px
├─ Header: 64px yükseklik
├─ Content: 2 kolon grid
└─ Footer: 150px yükseklik

Layout Grid (Mobile - 375px):
├─ Container: Full width, padding 16px
├─ Header: 56px yükseklik (sticky)
├─ Content: 1 kolon, vertical stack
└─ Footer: 120px yükseklik

Spacing Sistemi:
- 8px grid sistemi (base unit)
- Padding: 16px, 24px, 32px, 48px
- Gap: 8px, 12px, 16px, 24px, 32px, 48px
- Margin: 16px, 24px, 32px, 48px, 64px
```

### 6. **RENK PALETİ (OKLCH COLOR SPACE)**

```
Light Mode (CSS Variables):
- --background: oklch(1 0 0) - Pure white
- --foreground: oklch(0.145 0 0) - Pure black
- --card: oklch(1 0 0) - White
- --card-foreground: oklch(0.145 0 0) - Black
- --popover: oklch(1 0 0) - White
- --popover-foreground: oklch(0.145 0 0) - Black
- --primary: oklch(0.205 0 0) - Near black
- --primary-foreground: oklch(0.985 0 0) - Near white
- --secondary: oklch(0.97 0 0) - Light gray
- --secondary-foreground: oklch(0.205 0 0) - Near black
- --muted: oklch(0.97 0 0) - Light gray
- --muted-foreground: oklch(0.556 0 0) - Medium gray
- --accent: oklch(0.97 0 0) - Light gray
- --accent-foreground: oklch(0.205 0 0) - Near black
- --destructive: oklch(0.58 0.22 27) - Red
- --border: oklch(0.922 0 0) - Light border
- --input: oklch(0.922 0 0) - Input border
- --ring: oklch(0.708 0 0) - Focus ring
- --radius: 0.45rem - Border radius

Dark Mode (CSS Variables):
- --background: oklch(0.145 0 0) - True black (OLED optimized)
- --foreground: oklch(0.985 0 0) - Near white
- --card: oklch(0.205 0 0) - Dark gray
- --card-foreground: oklch(0.985 0 0) - Near white
- --popover: oklch(0.205 0 0) - Dark gray
- --popover-foreground: oklch(0.985 0 0) - Near white
- --primary: oklch(0.87 0.00 0) - Light gray
- --primary-foreground: oklch(0.205 0 0) - Dark
- --secondary: oklch(0.269 0 0) - Dark gray
- --secondary-foreground: oklch(0.985 0 0) - Near white
- --muted: oklch(0.269 0 0) - Dark gray
- --muted-foreground: oklch(0.708 0 0) - Medium gray
- --accent: oklch(0.371 0 0) - Medium dark gray
- --accent-foreground: oklch(0.985 0 0) - Near white
- --destructive: oklch(0.704 0.191 22.216) - Red
- --border: oklch(1 0 0 / 10%) - Subtle border
- --input: oklch(1 0 0 / 15%) - Input border
- --ring: oklch(0.556 0 0) - Focus ring
- --radius: 0.45rem - Border radius

Kullanım:
- Tailwind classes: bg-background, text-foreground, border-border
- CSS Variables: var(--background), var(--foreground)
- Dark mode: .dark class (next-themes)
```

### 7. **TİPOGRAFİ**

```
Font Family:
- Space Grotesk (Google Fonts)
- CSS Variable: var(--font-sans)
- Fallback: system-ui, sans-serif

Font Weights:
- Light: 300 (font-light)
- Regular: 400 (font-normal)
- Medium: 500 (font-medium)
- Bold: 700 (font-bold)

Başlık (H1):
- Font: Space Grotesk Bold (700)
- Size: 36px (desktop) / 28px (tablet) / 24px (mobile)
- Line Height: 44px / 34px / 30px
- Letter Spacing: -0.01em
- Color: text-foreground
- Tailwind: text-4xl font-bold (desktop)

Alt Başlık (H2):
- Font: Space Grotesk Bold (700)
- Size: 28px (desktop) / 24px (tablet) / 20px (mobile)
- Line Height: 36px / 30px / 26px
- Letter Spacing: -0.01em
- Tailwind: text-3xl font-bold (desktop)

H3:
- Font: Space Grotesk Semibold (600)
- Size: 24px (desktop) / 20px (tablet) / 18px (mobile)
- Line Height: 32px / 28px / 24px
- Tailwind: text-2xl font-semibold

Body Text:
- Font: Space Grotesk Regular (400)
- Size: 16px (all breakpoints)
- Line Height: 24px
- Letter Spacing: 0
- Tailwind: text-base

Small Text:
- Font: Space Grotesk Regular (400)
- Size: 14px
- Line Height: 20px
- Tailwind: text-sm

Caption:
- Font: Space Grotesk Regular (400)
- Size: 12px
- Line Height: 16px
- Color: text-muted-foreground
- Tailwind: text-xs text-muted-foreground

Button Text:
- Font: Space Grotesk Medium (500) or Bold (700)
- Size: 16px
- Letter Spacing: 0.01em (uppercase için)
- Tailwind: text-base font-medium
```

### 8. **BİLEŞENLER & ELEMENTLER**

```
@merge/ui Bileşen Kütüphanesi:
- Accordion, Alert, Alert Dialog, Aspect Ratio, Avatar
- Badge, Breadcrumb, Button, Button Group, Calendar
- Card, Carousel, Chart, Checkbox, Collapsible
- Command, Context Menu, Dialog, Drawer, Dropdown Menu
- Empty, Field, Hover Card, Input, Input Group, Input OTP
- Item, KBD, Label, Menubar, Navigation Menu, Pagination
- Popover, Progress, Radio Group, Resizable, Scroll Area
- Select, Separator, Sheet, Sidebar, Skeleton, Slider
- Sonner (Toast), Spinner, Switch, Table, Tabs
- Textarea, Toggle, Toggle Group, Tooltip

Import Pattern:
import { Component } from "@merge/ui/components/component-name"

Icon Kütüphanesi:
- Phosphor Icons (@phosphor-icons/react)
- Import: import { IconName } from "@phosphor-icons/react"
- Size: 16px, 20px, 24px, 32px (w-4, w-5, w-6, w-8)

Header:
- Sticky: position: sticky, top: 0
- Background: bg-background
- Border: border-b border-border
- Height: 80px (desktop), 64px (tablet), 56px (mobile)
- Z-index: 50

Content Area:
- Container: max-w-7xl mx-auto px-6 (desktop)
- Scroll: overflow-y-auto (if needed)
- Grid: grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6

Action Buttons:
- Primary: bg-primary text-primary-foreground
- Secondary: bg-secondary text-secondary-foreground
- Outline: border border-border bg-background
- Ghost: hover:bg-accent
- Size: h-10 px-4 (default), h-9 px-3 (sm), h-11 px-6 (lg)

Empty States:
- Component: Empty from @merge/ui/components/empty
- Icon: Phosphor icon
- Message: text-muted-foreground
- CTA: Button component

Loading States:
- Skeleton: Skeleton from @merge/ui/components/skeleton
- Spinner: Spinner from @merge/ui/components/spinner
- Pattern: Same layout as content, gray boxes
```

### 9. **ANİMASYON & GEÇİŞLER**

```
CSS Transitions:
- Duration: 150ms, 200ms, 300ms
- Easing: ease-out, ease-in-out
- Properties: opacity, transform, color, background-color

Hover States:
- Scale: hover:scale-105 (subtle)
- Opacity: hover:opacity-90
- Border: hover:border-2 (from border)
- Background: hover:bg-accent
- Transition: transition-all duration-200

Focus States:
- Ring: focus-visible:ring-2 focus-visible:ring-ring
- Outline: focus-visible:outline-none
- Offset: focus-visible:ring-offset-2

Page Transitions:
- Next.js handles route transitions
- Fade: opacity transition
- Duration: 200ms

Micro Interactions:
- Button press: active:scale-95
- Duration: 100ms
- Easing: ease-in-out

Animations (if needed):
- Framer Motion (if complex animations needed)
- CSS animations for simple cases
- Prefers-reduced-motion: respect user preference
```

### 10. **ETKİLEŞİM & DAVRANIŞ**

```
Kullanıcı Aksiyonları:
- Click/Tap: onClick handlers
- Hover: CSS :hover (desktop only)
- Focus: Keyboard navigation
- Form submission: onSubmit handlers

Form Validasyonu:
- Real-time: onChange validation
- On submit: onSubmit validation
- Error messages: Field component from @merge/ui
- Success feedback: Toast (Sonner)

Infinite Scroll:
- Intersection Observer API
- Loading indicator: Skeleton or Spinner
- Trigger: 200px before bottom

Modal/Dialog:
- Component: Dialog from @merge/ui/components/dialog
- Backdrop: bg-black/50
- Close: ESC key or click outside
- Animation: Fade in/out

Sheet/Drawer:
- Component: Sheet from @merge/ui/components/sheet
- Slide from: right, left, top, bottom
- Backdrop: bg-black/50
```

### 11. **ERİŞİLEBİLİRLİK**

```
Semantic HTML:
- Use proper HTML elements (button, nav, main, etc.)
- ARIA labels where needed
- Roles: role="button", role="navigation", etc.

Keyboard Navigation:
- Tab order: Logical flow
- Focus states: Visible ring
- Skip links: Skip to main content
- Keyboard shortcuts: Standard (Enter, Esc, etc.)

Screen Reader:
- Alt text: All images
- ARIA labels: Icon-only buttons
- ARIA descriptions: Complex components
- Live regions: Dynamic content updates

Contrast Ratios:
- Text: WCAG AA (4.5:1) minimum
- Large text: WCAG AA (3:1) minimum
- AAA preferred where possible
- Test with: Chrome DevTools Lighthouse

Reduced Motion:
- CSS: @media (prefers-reduced-motion: reduce)
- Disable animations: animation: none
- Static alternatives: Provide static states
```

### 12. **RESPONSIVE TASARIM**

```
Mobile First Approach:
- Base styles: Mobile (< 768px)
- Tablet: md: prefix (768px+)
- Desktop: lg: prefix (1024px+)
- Large desktop: xl: prefix (1280px+)

Breakpoints:
- sm: 640px
- md: 768px
- lg: 1024px
- xl: 1280px
- 2xl: 1536px

Grid System:
- Mobile: grid-cols-1
- Tablet: md:grid-cols-2
- Desktop: lg:grid-cols-3 or lg:grid-cols-4

Spacing:
- Mobile: p-4 (16px)
- Tablet: md:p-6 (24px)
- Desktop: lg:p-8 (32px)

Typography:
- Mobile: text-base (16px)
- Tablet: md:text-lg (18px)
- Desktop: lg:text-xl (20px)

Container:
- Mobile: w-full px-4
- Tablet: md:max-w-2xl md:mx-auto
- Desktop: lg:max-w-7xl lg:mx-auto
```

### 13. **DURUMLAR & VARYANTLAR**

```
Loading State:
- Skeleton loader: Skeleton component
- Spinner: Spinner component
- Pattern: Match content layout
- Position: Center or inline

Empty State:
- Component: Empty from @merge/ui
- Icon: Phosphor icon (appropriate)
- Message: Descriptive text
- CTA: Button to take action
- Design: Centered, minimal

Error State:
- Icon: Alert circle
- Message: Error description
- Retry button: Primary button
- Design: Centered, clear

Success State:
- Toast: Sonner component
- Message: Success description
- Auto-dismiss: 3-5 seconds
- Position: Top right (default)

Offline State:
- Banner: Top of page
- Message: "No internet connection"
- Cached content: Show if available
- Retry: Button to reconnect

Dark Mode:
- Toggle: Theme switcher (next-themes)
- Transition: Smooth color transition
- Images: No change (product photos)
- Icons: Auto-invert if needed
```

### 14. **TEKNİK DETAYLAR**

```
Performance:
- Image optimization: next/image
- Code splitting: Automatic (Next.js)
- Lazy loading: Intersection Observer
- Font loading: next/font (optimized)
- CSS: Tailwind (purged unused)

SEO:
- Metadata: Metadata API (Next.js)
- Open Graph: og: tags
- Structured data: JSON-LD
- Sitemap: sitemap.xml
- Robots: robots.txt

API Integration:
- Server Components: Direct fetch
- Client Components: useEffect + fetch or TanStack Query
- Error handling: Error boundaries
- Loading: Suspense boundaries

Build & Deploy:
- Build: next build
- Output: Static or Server
- Environment: .env.local
- Vercel: Recommended platform
```

### 15. **ÖZEL NOTLAR & KISITLAMALAR**

```
Özel Gereksinimler:
- Server Components by default
- Client Components: "use client" directive
- No useEffect in Server Components
- Async components: Supported in Server Components

Kaçınılması Gerekenler:
- Gradyan kullanma
- Aşırı shadow/depth
- Renkli accent'ler (monochrome only)
- Aşırı animasyon
- Karmaşık layout
- Inline styles (use Tailwind)

Best Practices:
- 8px grid sistemi
- Consistent spacing
- Pixel-perfect alignment
- Performance-first
- Accessibility-first
- Mobile-first responsive
- Semantic HTML
- TypeScript strict mode
```

### 16. **KOD ÖRNEKLERİ**

```
Layout Structure:
```tsx
import type { Metadata } from "next";
import { Space_Grotesk } from "next/font/google";
import "@merge/ui/globals.css";
import { Providers } from "@/components/providers";

const spaceGrotesk = Space_Grotesk({
  subsets: ["latin"],
  weight: ["300", "400", "500", "700"],
  variable: "--font-sans",
});

export const metadata: Metadata = {
  title: "Page Title",
  description: "Page description",
};

export default function PageLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className={spaceGrotesk.variable}>
      <body className={`${spaceGrotesk.variable} antialiased`}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
```

Component Example:
```tsx
import { Button } from "@merge/ui/components/button";
import { Card, CardContent, CardHeader, CardTitle } from "@merge/ui/components/card";
import { cn } from "@merge/ui/lib/utils";

export function ExampleComponent() {
  return (
    <div className="container mx-auto px-4 py-8">
      <Card>
        <CardHeader>
          <CardTitle>Title</CardTitle>
        </CardHeader>
        <CardContent>
          <Button>Action</Button>
        </CardContent>
      </Card>
    </div>
  );
}
```

Responsive Example:
```tsx
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 p-4 md:p-6 lg:p-8">
  {/* Content */}
</div>
```
```

---

## 🎯 KULLANIM TALİMATLARI

1. **Ekran Seçimi**: Hangi ekranı tasarlayacağınızı belirleyin
2. **Şablonu Doldurun**: Yukarıdaki şablonu ekran özelliklerine göre doldurun
3. **Teknoloji Detayları**: Web-specific teknolojileri unutmayın (Next.js, Server Components, etc.)
4. **Stitch'e Verin**: Tamamlanan prompt'u Stitch AI'a verin
5. **İterasyon**: Gerekirse prompt'u iyileştirin ve tekrar deneyin

---

## 📌 İPUÇLARI

- **Next.js App Router**: Server Components kullanımını unutmayın
- **Responsive**: Mobile-first yaklaşım, breakpoint'leri doğru kullanın
- **Dark Mode**: next-themes ile otomatik, CSS variables kullanın
- **Performance**: next/image, code splitting, lazy loading
- **Accessibility**: Semantic HTML, ARIA labels, keyboard navigation
- **TypeScript**: Strict mode, type safety
- **Tailwind**: Utility-first, no inline styles
- **Components**: @merge/ui kütüphanesini kullanın
- **Icons**: Phosphor Icons kullanın
- **Font**: Space Grotesk, CSS variable ile

---

## 🔗 İLGİLİ DOSYALAR

- Layout: `apps/web/app/layout.tsx`
- Components: `packages/ui/src/components/`
- Styles: `packages/ui/src/styles/globals.css`
- Config: `apps/web/next.config.mjs`
- TypeScript: `apps/web/tsconfig.json`
