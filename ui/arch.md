# Mimari Dokümantasyon

Bu dokümantasyon, projenin kullandığı teknoloji stack'ini ve her bir teknolojinin amacını açıklar.

---

## 🌐 Web Stack (Next.js)

### Core Framework

#### **Next.js 16 (App Router)**
- Modern React framework'ü, server-side rendering ve static site generation desteği
- App Router ile gelişmiş routing ve layout sistemi
- Built-in optimizasyonlar (image, font, script)

#### **React 19**
- En son React versiyonu, gelişmiş performans ve yeni özellikler
- React Compiler ile otomatik optimizasyon desteği

#### **TypeScript (strict mode)**
- Tip güvenliği sağlar, hata yakalama ve geliştirici deneyimini iyileştirir
- Strict mode ile maksimum tip kontrolü

#### **Turborepo**
- Monorepo yapısı için build sistemi
- Paralel build ve cache mekanizması ile hızlı geliştirme

#### **pnpm**
- Hızlı ve disk alanından tasarruflu paket yöneticisi
- Workspace desteği ile monorepo için ideal

---

### Styling & UI

#### **Tailwind CSS**
- Utility-first CSS framework, hızlı ve tutarlı stil geliştirme

#### **shadcn/ui**
- Kopyala-yapıştır tabanlı, özelleştirilebilir UI bileşen kütüphanesi
- Radix UI üzerine kurulu, erişilebilirlik odaklı

#### **Radix UI**
- Erişilebilir, headless UI bileşenleri
- shadcn/ui'nin temelini oluşturur

#### **Framer Motion (LazyMotion pattern)**
- Animasyon kütüphanesi
- LazyMotion ile yaklaşık %50 bundle boyutu azaltma

#### **Lucide React**
- Modern, hafif ikon kütüphanesi
- Tree-shaking desteği ile sadece kullanılan ikonlar bundle'a eklenir

#### **tailwind-merge + clsx**
- Tailwind class'larını birleştirme ve çakışmaları çözme
- Dinamik class oluşturma için yardımcı fonksiyonlar

#### **next-themes**
- Dark/light mode desteği
- Sistem tercihini otomatik algılama

#### **tailwindcss-animate**
- Tailwind için animasyon utility'leri
- Hazır animasyon sınıfları

---

### UI Components

#### **vaul**
- Drawer ve bottom sheet bileşenleri
- Mobil ve desktop için uygun açılır paneller

#### **cmdk**
- Command palette (⌘K) bileşeni
- Hızlı arama ve navigasyon için

#### **sonner**
- Toast bildirimleri
- Modern ve özelleştirilebilir bildirim sistemi

#### **embla-carousel**
- Carousel/slider bileşeni
- Performanslı ve özelleştirilebilir

---

### State & Data Fetching

#### **TanStack Query (React Query)**
- Server state yönetimi
- Caching, refetching, optimistic updates
- API çağrılarını kolaylaştırır

#### **Zustand**
- Hafif, basit state management kütüphanesi
- Client-side state için minimal boilerplate

#### **nuqs**
- URL query string yönetimi
- Type-safe URL parametreleri

---

### Forms & Validation

#### **React Hook Form**
- Performanslı form yönetimi
- Minimal re-render ile hızlı formlar

#### **Zod**
- TypeScript-first schema validation
- Runtime type checking ve validation

#### **@hookform/resolvers**
- React Hook Form ile Zod entegrasyonu
- Schema validation'ı formlara bağlar

---

### Testing

#### **Vitest**
- Hızlı unit test framework'ü
- Vite tabanlı, Jest uyumlu API

#### **React Testing Library**
- React bileşenlerini test etme
- Kullanıcı odaklı test yaklaşımı

#### **Playwright**
- End-to-end (E2E) test framework'ü
- Cross-browser test desteği

#### **MSW (Mock Service Worker)**
- API mock'lamak için
- Test ve development sırasında gerçekçi API simülasyonu

#### **Storybook**
- UI bileşen geliştirme ve dokümantasyon
- İzole ortamda bileşen testi

---

### Code Quality & Developer Experience

#### **ESLint**
- JavaScript/TypeScript kod kalitesi kontrolü
- Hata ve potansiyel sorunları yakalar

