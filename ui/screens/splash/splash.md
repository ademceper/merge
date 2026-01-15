# 📱 MERGE - SPLASH SCREEN DETAYLI ANALİZ

## 1. **TEMEL ÖZELLİKLER & TASARIM PRENSİPLERİ**

### **1.1. Splash Screen'in Amacı**

**Ana Fonksiyonlar:**
- **Yükleme Maskeleme**: Backend API çağrıları, veri yükleme süresini görsel olarak kapatma
- **Marka Tanıtımı**: İlk temas noktası, marka kimliği oluşturma
- **Profesyonellik**: Kaliteli bir ürün izlenimi verme
- **Geçiş Yönetimi**: Boş ekrandan ana ekrana yumuşak geçiş
- **Psikolojik Hazırlık**: Kullanıcıyı uygulamaya mental olarak hazırlama

**Merge İçin Özel Değer:**
- Hibrit platform kimliğini yansıtma (pazaryeri + kendi site)
- Technical credibility (developer tools connection)
- Premium positioning (B2B + B2C)
- Hız vurgusu (merge = efficiency)

### **1.2. Minimalist Tasarım DNA'sı**

**Apple Prensipleri:**
- Clarity (Netlik): Her element açık ve anlaşılır
- Deference (Saygı): İçerik öne çıkar, UI geri planda
- Depth (Derinlik): Subtle shadow/layering (çok minimal)
- Consistency (Tutarlılık): Tüm ekranlarla uyumlu

**Vercel Prensipleri:**
- Speed (Hız): Anında yükleme hissi
- Precision (Kesinlik): Pixel-perfect alignment
- Simplicity (Basitlik): Gereksiz hiçbir element yok
- Technical (Teknik): Developer-friendly aesthetic

**Merge'e Uyarlama:**
- Monochrome palette (siyah-beyaz)
- Geometric forms (keskin, mathematical)
- Negative space dominance (boşluk = tasarım)
- No gradients, no shadows, no depth (flat)
- Typography as hero (yazı tipi kimlik)

---

## 2. **EKRAN DÜZENİ & LAYOUT DETAYLARI**

### **2.1. Light Mode Splash (Beyaz Arka Plan)**

**Arka Plan:**
```
Renk: #FFFFFF (Pure white)
Texture: Yok (solid color)
Gradient: Yok
Pattern: Yok
```

**Layout Grid:**
```
Ekran Yüksekliği: 812dp (iPhone 13 Pro standard)
Ekran Genişliği: 375dp

Bölümleme:
├─ Safe Area Top: 44dp (notch/status bar)
├─ Upper Section: 200dp (boşluk)
├─ Logo Zone: 100dp (merkez element)
├─ Name Zone: 60dp (app name)
├─ Middle Space: 180dp (critical whitespace)
├─ Loading Zone: 40dp (indicator)
├─ Lower Space: 140dp (boşluk)
├─ Version Zone: 24dp (footer)
└─ Safe Area Bottom: 24dp (home indicator)
```

**Element Pozisyonları (Matematiksel):**

**Logo:**
- X: 187.5dp (center horizontal)
- Y: 306dp (center vertical - slight offset)
- Anchor: Center-center
- Size: 100x100dp
- Margin: 48dp her yandan (minimum touch zone)

**App Name (MERGE):**
- X: 187.5dp (center)
- Y: 426dp (logo altı + 20dp)
- Font Size: 22sp
- Line Height: 26sp
- Alignment: Center

**Loading Indicator:**
- X: 187.5dp (center)
- Y: 626dp
- Size: 24x2dp (line) veya 8dp (dot)
- Animation space: 40dp height

**Version:**
- X: 187.5dp (center) veya 343dp (right aligned)
- Y: 764dp (bottom - 24dp)
- Font Size: 10sp
- Alignment: Center veya Right

### **2.2. Dark Mode Splash (Siyah Arka Plan)**

**Arka Plan:**
```
Renk: #000000 (True black - OLED optimized)
Texture: Yok
Anti-aliasing: Edges crisp
```

**Renk Değişimleri:**
```
Logo: #FFFFFF (beyaz)
App Name: #FFFFFF
Loading Indicator Active: #FFFFFF
Loading Indicator Inactive: #333333
Version: #666666 (opacity: 0.4)
```

