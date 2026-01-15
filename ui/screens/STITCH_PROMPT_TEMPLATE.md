# 🎨 STITCH PROMPT ŞABLONU - MERGE UI EKRAN TASARIMI

Bu dokümantasyon, Stitch AI ile ekran tasarımları oluştururken kullanılacak standart prompt şablonunu içerir. Her ekran için bu şablonu kullanarak tutarlı ve detaylı tasarımlar elde edebilirsiniz.

---

## 📋 PROMPT YAPISI

### 1. **EKRAN BİLGİLERİ & BAĞLAM**

```
Ekran Adı: [Ekranın tam adı]
Platform: [Mobil / Desktop / Her İkisi]
Bölüm: [Müşteri Tarafı - Pazaryeri / Satıcı Sitesi / Satıcı Paneli / Admin Paneli]
Kategori: [Onboarding / Ana Sayfa / Ürün / Sepet / Profil / vb.]
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
- [Hibrit platform özelliği]
- [B2B/B2C farkı]
- [Teknik özellik]
```

### 3. **TASARIM PRENSİPLERİ**

```
Tasarım DNA'sı:
- Minimalist: Monochrome palette (siyah-beyaz)
- Geometric: Keskin, matematiksel formlar
- Negative Space: Boşluk = tasarım
- Flat Design: Gradyan yok, shadow yok, depth yok
- Typography as Hero: Yazı tipi kimlik

Referans Stiller:
- Apple: Clarity, Deference, Depth (minimal), Consistency
- Vercel: Speed, Precision, Simplicity, Technical
- Merge Uyarlaması: [Özel notlar]
```

### 4. **EKRAN DÜZENİ & LAYOUT**

```
Platform: [Mobil / Desktop]

Ekran Boyutları:
- Mobil: 375x812dp (iPhone 13 Pro standard)
- Desktop: 1440x900px (standard desktop)

Layout Grid:
├─ Safe Area Top: [X]dp/px
├─ Header Zone: [Y]dp/px
├─ Content Zone: [Z]dp/px
├─ Action Zone: [W]dp/px
├─ Footer Zone: [V]dp/px
└─ Safe Area Bottom: [U]dp/px

Element Pozisyonları:
- [Element 1]: X: [pozisyon], Y: [pozisyon], Size: [boyut]
- [Element 2]: X: [pozisyon], Y: [pozisyon], Size: [boyut]
- [Element 3]: X: [pozisyon], Y: [pozisyon], Size: [boyut]

Spacing Sistemi:
- 8dp grid sistemi
- Padding: 16dp, 24dp, 32dp
- Gap: 8dp, 12dp, 16dp, 24dp
```

### 5. **RENK PALETİ**

```
Light Mode:
- Background: #FFFFFF (Pure white)
- Primary Text: #000000 (Pure black)
- Secondary Text: #666666
- Tertiary Text: #999999
- Border: #E5E5E5
- Accent: [Varsa belirt]
- Error: #DC2626
- Success: #16A34A
- Warning: #F59E0B

Dark Mode:
- Background: #000000 (True black - OLED optimized)
- Primary Text: #FFFFFF
- Secondary Text: #CCCCCC
- Tertiary Text: #999999
- Border: #333333
- Accent: [Varsa belirt]
- Error: #EF4444
- Success: #22C55E
- Warning: #FBBF24

Özel Durumlar:
- [Varsa özel renkler]
```

### 6. **TİPOGRAFİ**

```
Başlık (H1):
- Font: Inter Semibold (600)
- Size: 28sp (mobil) / 36px (desktop)
- Line Height: 34sp / 44px
- Letter Spacing: -0.01em
- Color: #000000 (light) / #FFFFFF (dark)

Alt Başlık (H2):
- Font: Inter Semibold (600)
- Size: 22sp (mobil) / 28px (desktop)
- Line Height: 28sp / 36px
- Letter Spacing: -0.01em

Body Text:
- Font: Inter Regular (400)
- Size: 16sp (mobil) / 16px (desktop)
- Line Height: 24sp / 24px
- Letter Spacing: 0

Caption:
- Font: Inter Regular (400)
- Size: 12sp (mobil) / 12px (desktop)
- Line Height: 16sp / 16px
- Color: #666666 (light) / #CCCCCC (dark)

Button Text:
- Font: Inter Semibold (600)
- Size: 16sp (mobil) / 16px (desktop)
- Letter Spacing: 0.01em
```