#### **Prettier**
- Kod formatlama
- Tutarlı kod stili sağlar

#### **Husky + lint-staged**
- Git hook'ları ile pre-commit kontrolleri
- Sadece değişen dosyaları lint'ler

#### **commitlint**
- Commit mesajlarını standartlaştırır
- Conventional commits formatı

#### **T3 Env**
- Type-safe environment variable yönetimi
- Runtime ve build-time validation

#### **knip**
- Kullanılmayan kod, bağımlılık ve export'ları bulur
- Bundle boyutunu küçültmeye yardımcı

---

### DevTools

#### **@tanstack/react-query-devtools**
- React Query için geliştirici araçları
- Cache durumunu görselleştirme ve debug

---

### Monitoring & Analytics

#### **Sentry**
- Hata takibi ve performans izleme
- Production'da sorunları yakalama ve raporlama

#### **Vercel Analytics**
- Web vitals ve kullanıcı analitiği
- Performans metrikleri

---

### Deployment

#### **Vercel**
- Next.js için optimize edilmiş hosting platformu
- Otomatik CI/CD ve edge network

#### **GitHub Actions**
- CI/CD pipeline'ları
- Otomatik test ve deployment

---

### Utility Libraries

#### **date-fns**
- Tarih manipülasyonu ve formatlama
- Hafif ve modüler tarih kütüphanesi

#### **@tanstack/react-table**
- Güçlü tablo/data grid bileşeni
- Sorting, filtering, pagination desteği

#### **@tremor/react**
- Modern chart kütüphanesi
- Recharts'a göre yaklaşık %60 daha hafif

#### **react-dropzone**
- Dosya drag & drop desteği
- Dosya yükleme için kullanıcı dostu arayüz

#### **@dnd-kit**
- Drag and drop işlevselliği
- Erişilebilir ve performanslı

#### **react-intersection-observer**
- Scroll ve görünürlük takibi
- Lazy loading ve infinite scroll için

#### **usehooks-ts**
- TypeScript için hazır React hook'ları
- Yaygın kullanılan hook pattern'leri

---

### Security

#### **dompurify**
- HTML sanitization
- XSS saldırılarına karşı koruma

---

### Internationalization (i18n)

#### **next-intl**
- Next.js için çoklu dil desteği
- Routing ve mesaj yönetimi

---

### SEO & Performance

#### **next/font**
- Font optimizasyonu
- Otomatik font subsetting ve preloading

#### **next/image**
- Optimize edilmiş görsel bileşeni
- Otomatik lazy loading ve format dönüşümü

#### **@vercel/og**
- Dinamik Open Graph görsel üretimi
- Social media paylaşımları için

#### **next-sitemap**
- Otomatik sitemap oluşturma
- SEO için gerekli XML dosyaları

---

### Performance Optimization

#### **React Compiler**
- React 19 ile otomatik memoization
- Manuel useMemo/useCallback gereksinimini azaltır

#### **Turbopack**
- Next.js 16'da deneysel ultra-hızlı bundler
- Webpack'e alternatif, çok daha hızlı build süreleri

---

## 📱 Mobile Stack (React Native)

### Core Framework

#### **Expo SDK**
- React Native geliştirme platformu
- Native modül erişimi ve build araçları

#### **React Native (New Architecture & Bridgeless Mode)**
- Cross-platform mobil uygulama framework'ü
- New Architecture ile gelişmiş performans
- Bridgeless mode ile daha hızlı JavaScript-Native iletişimi

#### **TypeScript (strict mode)**
- Tip güvenliği (web stack ile aynı)

#### **Expo Router v4**
- File-based routing sistemi
- Web benzeri navigasyon deneyimi

#### **Turborepo**
- Monorepo desteği (web ile paylaşılan)

---

### Styling & UI

#### **UniWind v4**
- Tailwind CSS'in React Native uyarlaması
- Web ile tutarlı stil sistemi

#### **rn-primitives**
- React Native için primitive UI bileşenleri
- shadcn/ui benzeri yaklaşım

#### **Reusables**
- shadcn/ui bileşenlerinin React Native uyarlaması
- rn-primitives üzerine kurulu, web ile paylaşılan tasarım sistemi