**OLED Optimizasyonu:**
- True black (#000000) = pixel off
- Pil tasarrufu: %30-40 (OLED ekranlarda)
- Kontrast oranı: ∞:1 (pure black vs white)
- Göz yorgunluğu: Karanlıkta daha az
- Burn-in riski: Minimal (static logo 1-2 saniye)

### **2.3. Off-White Varyant (Soft Light)**

**Arka Plan:**
```
Renk: #FAFAFA veya #F5F5F5
Tone: Warm white (çok hafif krem)
Kullanım: Uzun splash (>2 saniye) durumlarında
Avantaj: Göz yormaz, LCD'de daha doğal
```

**Renk Ayarlamaları:**
```
Logo: #000000 (değişmez)
App Name: #1A1A1A (biraz yumuşak)
Loading: #999999
Version: #CCCCCC
```

---

## 3. **TİPOGRAFİ DETAYLARI**

### **3.1. App Name Typography (MERGE)**

**Font Seçimi:**
```
Primary: Inter Semibold (600)
Fallback: SF Pro Display Semibold
Web Fallback: -apple-system, system-ui, sans-serif

Neden Inter?
- Vercel'in resmi fontu
- Açık kaynak (ücretsiz)
- Geometric, modern
- Excellent legibility
- Variable font support
```

**Font Özellikleri:**
```
Size: 22sp (mobil), 28sp (tablet)
Weight: 600 (Semibold)
Line Height: 26sp (1.18 ratio - tight)
Letter Spacing: -0.01em (-0.22dp)
Case: Uppercase (MERGE)
Color: #000000 (light) / #FFFFFF (dark)
Rendering: Subpixel antialiasing
```

**Neden Uppercase?**
- Güçlü, autoritatif görünüm
- Geometric symmetry
- Vercel tarzı (logo VERCEL yazıyor)
- Technical credibility
- Minimal, compact

**Kerning (Harf Arası Mesafe):**
```
M-E: -0.01em (default)
E-R: -0.01em
R-G: -0.01em
G-E: -0.01em

Toplam width: ~88dp (22sp font'ta)
Optical balance: E'lerin çubuğu hizalı
```

### **3.2. Version Typography**

**Font Özellikleri:**
```
Font: Inter Regular (400)
Size: 10sp
Weight: 400 (Regular)
Letter Spacing: 0 (normal)
Line Height: 12sp
Color Light: #CCCCCC
Color Dark: #333333
Additional Opacity: 0.5 (final: ~50% görünür)
```

**Format Seçenekleri:**
```
Minimal: "v1.0.0"
Standard: "Version 1.0.0"
Technical: "v1.0.0-beta"
Date: "v1.0.0 (2025.01)"

Önerilen: "v1.0.0" (en minimal)
```

**Pozisyon:**
```
Alignment: Center (recommended) veya Right
Bottom Margin: 24dp (home indicator'dan uzak)
Side Margin: 24dp (eğer right aligned)
```

### **3.3. Typography Best Practices**

**Font Loading:**
- System font kullan (Inter varsa)
- Fallback'ler hazır olsun
- FOUT (Flash of Unstyled Text) olmasın
- Splash'te custom font yükleme riski yok (basit text)

**Rendering Quality:**
- Subpixel antialiasing: Açık
- Hinting: Auto
- Text rendering: OptimizeLegibility
- Font smoothing: Antialiased

**Accessibility:**
- Minimum contrast ratio: 7:1 (AAA standard)
- #000000 on #FFFFFF: 21:1 ✓
- #FFFFFF on #000000: 21:1 ✓
- Font size: 22sp > 18sp minimum ✓

---

## 4. **ANİMASYON STRATEJİSİ**

### **4.1. Entry Animation (Logo Giriş)**

**Önerilen: Fade + Micro Scale**

**Timeline:**
```
0ms:
- Background: #FFFFFF (anında görünür)
- Logo: opacity 0, scale 0.98
- App name: opacity 0
- Loading: gizli

100ms:
- Background fade tamamlandı

100-500ms:
- Logo animate başlıyor
  - Opacity: 0 → 1
  - Scale: 0.98 → 1.0
  - Easing: cubic-bezier(0.25, 0.1, 0.25, 1)

500ms:
- Logo tam görünür

550-750ms:
- App name animate
  - Opacity: 0 → 1
  - TranslateY: +10dp → 0
  - Easing: ease-out

750ms:
- Tüm statik elementler görünür

800ms:
- Loading indicator başlıyor (döngüsel)
```

**Animasyon Parametreleri:**
```
Logo Scale Range: 0.98 - 1.0 (çok subtle)
Neden 0.98?: Daha küçük değerler fark edilir, bu "hint of motion"
Opacity Curve: Linear değil, ease-out
Duration: 400ms (Apple standard)
Delay: 100ms (arka plan yerleştikten sonra)
```

**Alternatif: Fade Only (Ultra Minimal)**
```
0-300ms:
- Opacity: 0 → 1
- Easing: ease-out
- No scale, no translate
- Vercel'in kullandığı yöntem
```

### **4.2. Loading Indicator Animation**

**Tip 1: Horizontal Line Sweep**

**Görünüm:**
```
Frame 1: ▁▁▁▁▁▁▁▁ (gri, inactive)
Frame 2: ▂▁▁▁▁▁▁▁ (siyah active kısmı sağa kayıyor)
Frame 3: ▁▂▁▁▁▁▁▁
Frame 4: ▁▁▂▁▁▁▁▁
Frame 5: ▁▁▁▂▁▁▁▁
Frame 6: ▁▁▁▁▂▁▁▁
Frame 7: ▁▁▁▁▁▂▁▁
Frame 8: ▁▁▁▁▁▁▂▁
Frame 9: ▁▁▁▁▁▁▁▂
Frame 10: ▁▁▁▁▁▁▁▁ (loop başa dön)
```

**Teknik Detaylar:**
```
Total Width: 24dp
Segment Count: 8 segments (3dp each)
Active Segment: 1 segment (3dp)
Active Color: #000000 (light) / #FFFFFF (dark)
Inactive Color: #E0E0E0 (light) / #333333 (dark)
Animation Duration: 1200ms (1 cycle)
Easing: Linear (sürekli hareket)
Direction: Left to right
Loop: Infinite
```

**Tip 2: Three Dots Pulse**

**Görünüm:**
```
● ○ ○  (dot 1 büyük)
○ ● ○  (dot 2 büyük)
○ ○ ●  (dot 3 büyük)
● ○ ○  (loop)
```

**Teknik Detaylar:**
```
Dot Size: 8dp diameter
Spacing: 12dp apart (center to center)
Total Width: 32dp
Active Scale: 1.0
Inactive Scale: 0.6
Active Opacity: 1.0
Inactive Opacity: 0.3
Duration: 900ms per cycle
Easing: ease-in-out
```

**Tip 3: Single Dot Fade (Minimalist)**

**Görünüm:**
```
●  (fade in/out, tek nokta)
```

**Teknik Detaylar:**
```
Size: 8dp
Opacity: 0.3 ↔ 1.0 (breathing)
Duration: 1500ms (yavaş)
Easing: ease-in-out
Loop: Infinite
Color: #000000 (light) / #FFFFFF (dark)
```

### **4.3. Exit Animation (Ana Ekrana Geçiş)**

**Cross Fade (Önerilen):**
```
Splash Screen:
- 0ms: Tam görünür (opacity 1)
- 0-300ms: Opacity 1 → 0
- Easing: ease-in

Home Screen:
- 0ms: Opacity 0 (arkada hazır)
- 150ms: Opacity 0 → 1 başlıyor (overlap)
- 150-400ms: Opacity 0 → 1
- Easing: ease-out

Total transition: 400ms
Overlap period: 150ms (smooth)
```

**Slide Up (Alternatif):**
```
Splash Screen:
- 0-300ms: TranslateY 0 → -100%
- Opacity: 1 → 0 (simultaneously)

Home Screen:
- 0-400ms: TranslateY 100% → 0
- Opacity: 1 (sabit)

Total transition: 400ms
Feel: Daha dinamik, iOS tarzı
```

---

## 5. **SÜRE YÖNETİMİ & YÜKLEME MANTĞI**

### **5.1. Süre Stratejisi**

**Minimum Gösterim Süresi:**
```
Duration: 1200ms (1.2 saniye)

Breakdown:
- 0-400ms: Logo entry animation
- 400-800ms: Statik gösterim (branding)
- 800-1200ms: Loading indicator (feedback)
- 1200ms+: Veri hazırsa exit, değilse devam

Neden 1200ms?
- 1000ms altı: Çok hızlı, logo fark edilmez
- 1500ms üstü: Uzun hissedilmeye başlar
- 1200ms: Optimal branding + hız dengesi
```

**Maksimum Gösterim Süresi:**
```
Duration: 3000ms (3 saniye)

Sebep: Kullanıcı sabır limiti
3 saniye sonra: Timeout uyarısı veya retry

Exception: İlk açılış (cold start)
- 5 saniye'ye kadar acceptable
- Asset download, cache setup
```

### **5.2. Backend Yükleme Orchestration**

**Paralel İşlemler:**
```
Splash görünür olur olmaz (0ms):

Paralel başlatılanlar:
├─ Auth Token Check (200ms)
├─ User Profile Fetch (400ms)
├─ Categories API (300ms)
├─ Config/Settings (150ms)
└─ Firebase Initialize (250ms)

En yavaşı: 400ms (User Profile)

Promise.all([...]) ile hepsi biter
Total: 400ms (en uzun süren)

Ama splash minimum 1200ms gösterilir
Fark: 800ms (branding time)
```

**Conditional Loading:**
```
IF (user logged in):
  - Load user data
  - Load favorites, cart
  - Total: ~600ms

ELSE (guest):
  - Only categories, config
  - Total: ~300ms
  - Splash 1200ms'de tamamlanır (fast)

Strategi: Guest'ler daha hızlı başlar
```

### **5.3. Yavaş Yükleme Senaryosu**

**1.5-3 saniye arası:**
```
Loading indicator devam eder
UI değişmez (sakin kal)
Kullanıcı: "Yükleniyor" anlıyor
```

**3+ saniye:**
```
Timeline:
3000ms: Loading indicator altına text ekle

Ekran:
┌─────────────────────────┐
│         LOGO            │
│         MERGE           │
│           ─             │ ← Loading
│                         │
│    Bağlanıyor...        │ ← Yeni text
└─────────────────────────┘

Font: Inter Regular, 12sp
Color: #999999
Fade in: 200ms
```

**5+ saniye:**
```
Timeline:
5000ms: Retry button veya hata mesajı

Ekran:
┌─────────────────────────┐
│         LOGO            │
│         MERGE           │
│                         │
│  ⚠️ Bağlantı sorunu     │
│                         │
│  [Tekrar Dene]          │ ← Button
└─────────────────────────┘

Button style: Minimal, outline
```

---

## 6. **TEKNİK İMPLEMENTASYON DETAYLARI**

### **6.1. Platform Özellikleri**

**iOS Implementation:**
```
Sistem Splash:
- LaunchScreen.storyboard (zorunlu)
- Statik görsel (animasyon yok)
- Sistem tarafından gösterilir (300-500ms)

Custom Splash:
- ViewController (SwiftUI/UIKit)
- Animasyonlar burada
- Süre kontrolü burada
- 1200ms minimum enforcement
```

**Android Implementation:**
```
Sistem Splash:
- Android 12+ (API 31): SplashScreen API
- windowSplashScreenAnimatedIcon
- windowSplashScreenBackground
- 1000ms otomatik

Custom Splash:
- SplashActivity
- ViewModel ile data load
- setKeepOnScreenCondition
- Animasyonlar burada
```

**React Native Implementation:**
```
Package: react-native-splash-screen

Setup:
- Native splash (iOS/Android)
- JS splash (custom)
- Hide method: SplashScreen.hide()

Timing:
useEffect(() => {
  const timer = setTimeout(() => {
    if (dataLoaded) {
      SplashScreen.hide();
    }
  }, 1200);
}, [dataLoaded]);
```

### **6.2. Asset Optimization**

**Logo Asset Formats:**

**iOS:**
```
Assets.xcassets/
  merge-logo.imageset/
    merge-logo.pdf (vector, preferred)
    veya
    merge-logo@1x.png (100x100px)
    merge-logo@2x.png (200x200px)
    merge-logo@3x.png (300x300px)

Format: PNG-24 (alpha channel)
Compression: Lossless
Size: @3x ~15KB max
```

**Android:**
```
res/
  drawable-mdpi/merge_logo.png     (100x100px)
  drawable-hdpi/merge_logo.png     (150x150px)
  drawable-xhdpi/merge_logo.png    (200x200px)
  drawable-xxhdpi/merge_logo.png   (300x300px)
  drawable-xxxhdpi/merge_logo.png  (400x400px)

veya (Önerilen):
drawable/merge_logo.xml (vector XML)

Vector avantajları:
- Tek dosya
- Sonsuz ölçekleme
- ~2KB boyut
```

**Web (PWA):**
```
SVG format (vector)
- Scalable
- CSS ile renkendirilebilir (dark mode)
- Küçük dosya boyutu (~3KB)

Fallback PNG:
- 256x256px (Retina)
- WebP format (daha küçük)
```

### **6.3. Performance Optimization**

**Memory Management:**
```
Splash aktifken:
- Sadece critical assets yükle
- Home screen render etme (gereksiz)
- Memory footprint: <30MB

Splash exit'te:
- Splash resources temizle
- Logo bitmap recycle (Android)
- View hierarchy clear
```

**CPU Usage:**
```
Splash animation:
- GPU kullan (transform, opacity)
- CPU kullanma (width, height, left, top)
- 60 FPS hedefle
- Frame time: 16ms altı

Monitoring:
- iOS: Instruments (Time Profiler)
- Android: Systrace
- React Native: Perf Monitor
```

**Battery Impact:**
```
OLED Dark Mode:
- True black (#000000) = pixel off
- Pil tasarrufu: ~35%
- 2 saniye splash = ihmal edilebilir etki

LCD Light Mode:
- Normal pil tüketimi
- Backlight always on
```

---

## 7. **ERİŞİLEBİLİRLİK (ACCESSIBILITY)**

### **7.1. Screen Reader Desteği**

**iOS VoiceOver:**
```
Splash container:
accessibilityLabel: "Merge uygulaması yükleniyor"
accessibilityHint: "Lütfen bekleyin"
isAccessibilityElement: true

Logo:
accessibilityLabel: "Merge logosu"
isAccessibilityElement: false (decorative)

App name:
accessibilityLabel: "Merge"
isAccessibilityElement: false (redundant)

Loading indicator:
accessibilityLabel: "Yükleniyor"
accessibilityTraits: .updatesFrequently
```

**Android TalkBack:**
```
Splash layout:
contentDescription: "Merge uygulaması yükleniyor"
importantForAccessibility: yes

Logo:
contentDescription: null (decorative)
importantForAccessibility: no

Loading:
contentDescription: "İçerik yükleniyor"
```

### **7.2. Reduced Motion (Hareket Azaltma)**

**Sistem Ayarı Kontrolü:**

**iOS:**
```
if UIAccessibility.isReduceMotionEnabled {
  // Animasyonları devre dışı bırak
  logoView.alpha = 1.0 // Fade yok
  // veya
  animationDuration = 0 // Anında göster
}
```

**Android:**
```
Settings.Global.getFloat(
  contentResolver,
  Settings.Global.ANIMATOR_DURATION_SCALE,
  1.0f
)

if (scale == 0) {
  // Animasyon yok
  view.alpha = 1f
}
```

**Davranış:**
- Reduce motion aktifse: Sadece fade (opacity)
- Scale, translate animasyonları iptal
- Veya tamamen instant göster
- Loading indicator: Statik (blinking değil)

### **7.3. Kontrast & Görünürlük**

**High Contrast Mode:**
```
iOS: UIAccessibility.isDarkerSystemColorsEnabled
Android: AccessibilityManager

Adjustment:
Normal: #000000 on #FFFFFF (21:1 contrast)
High Contrast: Aynı kalır (zaten maximum)

Eğer gri kullanılsaydı:
Normal: #666666 on #FFFFFF (5.7:1)
High Contrast: #333333 on #FFFFFF (12.6:1)
```

**Large Text Support:**
```
Sistem font büyütme:
iOS: Dynamic Type
Android: Font Scale

App Name:
- Base: 22sp
- Large: 22sp * 1.3 = 28sp
- Extra Large: 22sp * 1.5 = 33sp

Layout adjustment:
- Logo küçültebilir (100dp → 80dp)
- Vertical spacing ayarlanır
```

---

## 8. **DARK MODE GEÇIŞ DETAYLARI**

### **8.1. Sistem Dark Mode Algılama**

**iOS:**
```
Trait collection:
userInterfaceStyle: .light / .dark

Otomatik güncelleme:
override func traitCollectionDidChange() {
  updateColors()
}

SwiftUI:
@Environment(\.colorScheme) var colorScheme

if colorScheme == .dark {
  // Dark mode assets
}
```

**Android:**
```
Configuration:
uiMode & UI_MODE_NIGHT_MASK

Result:
UI_MODE_NIGHT_YES → Dark mode
UI_MODE_NIGHT_NO → Light mode

Automatic:
res/values/colors.xml (light)
res/values-night/colors.xml (dark)
```

### **8.2. Renk Geçiş Stratejisi**

**Instant Switch (Önerilen):**
```
Splash açılırken sistem modunu oku
Direkt doğru renkleri kullan
Geçiş animasyonu yok

Neden?
- Splash sadece 1-2 saniye
- Geçiş animasyonu gereksiz
- Daha hızlı
```

**Animated Switch (Alternatif):**
```
Eğer kullanıcı splash aktifken mod değiştirirse:
(Nadir senaryo)

Background color: Animate 300ms
Logo color: Animate 300ms
Text color: Animate 300ms

Easing: ease-in-out
```

### **8.3. OLED Optimizasyon (Dark Mode)**

**True Black Kullanımı:**
```
Color: #000000 (not #0A0A0A veya #111111)

Neden pure black?
- OLED'de pixel tamamen kapanır
- Pil tasarrufu maksimum
- Infinite contrast
- Premium görünüm

Risk:
- Burn-in (image retention)
- Splash 1-2 saniye, risk minimal
```

**Smearing Önleme:**
```
OLED smearing: Beyaz nesne siyah arka planda hareket edince iz
Logo animasyonu yavaşsa sorun yok
Hızlı animasyon (fast fade): Ghost image
Çözüm: Smooth easing curves (ease-out)
```

---

## 9. **TEST SENARYOLARI**

### **9.1. Fonksiyonel Testler**

**Görsel Testler:**
```
✓ Logo merkeze yerleşmiş mi?
✓ App name doğru font ve boyutta mı?
✓ Version number görünüyor mu?
✓ Loading indicator animasyon yapıyor mu?
✓ Renk kontrastı yeterli mi? (WCAG AAA)
✓ Safe area'lar doğru mu? (notch, home indicator)
```

**Animasyon Testler:**
```
✓ Entry animation smooth mu? (60 FPS)
✓ Loading indicator döngüsel çalışıyor mu?
✓ Exit animationı jarring değil mi?
✓ Animasyon süreleri doğru mu? (400ms vs 600ms)
✓ Reduce motion aktifse animasyon iptal mi?
```

**Süre Testleri:**
```
✓ Minimum 1200ms gösteriliyor mu?
✓ Maksimum 3000ms'de timeout mu?
✓ Yavaş network'te feedback var mı?
✓ İlk açılışta (cold start) beklenebilir mi?
```

### **9.2. Platform Testleri**

**iOS Cihazlar:**
```
✓ iPhone 8 (notch yok, küçük ekran)
✓ iPhone 13 Pro (standard notch)
✓ iPhone 14 Pro Max (dynamic island)
✓ iPad Mini (tablet boyut)
✓ iPad Pro 12.9" (çok büyük)

Her cihazda:
- Logo boyut uygun mu?
- Safe area'lar doğru mu?
- Font okunuyor mu?
```

**Android Cihazlar:**
```
✓ Small (5"): Logo küçülmeli mi?
✓ Medium (6"): Standard layout
✓ Large (6.7"+): Logo büyüyebilir mi?
✓ Foldables: Açık/kapalı state
✓ Tablet: Layout farklı mı?

Screen ratios:
- 16:9, 18:9, 19:9, 20:9, 21:9
- Her ratio'da test
```

**Orientation (Yönelim):**
```
✓ Portrait (dikey): Primary
✓ Landscape (yatay): Destekleniyor mu?

Öneri: Splash'i portrait lock
Sebep: Kısa süre, rotation gereksiz
```

### **9.3. Performance Testleri**

**FPS Monitoring:**
```
Target: 60 FPS (16.67ms per frame)

Test tools:
- iOS: Instruments > Core Animation FPS
- Android: GPU Profiling > On-screen bars
- React Native: Perf Monitor

Acceptable: 55+ FPS
Poor: <50 FPS (janky)
```

**Memory Profiling:**
```
Splash active:
- Memory: <30MB
- CPU: <15%

Splash → Home transition:
- Memory spike: <50MB
- Garbage collection: Minimal

Tools:
- iOS: Instruments > Allocations
- Android: Memory Profiler
```

**Battery Impact:**
```
Measure: 100 cold starts

Expected:
- Light mode: 0.05% battery drop
- Dark mode (OLED): 0.03% drop

Method:
- iOS: Xcode Energy Log
- Android: Battery Historian
```

---

## 10. **HATA DURUMLARI & FALLBACK'LER**

### **10.1. Asset Yüklenemedi**

**Senaryo: Logo PNG yüklenemedi**
```
Fallback:
1. Degrade to text-only (MERGE yazısı)
2. Sistem font ile göster
3. Veya basit shape (kare, daire)

Prevention:
- Logo'yu app bundle'a gömme (runtime fetch değil)
- Compile-time asset verification
```

### **10.2. Animasyon Crash**

**Senaryo: Animation engine fail**
```
Fallback:
1. Tüm animasyonları skip
2. Statik gösterim (1200ms)
3. Direct transition (no fade)

Prevention:
- Try-catch animation blocks
- Test on low-end devices
```

### **10.3. Timeout (Network Slow)**

**3 saniye sonrası:**
```
Display:
  ⚠️ Bağlantı yavaş
  [Tekrar Dene] [Devam Et]

Devam Et action:
- Home screen'e geç (data incomplete)
- Show error banners
- Retry in background
```

### **10.4. Crash on Splash**

**Senaryo: App splash'te crash oluyor**
```
Debug strategy:
1. Check log: Hangi component?
2. Disable animations (test)
3. Test on simulator vs device
4. Check memory constraints

Production:
- Crash reporting: Sentry, Firebase Crashlytics
- Quick patch release
- Temporary: Remove splash, direct home
```

---

## 11. **BEST PRACTICES & KAÇINILMASI GEREKENLER**

### **11.1. ✅ YAPILMASI GEREKENLER**

**Tasarım:**
- Pure siyah-beyaz kullan (gradyan yok)
- 8dp grid sistemine uy
- Safe area'lara dikkat et
- Dark mode'u destekle
- Pixel-perfect alignment

**Animasyon:**
- 60 FPS hedefle
- Subtle hareketler (over-animation yok)
- Reduce motion'ı destekle
- GPU-accelerated properties kullan
- Easing curves doğru seç

**Performans:**
- Minimum yükleme süresine uy (1.2s)
- Async data loading
- Memory efficient
- Battery optimized (OLED dark mode)

**Erişilebilirlik:**
- Screen reader labels
- High contrast support
- Large text support
- Keyboard navigation (web)

### **11.2. ❌ KAÇINILMASI GEREKENLER**

**Tasarım:**
- Gradient kullanma (flat kal)
- Drop shadow ekleme (depth yok)
- Çok element ekleme (minimal kal)
- Renkli accent kullanma (monochrome)
- Comic sans benzeri fontlar

**Animasyon:**
- 5+ saniye animasyon
- Karmaşık 3D efektler
- Bounce/Spring aşırı kullanım
- Çok hızlı animations (<200ms)
- Continuous rotation (dizzying)

**Teknik:**
- Video splash (dosya boyutu, performance)
- Network'ten logo fetch (risk)
- Heavy processing (splash'te ML model yükleme gibi)
- Custom font download (splash'te)
- Auto-play sound (annoying)

**UX:**
- Skip button (1-2 saniye splash gereksiz)
- Interaktif element (oyun vs, confusion)
- Reklam gösterme (ilk izlenim kötü)
- Forced delay (data hazırsa hemen geç)

---

## 12. **PLATFORM SPESİFİK DETAYLAR**

### **12.1. iOS Specific**

**Launch Screen Requirements:**
- LaunchScreen.storyboard zorunlu
- Animasyon yok (statik)
- App Store guideline uyumu

**Dynamic Island Handling:**
```
iPhone 14 Pro+:
- Status bar yükseklik: 54dp (was 44dp)
- Logo offset adjust: +10dp aşağı kaydır
- Veya Dynamic Island'ı ignore (center perfect merkez)
```

**Safe Area Insets:**
```
safeAreaLayoutGuide kullan
İstemde safe area padding
Top: 44dp (notch) / 54dp (island)
Bottom: 34dp (home indicator)
```

### **12.2. Android Specific**

**Material Design 3:**
- SplashScreen API (Android 12+)
- windowSplashScreenBackground
- windowSplashScreenAnimatedIcon
- Otomatik dark mode support

**Edge-to-Edge:**
```
Flags:
- LAYOUT_STABLE
- LAYOUT_FULLSCREEN

WindowInsets:
- systemBars() padding
- Status bar: 24dp
- Navigation bar: 48dp (3-button) / 0dp (gesture)
```

**Foldable Support:**
```
Unfolded:
- Ekran 7.6" (tablet mode)
- Logo 120dp (larger)

Folded:
- Ekran 6.2" (phone mode)
- Logo 100dp (standard)

Hinge area:
- Logo merkeze yerleştir (hinge'den uzak)
```

### **12.3. Web/PWA Specific**

**Initial Load:**
```
HTML: Inline critical CSS
<style>
  .splash {
    background: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
  }
</style>

Body:
<div class="splash">
  <svg>...</svg> <!-- Inline SVG logo -->
</div>
```

**Loading Strategy:**
```
1. Instant HTML splash
2. JS yüklenirken göster
3. React/Vue mount olunca
4. API calls tamamlansın
5. Splash fade out (300ms)
6. App render
```

---

## 13. **ANALİTİK & ÖLÇÜMLENDİRME**

### **13.1. Track Edilmesi Gereken Metrikler**

**Süre Metrikleri:**
```
splash_shown_duration:
- p50 (median): 1.2s
- p95: 2.5s
- p99: 3.8s

Hedef:
- p95 < 2.5s (kullanıcıların %95'i 2.5s'de geçiyor)
```

**Platform Dağılımı:**
```
splash_platform:
- iOS: 55%
- Android: 43%
- Web: 2%

splash_mode:
- Light: 65%
- Dark: 35%
```

**Performans:**
```
splash_fps_avg:
- Target: 60 FPS
- Acceptable: 55+ FPS
- Track: <50 FPS (poor performance devices)

splash_memory_peak:
- Average: 25MB
- High: >40MB (investigate)
```

**Hata Oranları:**
```
splash_error_rate:
- Network timeout: 2%
- Asset load fail: 0.1%
- Animation crash: 0.01%

Target: <1% total error
```

### **13.2. A/B Test Önerileri**

**Test 1: Logo Boyutu**
```
Variant A: 100x100dp
Variant B: 120x120dp
Metric: Brand recall, user satisfaction survey
```

**Test 2: Animasyon Tipi**
```
Variant A: Fade only
Variant B: Fade + scale
Metric: Perceived speed, smoothness rating
```

**Test 3: Minimum Süre**
```
Variant A: 1000ms
Variant B: 1200ms
Variant C: 1500ms
Metric: Brand impression, frustration rate
```

**Test 4: Loading Indicator**
```
Variant A: Horizontal line
Variant B: Three dots
Variant C: No indicator
Metric: User understanding, perceived wait time
```

---

Bu Merge splash ekranının tüm yönlerini kapsayan detaylı analiz. Başka bir ekranın detaylandırılmasını ister misin?