### 7. **BİLEŞENLER & ELEMENTLER**

```
Header:
- [Logo / Geri butonu / Başlık / Aksiyon butonları]
- Sticky: [Evet/Hayır]
- Height: [X]dp/px

Content Area:
- [Liste / Grid / Form / vb.]
- Scroll: [Vertical / Horizontal / Her ikisi]
- Pagination: [Var/Yok]

Action Buttons:
- Primary: [Buton metni, pozisyon, stil]
- Secondary: [Buton metni, pozisyon, stil]
- Sticky: [Evet/Hayır]

Empty States:
- [Boş durum tasarımı]
- Icon: [Hangi ikon]
- Message: [Mesaj metni]
- CTA: [Call-to-action]

Loading States:
- [Yükleme göstergesi tipi]
- [Skeleton loader / Spinner / Progress bar]
```

### 8. **ANİMASYON & GEÇİŞLER**

```
Entry Animation:
- [Fade / Slide / Scale / Kombinasyon]
- Duration: [X]ms
- Easing: [ease-out / ease-in-out / cubic-bezier]
- Delay: [Y]ms

Exit Animation:
- [Fade / Slide / Scale / Kombinasyon]
- Duration: [X]ms
- Easing: [ease-in / ease-out]

Micro Interactions:
- [Buton press / Hover / Focus / vb.]
- Duration: [X]ms
- Feedback: [Haptic / Visual / Audio]

Page Transitions:
- [Cross-fade / Slide / Push / Modal]
- Duration: [X]ms
```

### 9. **ETKİLEŞİM & DAVRANIŞ**

```
Kullanıcı Aksiyonları:
- [Tıklama / Swipe / Scroll / Long press / vb.]
- [Her aksiyonun sonucu]

Form Validasyonu:
- [Real-time / On submit]
- [Hata mesajları konumu]
- [Başarı feedback'i]

Pull to Refresh:
- [Var/Yok]
- [Animasyon tipi]

Infinite Scroll:
- [Var/Yok]
- [Loading indicator pozisyonu]

Modal/Drawer:
- [Açılma yönü]
- [Backdrop opacity]
- [Kapatma yöntemleri]
```

### 10. **ERİŞİLEBİLİRLİK**

```
Screen Reader:
- [Accessibility labels]
- [Hints]
- [Traits / Roles]

Touch Targets:
- Minimum: 44x44dp (iOS) / 48x48dp (Android)
- [Elementlerin touch target boyutları]

Keyboard Navigation:
- [Tab order]
- [Focus states]
- [Keyboard shortcuts]

Contrast Ratios:
- Text: WCAG AA (4.5:1) / AAA (7:1)
- [Kontrast değerleri]

Reduced Motion:
- [Animasyon alternatifleri]
- [Statik fallback'ler]
```

### 11. **PLATFORM SPESİFİK DETAYLAR**

```
iOS:
- Safe Area: [Notch / Dynamic Island / Home indicator]
- Gestures: [Swipe back / Pull to refresh]
- Haptic: [Feedback tipleri]

Android:
- Edge-to-edge: [Status bar / Navigation bar]
- Material Design: [Elevation / Ripple]
- Back button: [Davranış]

Desktop:
- Hover states: [Hover efektleri]
- Keyboard shortcuts: [Kısayollar]
- Window resize: [Responsive davranış]
```

### 12. **DURUMLAR & VARYANTLAR**

```
Loading State:
- [Skeleton / Spinner / Progress]
- [Pozisyon ve stil]

Empty State:
- [Icon + Message + CTA]
- [Tasarım detayları]

Error State:
- [Hata mesajı]
- [Retry butonu]
- [Tasarım]

Success State:
- [Başarı mesajı]
- [Feedback animasyonu]

Offline State:
- [İnternet yok durumu]
- [Tasarım]

Dark Mode:
- [Renk değişimleri]
- [Asset değişimleri]
```

### 13. **TEKNİK DETAYLAR**

