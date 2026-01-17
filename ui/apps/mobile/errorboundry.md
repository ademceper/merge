# 🛡️ Error Boundary Best Practices - Kapsamlı Rehber

## İçindekiler
1. [Error Boundary Nedir?](#1-error-boundary-nedir)
2. [Error Boundary'nin Sınırlamaları](#2-error-boundarynin-sınırlamaları)
3. [3 Katmanlı Strateji](#3-3-katmanlı-strateji)
4. [Expo Router'da ErrorBoundary](#4-expo-routerda-errorboundary)
5. [react-error-boundary Kütüphanesi](#5-react-error-boundary-kütüphanesi)
6. [Async Error Handling](#6-async-error-handling)
7. [Error Logging & Monitoring](#7-error-logging--monitoring)
8. [Anti-Patterns - Kaçınılması Gerekenler](#8-anti-patterns---kaçınılması-gerekenler)
9. [Özet Checklist](#9-özet-checklist)

---

## 1. Error Boundary Nedir?

Error Boundary, React'ın hata yakalama mekanizmasıdır. JavaScript'teki `try-catch` bloğuna benzer şekilde çalışır, ancak component'ler için tasarlanmıştır.

### Temel Özellikler:
- ✅ Child component tree'deki rendering hatalarını yakalar
- ✅ Lifecycle method hatalarını yakalar
- ✅ Constructor hatalarını yakalar
- ✅ Fallback UI gösterir (beyaz ekran yerine)
- ✅ Uygulamanın tamamen çökmesini engeller

### Facebook Messenger Örneği:
Facebook Messenger, her bölümü (sidebar, info panel, mesaj listesi, input) ayrı Error Boundary'ler ile sarar. Bir bölüm çökerse diğerleri çalışmaya devam eder.

---

## 2. Error Boundary'nin Sınırlamaları

### ❌ YAKALAYAMAZ:

| Hata Tipi | Açıklama | Çözüm |
|-----------|----------|-------|
| **Event Handlers** | `onPress`, `onClick` içindeki hatalar | `try-catch` kullan |
| **Async Code** | `setTimeout`, `Promise`, `fetch` | `try-catch` + `useErrorBoundary` hook |
| **Server-Side Rendering** | SSR sırasındaki hatalar | Server-side error handling |
| **Error Boundary Kendisi** | Boundary'nin kendi hataları | Parent boundary yakalar |

### Örnek - Event Handler Hatası:
```tsx
// ❌ Error Boundary YAKALAMAZ
function BuggyButton() {
  const handleClick = () => {
    throw new Error("Event handler error"); // Yakalanmaz!
  };
  return <Button onPress={handleClick} title="Click" />;
}

// ✅ Doğru yaklaşım - try-catch
function SafeButton() {
  const handleClick = () => {
    try {
      throw new Error("Event handler error");
    } catch (error) {
      console.error(error);
      // Hata state'e kaydet veya useErrorBoundary kullan
    }
  };
  return <Button onPress={handleClick} title="Click" />;
}
```

---

## 3. 3 Katmanlı Strateji

En iyi yaklaşım, hataları farklı seviyelerde yakalamaktır:

```
┌─────────────────────────────────────────────────────┐
│             KATMAN 1: GLOBAL                        │
│             (app/_layout.tsx)                       │
│             Son çare - Tüm uygulama                 │
│  ┌───────────────────────────────────────────────┐  │
│  │          KATMAN 2: SAYFA/NAVIGATOR            │  │
│  │          (her route dosyası)                  │  │
│  │          Sayfa seviyesi izolasyon             │  │
│  │  ┌─────────────────────────────────────────┐  │  │
│  │  │       KATMAN 3: COMPONENT               │  │  │
│  │  │       (kritik widget'lar)               │  │  │
│  │  │       En granüler kontrol               │  │  │
│  │  └─────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

### Strateji Karşılaştırması:

| Strateji | Avantaj | Dezavantaj | Ne Zaman Kullan |
|----------|---------|------------|-----------------|
| **Top-Level** | Basit, tüm app'i korur | En az granüler, tek hata tüm UI'ı etkiler | Her zaman (son çare) |
| **Sayfa/Layout** | Orta granülerlik, izolasyon | Hangi component hatalı belirsiz | Kritik sayfalar |
| **Component** | En granüler, izole | Kod tekrarı olabilir | Bağımsız widget'lar |

### Best Practice:
**Kombinasyon kullan!** Global + kritik sayfalarda Page-level + riskli component'lerde Component-level

---

## 4. Expo Router'da ErrorBoundary

### 4.1 Temel Kullanım (Named Export)

Expo Router'da ErrorBoundary, **aynı dosyadan export edilir** (Next.js'teki gibi ayrı `error.tsx` dosyası YOK):

```tsx
// app/(tabs)/cart.tsx
import { View, Text, Button } from 'react-native';
import { type ErrorBoundaryProps } from 'expo-router';

// ✅ Named export ile ErrorBoundary
export function ErrorBoundary({ error, retry }: ErrorBoundaryProps) {
  return (
    <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
      <Text>🛒</Text>
      <Text style={{ fontSize: 18, fontWeight: 'bold' }}>Sepet yüklenemedi</Text>
      <Text style={{ color: 'gray' }}>{error.message}</Text>
      <Button title="Tekrar Dene" onPress={retry} />
    </View>
  );
}

// Sayfa component'i
export default function CartScreen() {
  return (/* ... */);
}
```

### 4.2 Hata Akışı

```
Component Hatası
      │
      ▼
┌─ Aynı Route'un ErrorBoundary'si ─┐
│  Varsa → Yakala                  │
│  Yoksa → Yukarı çık              │
└──────────────────────────────────┘
      │
      ▼
┌─ Parent Layout ErrorBoundary ─┐
│  Varsa → Yakala               │
│  Yoksa → Yukarı çık           │
└───────────────────────────────┘
      │
      ▼
┌─ Root _layout.tsx ErrorBoundary ─┐
│  Son çare                        │
└──────────────────────────────────┘
```

### 4.3 Factory Pattern (Temiz Kod İçin)

```tsx
// components/createPageErrorBoundary.tsx
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { type ErrorBoundaryProps } from 'expo-router';

interface Config {
  icon: string;
  title: string;
  retryText?: string;
}

export function createPageErrorBoundary(config: Config) {
  return function ErrorBoundary({ error, retry }: ErrorBoundaryProps) {
    return (
      <View style={styles.container}>
        <Text style={styles.icon}>{config.icon}</Text>
        <Text style={styles.title}>{config.title}</Text>
        <Text style={styles.message}>{error.message}</Text>
        <TouchableOpacity style={styles.button} onPress={retry}>
          <Text style={styles.buttonText}>{config.retryText || 'Tekrar Dene'}</Text>
        </TouchableOpacity>
      </View>
    );
  };
}

const styles = StyleSheet.create({
  container: { 
    flex: 1, 
    justifyContent: 'center', 
    alignItems: 'center', 
    padding: 20,
    backgroundColor: '#FEF2F2' 
  },
  icon: { fontSize: 64 },
  title: { fontSize: 20, fontWeight: 'bold', marginTop: 16, color: '#991B1B' },
  message: { color: '#7F1D1D', marginTop: 8, textAlign: 'center' },
  button: { 
    marginTop: 20, 
    paddingHorizontal: 24, 
    paddingVertical: 12, 
    backgroundColor: '#DC2626', 
    borderRadius: 8 
  },
  buttonText: { color: 'white', fontWeight: '600' },
});
```

**Kullanım:**
```tsx
// app/(tabs)/cart.tsx
import { createPageErrorBoundary } from '@/components/createPageErrorBoundary';

export const ErrorBoundary = createPageErrorBoundary({
  icon: '🛒',
  title: 'Sepet yüklenemedi',
  retryText: 'Yeniden Yükle'
});

export default function CartScreen() { /* ... */ }
```

### 4.4 Global ErrorBoundary (_layout.tsx)

```tsx
// app/_layout.tsx
import { Stack } from 'expo-router';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { type ErrorBoundaryProps } from 'expo-router';
import * as Sentry from '@sentry/react-native'; // Opsiyonel

export function ErrorBoundary({ error, retry }: ErrorBoundaryProps) {
  // Production'da hata raporla
  if (!__DEV__) {
    Sentry.captureException(error);
  }

  return (
    <View style={styles.container}>
      <Text style={styles.icon}>😵</Text>
      <Text style={styles.title}>Bir şeyler ters gitti</Text>
      <Text style={styles.message}>{error.message}</Text>
      
      <TouchableOpacity style={styles.primaryButton} onPress={retry}>
        <Text style={styles.primaryButtonText}>Tekrar Dene</Text>
      </TouchableOpacity>
      
      {__DEV__ && (
        <View style={styles.devInfo}>
          <Text style={styles.devTitle}>Debug Info:</Text>
          <Text style={styles.devStack}>{error.stack}</Text>
        </View>
      )}
    </View>
  );
}

export default function RootLayout() {
  return <Stack />;
}

const styles = StyleSheet.create({
  container: { 
    flex: 1, 
    justifyContent: 'center', 
    alignItems: 'center', 
    padding: 20,
    backgroundColor: '#fff' 
  },
  icon: { fontSize: 80 },
  title: { fontSize: 24, fontWeight: 'bold', marginTop: 20 },
  message: { color: 'gray', marginTop: 10, textAlign: 'center' },
  primaryButton: { 
    marginTop: 30, 
    paddingHorizontal: 32, 
    paddingVertical: 16, 
    backgroundColor: '#3B82F6', 
    borderRadius: 12 
  },
  primaryButtonText: { color: 'white', fontWeight: 'bold', fontSize: 16 },
  devInfo: { marginTop: 30, padding: 16, backgroundColor: '#FEE2E2', borderRadius: 8 },
  devTitle: { fontWeight: 'bold', color: '#991B1B' },
  devStack: { fontSize: 10, color: '#7F1D1D', marginTop: 8 },
});
```

---

## 5. react-error-boundary Kütüphanesi

Expo Router'ın ErrorBoundary'si sadece **route seviyesinde** çalışır. Component bazında izolasyon için `react-error-boundary` kullan:

### Kurulum:
```bash
npm install react-error-boundary
# veya
yarn add react-error-boundary
```

### 5.1 Temel Kullanım:

```tsx
import { ErrorBoundary } from 'react-error-boundary';

function ErrorFallback({ error, resetErrorBoundary }) {
  return (
    <View style={styles.fallback}>
      <Text>⚠️ Hata oluştu</Text>
      <Text>{error.message}</Text>
      <Button title="Tekrar Dene" onPress={resetErrorBoundary} />
    </View>
  );
}

export default function CartScreen() {
  return (
    <ScrollView>
      {/* Her component izole */}
      <ErrorBoundary FallbackComponent={ErrorFallback}>
        <CartItems />
      </ErrorBoundary>

      <ErrorBoundary FallbackComponent={ErrorFallback}>
        <CartSummary />
      </ErrorBoundary>

      <ErrorBoundary FallbackComponent={ErrorFallback}>
        <RecommendedProducts />
      </ErrorBoundary>
    </ScrollView>
  );
}
```

### 5.2 Fallback Seçenekleri:

```tsx
// 1. Static fallback (basit)
<ErrorBoundary fallback={<Text>Hata oluştu</Text>}>
  <MyComponent />
</ErrorBoundary>

// 2. FallbackComponent (component)
<ErrorBoundary FallbackComponent={ErrorFallback}>
  <MyComponent />
</ErrorBoundary>

// 3. fallbackRender (render prop - en esnek)
<ErrorBoundary
  fallbackRender={({ error, resetErrorBoundary }) => (
    <View>
      <Text>{error.message}</Text>
      <Button title="Retry" onPress={resetErrorBoundary} />
    </View>
  )}
>
  <MyComponent />
</ErrorBoundary>
```

### 5.3 Reset Mekanizması:

```tsx
<ErrorBoundary
  FallbackComponent={ErrorFallback}
  onReset={() => {
    // State'i sıfırla
    queryClient.clear();
  }}
  resetKeys={[userId]} // Bu değişince otomatik reset
>
  <UserProfile userId={userId} />
</ErrorBoundary>
```

### 5.4 Error Logging:

```tsx
<ErrorBoundary
  FallbackComponent={ErrorFallback}
  onError={(error, info) => {
    // Sentry'ye gönder
    Sentry.captureException(error, {
      extra: { componentStack: info.componentStack }
    });
    
    // Analytics'e kaydet
    analytics.track('component_error', {
      error: error.message,
      component: info.componentStack
    });
  }}
>
  <MyComponent />
</ErrorBoundary>
```

---

## 6. Async Error Handling

Error Boundary async hataları **yakalayamaz**. Çözüm: `useErrorBoundary` hook

### 6.1 useErrorBoundary Hook:

```tsx
import { useErrorBoundary } from 'react-error-boundary';

function UserProfile({ userId }) {
  const { showBoundary } = useErrorBoundary();
  const [user, setUser] = useState(null);

  const fetchUser = async () => {
    try {
      const response = await fetch(`/api/users/${userId}`);
      if (!response.ok) throw new Error('Kullanıcı bulunamadı');
      const data = await response.json();
      setUser(data);
    } catch (error) {
      // ✅ Hatayı ErrorBoundary'ye ilet
      showBoundary(error);
    }
  };

  useEffect(() => {
    fetchUser();
  }, [userId]);

  return user ? <Text>{user.name}</Text> : <ActivityIndicator />;
}

// Kullanım
<ErrorBoundary FallbackComponent={ErrorFallback}>
  <UserProfile userId={123} />
</ErrorBoundary>
```

### 6.2 Event Handler'larda Kullanım:

```tsx
function SubmitButton() {
  const { showBoundary } = useErrorBoundary();

  const handleSubmit = async () => {
    try {
      await submitForm();
    } catch (error) {
      showBoundary(error); // ErrorBoundary'ye yönlendir
    }
  };

  return <Button title="Gönder" onPress={handleSubmit} />;
}
```

### 6.3 Hook'tan Throw Pattern:

```tsx
function UserProfile({ userId }) {
  const { data, error, isLoading } = useQuery(['user', userId], fetchUser);

  // ✅ Rendering sırasında throw et - ErrorBoundary yakalar
  if (error) throw error;

  if (isLoading) return <ActivityIndicator />;
  
  return <Text>{data.name}</Text>;
}
```

---

## 7. Error Logging & Monitoring

### 7.1 Sentry Entegrasyonu (Önerilen):

```bash
npx expo install @sentry/react-native
```

```tsx
// app/_layout.tsx
import * as Sentry from '@sentry/react-native';

Sentry.init({
  dsn: 'https://your-dsn@sentry.io/project-id',
  debug: __DEV__,
  tracesSampleRate: 1.0,
  environment: __DEV__ ? 'development' : 'production',
});

// Sentry'nin kendi ErrorBoundary'sini kullan
export default function RootLayout() {
  return (
    <Sentry.ErrorBoundary
      fallback={({ error, resetError }) => (
        <GlobalErrorFallback error={error} retry={resetError} />
      )}
      onError={(error, componentStack) => {
        console.error('Boundary caught:', error);
      }}
      beforeCapture={(scope) => {
        scope.setTag('location', 'root');
      }}
    >
      <Stack />
    </Sentry.ErrorBoundary>
  );
}
```

### 7.2 Multiple Boundary'lerde Tagging:

```tsx
// Her boundary'ye unique tag ekle
<Sentry.ErrorBoundary
  beforeCapture={(scope) => {
    scope.setTag('boundary', 'cart-items');
    scope.setTag('feature', 'shopping');
  }}
>
  <CartItems />
</Sentry.ErrorBoundary>

<Sentry.ErrorBoundary
  beforeCapture={(scope) => {
    scope.setTag('boundary', 'recommendations');
    scope.setTag('feature', 'shopping');
  }}
>
  <RecommendedProducts />
</Sentry.ErrorBoundary>
```

### 7.3 Custom Error Logging:

```tsx
// utils/errorLogger.ts
import * as Sentry from '@sentry/react-native';

export const logError = (error: Error, context?: Record<string, any>) => {
  // Development'ta console'a yaz
  if (__DEV__) {
    console.error('Error:', error);
    console.error('Context:', context);
    return;
  }

  // Production'da Sentry'ye gönder
  Sentry.captureException(error, {
    extra: context,
    tags: {
      errorType: error.name,
      timestamp: new Date().toISOString(),
    },
  });
};

// Kullanım
<ErrorBoundary
  onError={(error, info) => {
    logError(error, { componentStack: info.componentStack });
  }}
>
  <MyComponent />
</ErrorBoundary>
```

### 7.4 User Feedback (Sentry):

```tsx
<Sentry.ErrorBoundary
  showDialog // Kullanıcıya feedback dialog göster
  dialogOptions={{
    title: 'Bir hata oluştu',
    subtitle: 'Ekibimiz bilgilendirildi.',
    subtitle2: 'Yardımcı olmak isterseniz ne yaptığınızı anlatın.',
    labelName: 'İsim',
    labelEmail: 'Email',
    labelComments: 'Ne oldu?',
    labelSubmit: 'Gönder',
  }}
>
  <App />
</Sentry.ErrorBoundary>
```

---

## 8. Anti-Patterns - Kaçınılması Gerekenler

### ❌ Anti-Pattern 1: Tek Global Boundary

```tsx
// ❌ KÖTÜ - Tüm app tek boundary'de
export default function App() {
  return (
    <ErrorBoundary fallback={<ErrorScreen />}>
      <Navigation />
    </ErrorBoundary>
  );
}
// Sorun: Herhangi bir hata tüm app'i etkiler
```

```tsx
// ✅ İYİ - Katmanlı yapı
export default function App() {
  return (
    <ErrorBoundary fallback={<CriticalErrorScreen />}>
      <Navigation>
        <ErrorBoundary fallback={<PageErrorScreen />}>
          <HomePage />
        </ErrorBoundary>
      </Navigation>
    </ErrorBoundary>
  );
}
```

### ❌ Anti-Pattern 2: Async Hataları Boundary ile Yakalamaya Çalışmak

```tsx
// ❌ KÖTÜ - Async hatalar yakalanmaz
<ErrorBoundary>
  <UserList />
</ErrorBoundary>

function UserList() {
  useEffect(() => {
    fetch('/api/users').catch(console.error); // Boundary YAKALAMAZ
  }, []);
}
```

```tsx
// ✅ İYİ - useErrorBoundary kullan
function UserList() {
  const { showBoundary } = useErrorBoundary();
  
  useEffect(() => {
    fetch('/api/users').catch(showBoundary);
  }, []);
}
```

### ❌ Anti-Pattern 3: Aşırı Granüler Boundary

```tsx
// ❌ KÖTÜ - Her küçük component için boundary
<ErrorBoundary><Button /></ErrorBoundary>
<ErrorBoundary><Text /></ErrorBoundary>
<ErrorBoundary><Image /></ErrorBoundary>
// Sorun: Kod karmaşıklığı, performans
```

```tsx
// ✅ İYİ - Mantıksal gruplar için boundary
<ErrorBoundary>
  <ProductCard>
    <Image />
    <Text />
    <Button />
  </ProductCard>
</ErrorBoundary>
```

### ❌ Anti-Pattern 4: Hataları Yutmak

```tsx
// ❌ KÖTÜ - Hata loglanmıyor
<ErrorBoundary fallback={<Text>Hata</Text>}>
  <MyComponent />
</ErrorBoundary>
```

```tsx
// ✅ İYİ - Hata loglanıyor
<ErrorBoundary
  fallback={<Text>Hata</Text>}
  onError={(error, info) => {
    Sentry.captureException(error);
    analytics.track('error', { message: error.message });
  }}
>
  <MyComponent />
</ErrorBoundary>
```

### ❌ Anti-Pattern 5: Retry Olmadan Fallback

```tsx
// ❌ KÖTÜ - Kullanıcı çıkmaza girer
<ErrorBoundary fallback={<Text>Hata oluştu</Text>}>
  <MyComponent />
</ErrorBoundary>
```

```tsx
// ✅ İYİ - Recovery seçenekleri sun
<ErrorBoundary
  fallbackRender={({ error, resetErrorBoundary }) => (
    <View>
      <Text>Hata: {error.message}</Text>
      <Button title="Tekrar Dene" onPress={resetErrorBoundary} />
      <Button title="Ana Sayfaya Git" onPress={() => router.replace('/')} />
    </View>
  )}
>
  <MyComponent />
</ErrorBoundary>
```

---

## 9. Özet Checklist

### ✅ Yapılması Gerekenler:

| # | Aksiyon | Öncelik |
|---|---------|---------|
| 1 | Global ErrorBoundary ekle (`_layout.tsx`) | 🔴 Kritik |
| 2 | Kritik sayfalara Page-level boundary ekle | 🟠 Yüksek |
| 3 | Bağımsız widget'lara Component-level boundary ekle | 🟡 Orta |
| 4 | Sentry/Crashlytics entegrasyonu yap | 🔴 Kritik |
| 5 | Async hatalar için `useErrorBoundary` kullan | 🟠 Yüksek |
| 6 | Her boundary'de retry mekanizması sun | 🟠 Yüksek |
| 7 | Development'ta detaylı hata bilgisi göster | 🟡 Orta |
| 8 | Production'da kullanıcı dostu mesajlar göster | 🟠 Yüksek |
| 9 | `resetKeys` ile otomatik recovery sağla | 🟢 Düşük |
| 10 | Event handler'larda try-catch kullan | 🟠 Yüksek |

### Dosya Yapısı:

```
app/
├── _layout.tsx              # 🌍 Global ErrorBoundary + Sentry
├── (tabs)/
│   ├── _layout.tsx          # 📁 Tabs layout
│   ├── index.tsx            # Ana sayfa (boundary opsiyonel)
│   ├── cart.tsx             # 🛡️ Page ErrorBoundary + Component boundaries
│   └── profile.tsx          # 🛡️ Page ErrorBoundary
└── product/
    └── [id].tsx             # 🛡️ Page ErrorBoundary

components/
├── errors/
│   ├── createPageErrorBoundary.tsx    # Factory function
│   ├── ComponentErrorFallback.tsx     # Component-level fallback
│   └── GlobalErrorFallback.tsx        # Global fallback
└── ...

utils/
└── errorLogger.ts           # Sentry wrapper
```

### Karar Ağacı:

```
Hata türü nedir?
│
├─ Rendering hatası?
│  └─ ErrorBoundary kullan
│
├─ Event handler hatası?
│  └─ try-catch + useErrorBoundary
│
├─ Async/Network hatası?
│  └─ try-catch + useErrorBoundary
│
└─ Native crash?
   └─ Sentry Native SDK
```

---

## Sonuç

**En İyi Strateji = Katmanlı Yaklaşım:**

1. **Global Boundary** → Son çare, tüm app'i korur
2. **Page Boundary** → Sayfa seviyesi izolasyon
3. **Component Boundary** → Widget seviyesi izolasyon (react-error-boundary)
4. **Error Monitoring** → Sentry ile real-time tracking
5. **Recovery** → Her zaman retry/navigation seçeneği sun

Bu yapı sayesinde:
- Tek bir hata tüm app'i çökertmez ✅
- Kullanıcı bilgilendirilir ✅
- Hatalar loglanır ve izlenir ✅
- Recovery mümkün olur ✅