#### **Lucide React Native**
- İkon kütüphanesi (web ile aynı)

#### **react-native-reanimated**
- Yüksek performanslı animasyonlar
- Native thread'de çalışır

#### **@shopify/flash-list**
- Performanslı liste bileşeni
- FlatList'e alternatif, daha hızlı

#### **expo-blur**
- Blur efektleri
- Modern UI tasarımları için

#### **react-native-gesture-handler**
- Native gesture tanıma
- Dokunmatik etkileşimler için

#### **sonner-native**
- Toast bildirimleri (mobil uyarlama)

---

### State & Data Fetching

#### **TanStack Query (React Query)**
- Server state yönetimi (web ile aynı)

#### **Zustand**
- Client state yönetimi (web ile aynı)

#### **react-native-mmkv**
- Hızlı key-value storage
- AsyncStorage'a alternatif, çok daha hızlı

---

### Forms & Validation

#### **React Hook Form**
- Form yönetimi (web ile aynı)

#### **Zod**
- Schema validation (web ile aynı)

#### **@hookform/resolvers**
- Form-Zod entegrasyonu (web ile aynı)

---

### Advanced Graphics

#### **React Native Skia**
- 2D grafik çizimi
- Yüksek performanslı görselleştirmeler

---

### Testing

#### **Jest**
- Unit test framework'ü
- React Native için standart

#### **React Native Testing Library (RNTL)**
- Bileşen testleri
- Kullanıcı odaklı test yaklaşımı

#### **Maestro**
- E2E test framework'ü
- Mobil uygulamalar için optimize edilmiş

---

### Code Quality

#### **ESLint (React Native config)**
- Kod kalitesi kontrolü (web ile aynı yaklaşım)

#### **Prettier**
- Kod formatlama (web ile aynı)

#### **Husky + lint-staged**
- Pre-commit kontrolleri (web ile aynı)

---

### Development & Build

#### **Expo Go & Development Builds**
- Hızlı test süreçleri
- Native modül testi için development build'ler

#### **EAS Build**
- Cloud-based build servisi
- iOS ve Android build'leri

#### **EAS Submit**
- App store'lara otomatik gönderim
- CI/CD entegrasyonu

#### **EAS Update**
- Over-the-air (OTA) güncellemeler
- Store onayı olmadan hızlı güncelleme

---

### Monitoring & Analytics

#### **Sentry React Native**
- Hata takibi (web ile aynı servis)

#### **PostHog veya Amplitude**
- Ürün analitiği
- Kullanıcı davranışı takibi

---

### Utility Libraries

#### **date-fns**
- Tarih manipülasyonu (web ile aynı)

#### **react-native-safe-area-context**
- Safe area yönetimi
- Notch ve status bar için doğru layout

#### **Victory Native XL**
- Chart/graph kütüphanesi
- Veri görselleştirme

#### **expo-haptics**
- Haptic feedback
- Dokunsal geri bildirim

#### **expo-localization**
- Lokalizasyon desteği
- Dil ve bölge bilgisi

---

## 📊 Stack Karşılaştırması

### Web ve Mobile Arasında Paylaşılan Teknolojiler

- **TypeScript**: Her iki platformda da tip güvenliği
- **Turborepo**: Monorepo yönetimi
- **TanStack Query**: Server state yönetimi
- **Zustand**: Client state yönetimi
- **React Hook Form + Zod**: Form yönetimi ve validation
- **ESLint + Prettier**: Kod kalitesi
- **Sentry**: Hata takibi
- **date-fns**: Tarih işlemleri

### Platform-Specific Teknolojiler

- **Web**: Next.js, Tailwind CSS, shadcn/ui, Vercel
- **Mobile**: Expo, React Native, UniWind, EAS

---

## 🎯 Mimari Prensipler

1. **Code Sharing**: Mümkün olduğunca business logic ve utility'ler paylaşılır
2. **Type Safety**: Strict TypeScript her yerde
3. **Performance**: Bundle size ve runtime performansı optimize edilir
4. **Developer Experience**: Modern tooling ve otomasyon
5. **Accessibility**: Erişilebilirlik her zaman öncelikli
6. **Testing**: Unit, integration ve E2E test coverage
7. **Monitoring**: Production'da hata ve performans takibi