```
Performance:
- [Lazy loading gereksinimleri]
- [Image optimization]
- [Code splitting]

Responsive Breakpoints:
- Mobile: < 768px
- Tablet: 768px - 1024px
- Desktop: > 1024px

Asset Requirements:
- [Image boyutları]
- [Icon set]
- [Font dosyaları]

API Integration:
- [Data structure]
- [Loading states]
- [Error handling]
```

### 14. **ÖZEL NOTLAR & KISITLAMALAR**

```
Özel Gereksinimler:
- [Özel durumlar]
- [Kısıtlamalar]
- [Dikkat edilmesi gerekenler]

Kaçınılması Gerekenler:
- [Gradyan kullanma]
- [Aşırı animasyon]
- [Karmaşık layout]
- [Renkli accent'ler]

Best Practices:
- [Önerilen yaklaşımlar]
- [Pattern'ler]
- [Standartlar]
```

---

## 📝 ÖRNEK KULLANIM

### Örnek: Ana Sayfa Ekranı

```
# 📱 MERGE - ANA SAYFA EKRAN DETAYLI ANALİZ

## 1. TEMEL ÖZELLİKLER & TASARIM PRENSİPLERİ

### 1.1. Ana Sayfa'nın Amacı

Ana Fonksiyonlar:
- Ürün keşfi: Kullanıcıya ürünleri sunma
- Kategori navigasyonu: Hızlı kategori erişimi
- Kampanya gösterimi: Öne çıkan kampanyalar
- Kişiselleştirme: Size özel öneriler

Kullanıcı Hedefi:
- Hızlıca ürün bulma
- Kampanyaları keşfetme
- Kategorilere göz atma

Merge İçin Özel Değer:
- Hibrit platform: Pazaryeri + kendi site ürünleri
- Multi-vendor: Farklı satıcıların ürünleri
- Personalization: AI-driven öneriler

### 1.2. Minimalist Tasarım DNA'sı

Tasarım Prensipleri:
- Monochrome palette (siyah-beyaz)
- Geometric forms
- Negative space dominance
- Flat design (gradyan yok, shadow yok)
- Typography as hero

## 2. EKRAN DÜZENİ & LAYOUT DETAYLARI

### 2.1. Mobil Layout (375x812dp)

Layout Grid:
├─ Safe Area Top: 44dp
├─ Header Zone: 56dp
│  ├─ Logo: 24x24dp (sol)
│  ├─ Arama: 240dp genişlik (merkez)
│  ├─ Sepet: 24x24dp (sağ)
│  └─ Profil: 24x24dp (sağ)
├─ Content Zone: Scrollable
│  ├─ Kategori Ikonları: 80dp yükseklik (horizontal scroll)
│  ├─ Banner Slider: 200dp yükseklik
│  ├─ Flash Ürünler: 280dp yükseklik (horizontal scroll)
│  ├─ Kampanyalı Ürünler: 280dp yükseklik (grid 2 kolon)
│  ├─ Size Özel: 280dp yükseklik (horizontal scroll)
│  ├─ Çok Satanlar: 280dp yükseklik (grid 2 kolon)
│  └─ Yeni Gelenler: 280dp yükseklik (grid 2 kolon)
├─ Scroll to Top: 48x48dp (sağ alt, sticky)
└─ Safe Area Bottom: 34dp

Element Pozisyonları:
- Header: X: 0, Y: 0, Height: 56dp, Sticky: true
- Kategori Ikonları: X: 16dp, Y: 72dp, Height: 80dp
- Banner: X: 0, Y: 160dp, Height: 200dp, Full width
- Flash Ürünler: X: 16dp, Y: 368dp, Height: 280dp
- Scroll to Top: X: 327dp, Y: 730dp, Size: 48x48dp

Spacing:
- Section gap: 24dp
- Horizontal padding: 16dp
- Card gap: 12dp

### 2.2. Desktop Layout (1440x900px)

Layout Grid:
├─ Header: 80px yükseklik
│  ├─ Logo: 32x32px (sol)
│  ├─ Mega Menü: 600px genişlik (merkez)
│  ├─ Arama: 400px genişlik (merkez)
│  └─ Sepet + Profil: 120px genişlik (sağ)
├─ Content: Max-width 1200px, centered
│  ├─ Banner Carousel: 600px yükseklik
│  ├─ Kategori Grid: 4x2 grid
│  ├─ Ürün Slider'ları: 3 kolon grid
│  └─ Footer: 200px yükseklik
└─ Sticky Header: Mini header on scroll

## 3. RENK PALETİ

Light Mode:
- Background: #FFFFFF
- Header Background: #FFFFFF
- Card Background: #FFFFFF
- Border: #E5E5E5
- Primary Text: #000000
- Secondary Text: #666666
- Price Text: #000000
- Discount Badge: #000000 (bg: #F5F5F5)

Dark Mode:
- Background: #000000
- Header Background: #000000
- Card Background: #0A0A0A
- Border: #333333
- Primary Text: #FFFFFF
- Secondary Text: #CCCCCC
- Price Text: #FFFFFF
- Discount Badge: #FFFFFF (bg: #1A1A1A)

## 4. TİPOGRAFİ

Section Başlıkları:
- Font: Inter Semibold (600)
- Size: 20sp (mobil) / 24px (desktop)
- Line Height: 28sp / 32px

Ürün İsmi:
- Font: Inter Regular (400)
- Size: 14sp (mobil) / 16px (desktop)
- Line Height: 20sp / 24px
- Max lines: 2

Fiyat:
- Font: Inter Semibold (600)
- Size: 16sp (mobil) / 18px (desktop)
- Color: #000000 / #FFFFFF

İndirim Oranı:
- Font: Inter Semibold (600)
- Size: 12sp (mobil) / 14px (desktop)
- Color: #000000 / #FFFFFF

## 5. BİLEŞENLER & ELEMENTLER

Header:
- Sticky: Evet
- Background: Solid color (no blur)
- Border bottom: 1px solid #E5E5E5 (light) / #333333 (dark)
- Height: 56dp (mobil) / 80px (desktop)

Kategori İkonları:
- Size: 56x56dp (touch target)
- Icon: 24x24dp
- Label: 12sp, centered below
- Horizontal scroll, snap points
- Gap: 16dp

Banner Slider:
- Height: 200dp (mobil) / 600px (desktop)
- Full width
- Swipeable (mobile)
- Pagination dots: bottom center
- Auto-play: 5 saniye

Ürün Kartları:
- Size: 160x240dp (mobil) / 280x400px (desktop)
- Border: 1px solid #E5E5E5 / #333333
- Border radius: 0 (sharp corners)
- Image: 160x160dp / 280x280px
- Padding: 12dp / 16px
- Gap: 8dp / 12px

Flash Sale Badge:
- Position: Top right corner
- Size: 32x32dp
- Background: #000000 / #FFFFFF
- Text: #FFFFFF / #000000
- Font: Inter Semibold, 10sp

Scroll to Top Button:
- Size: 48x48dp
- Position: Bottom right, 24dp from edges
- Background: #000000 / #FFFFFF
- Icon: #FFFFFF / #000000
- Border radius: 0
- Sticky: Evet (scroll > 500dp)

## 6. ANİMASYON & GEÇİŞLER

Entry Animation:
- Fade in: 0 → 1 opacity
- Duration: 300ms
- Easing: ease-out
- Stagger: 50ms per section

Banner Slider:
- Transition: Slide (horizontal)
- Duration: 400ms
- Easing: ease-in-out

Ürün Kart Hover (Desktop):
- Scale: 1.0 → 1.02
- Duration: 200ms
- Easing: ease-out
- Border: 1px → 2px

Scroll to Top:
- Fade in on scroll: opacity 0 → 1
- Duration: 200ms
- Position: Fixed, bottom right

Pull to Refresh:
- Indicator: Minimal line (top)
- Color: #000000 / #FFFFFF
- Height: 2dp
- Animation: Linear sweep

## 7. ETKİLEŞİM & DAVRANIŞ

Kullanıcı Aksiyonları:
- Tap ürün kartı → Ürün detay sayfası
- Swipe banner → Sonraki/önceki banner
- Tap kategori → Kategori sayfası
- Scroll → Lazy load more products
- Pull down → Refresh content
- Tap scroll to top → Smooth scroll to top

Infinite Scroll:
- Trigger: 200dp before bottom
- Loading indicator: Bottom center
- Skeleton loader: 4 product cards

Empty States:
- No products: Icon + "Henüz ürün yok" + CTA
- No internet: Icon + "İnternet bağlantısı yok" + Retry button

## 8. ERİŞİLEBİLİRLİK

Screen Reader:
- Header: "Ana sayfa, arama, sepet, profil"
- Banner: "Kampanya banner, [başlık]"
- Ürün kartı: "[Ürün adı], [fiyat], [indirim]"
- Kategori: "[Kategori adı] kategorisi"

Touch Targets:
- Minimum: 44x44dp (iOS) / 48x48dp (Android)
- Kategori ikonları: 56x56dp ✓
- Ürün kartları: 160x240dp ✓
- Butonlar: 44x44dp minimum ✓

Contrast Ratios:
- Primary text: 21:1 ✓ (AAA)
- Secondary text: 7:1 ✓ (AAA)
- Borders: 12.6:1 ✓ (AAA)

## 9. PLATFORM SPESİFİK DETAYLAR

iOS:
- Safe Area: Notch (44dp) / Dynamic Island (54dp)
- Pull to refresh: Native iOS style
- Swipe back: Native gesture

Android:
- Status bar: 24dp
- Navigation bar: 48dp (3-button) / 0dp (gesture)
- Material ripple: Minimal (flat design)

Desktop:
- Hover states: Ürün kartları, butonlar
- Keyboard navigation: Tab order
- Scroll behavior: Smooth

## 10. DURUMLAR & VARYANTLAR

Loading State:
- Skeleton loader: 4 product cards
- Gray boxes with shimmer effect
- Same layout as content

Empty State:
- Icon: Shopping bag (outline)
- Message: "Henüz ürün bulunmuyor"
- CTA: "Kategorilere Göz At" (button)

Error State:
- Icon: Alert circle
- Message: "Bir şeyler ters gitti"
- CTA: "Tekrar Dene" (button)

Offline State:
- Banner: "İnternet bağlantısı yok"
- Cached content: Göster (eğer varsa)
- Retry button: Top right

Dark Mode:
- Tüm renkler invert
- Images: No change (product photos)
- Icons: Invert colors

## 11. TEKNİK DETAYLAR

Performance:
- Lazy load images: Intersection Observer
- Code splitting: Route-based
- Image optimization: WebP, responsive sizes
- Virtual scrolling: FlashList (mobile)

Responsive Breakpoints:
- Mobile: < 768px (1-2 kolon)
- Tablet: 768px - 1024px (2-3 kolon)
- Desktop: > 1024px (3-4 kolon)

API Integration:
- Endpoint: `/api/products?page=1&limit=20`
- Response: `{ products: [], total: 0, hasMore: boolean }`
- Loading: TanStack Query
- Error: Error boundary

## 12. ÖZEL NOTLAR & KISITLAMALAR

Özel Gereksinimler:
- Flash sale: Sayaç göster (countdown timer)
- Personalization: AI öneriler (backend'den)
- Multi-vendor: Satıcı badge göster

Kaçınılması Gerekenler:
- Gradyan kullanma
- Drop shadow ekleme
- Renkli accent'ler
- Aşırı animasyon
- Karmaşık layout

Best Practices:
- 8dp grid sistemi
- Consistent spacing
- Pixel-perfect alignment
- Performance-first
- Accessibility-first
```

---

## 🎯 KULLANIM TALİMATLARI

1. **Ekran Seçimi**: `screen.md` dosyasından tasarlanacak ekranı seçin
2. **Şablonu Doldurun**: Yukarıdaki şablonu ekran özelliklerine göre doldurun
3. **Stitch'e Verin**: Tamamlanan prompt'u Stitch AI'a verin
4. **İterasyon**: Gerekirse prompt'u iyileştirin ve tekrar deneyin

---

## 📌 İPUÇLARI

- **Detaylı Olun**: Ne kadar detaylı olursa, sonuç o kadar iyi olur
- **Tutarlılık**: Tüm ekranlarda aynı tasarım dilini kullanın
- **Platform Farkları**: Mobil ve desktop için ayrı detaylar verin
- **Durumlar**: Loading, error, empty state'leri unutmayın
- **Erişilebilirlik**: Her zaman accessibility'yi düşünün

