# 🎨 STITCH PROMPT ŞABLONU - MERGE UI MOBILE EKRAN TASARIMI

Bu dokümantasyon, Stitch AI ile **MOBILE** ekran tasarımları oluştururken kullanılacak standart prompt şablonunu içerir. Her ekran için bu şablonu kullanarak tutarlı ve detaylı tasarımlar elde edebilirsiniz.

---

## 📋 PROMPT YAPISI

### 1. **EKRAN BİLGİLERİ & BAĞLAM**

```
Ekran Adı: [Ekranın tam adı]
Platform: Mobile (iOS + Android)
Framework: React Native 0.81.5 + Expo 54.0.29
Routing: Expo Router 6.0.19 (file-based)
Bölüm: [Müşteri Tarafı - Pazaryeri / Satıcı Sitesi / Satıcı Paneli / Admin Paneli]
Kategori: [Onboarding / Ana Sayfa / Ürün / Sepet / Profil / vb.]
Architecture: New Architecture enabled
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
- Native mobile experience
- Platform-specific optimizations (iOS/Android)
- Gesture-based interactions
- Offline-first capability
- Push notifications ready
- Deep linking support
```

### 3. **TEKNOLOJİ STACK & YAPILANDIRMA**

```
Framework & Build:
- React Native 0.81.5
- Expo 54.0.29 (SDK)
- Expo Router 6.0.19 (file-based routing)
- TypeScript 5.9.2
- New Architecture: Enabled
- Metro Bundler (with NativeWind support)

UI Kütüphanesi:
- @merge/uim (workspace package)
- @rn-primitives/* (React Native primitives)
- Lucide React Native (icons)
- NativeWind 4.2.1 (Tailwind for RN)

Stil Sistemi:
- NativeWind (Tailwind CSS for React Native)
- Tailwind CSS 3.4.19
- CSS-in-JS via className prop
- StyleSheet API (when needed)
- react-native-css-interop 0.2.1

Font:
- Space Grotesk (@expo-google-fonts/space-grotesk)
- Weights: 300 (Light), 400 (Regular), 500 (Medium), 700 (Bold)
- Font names: "SpaceGrotesk-Light", "SpaceGrotesk-Regular", "SpaceGrotesk-Medium", "SpaceGrotesk-Bold"
- Loaded via: expo-font useFonts hook
- Tailwind config: fontFamily.sans, .light, .medium, .bold

State Management:
- React hooks (useState, useEffect, useContext)
- React Native Reanimated (for animations)
- AsyncStorage (@react-native-async-storage/async-storage)

Routing:
- Expo Router (file-based)
- Stack navigation (default)
- Dynamic routes: [param].tsx
- Layouts: _layout.tsx
- Tabs: (tabs)/_layout.tsx
- Groups: (group)/

Animations:
- react-native-reanimated 4.1.1
- react-native-gesture-handler 2.20.2
- Worklets: react-native-worklets 0.5.1

Platform APIs:
- expo-status-bar: Status bar styling
- expo-splash-screen: Splash screen control
- expo-linking: Deep linking
- expo-constants: App constants
- react-native-safe-area-context: Safe area handling
```

### 4. **TASARIM PRENSİPLERİ**

```
Tasarım DNA'sı:
- Minimalist: Monochrome palette (siyah-beyaz)
- Geometric: Keskin, matematiksel formlar
- Negative Space: Boşluk = tasarım
- Flat Design: Gradyan yok, shadow minimal, depth yok
- Typography as Hero: Yazı tipi kimlik
- Native Feel: Platform conventions respected

Platform Guidelines:
- iOS: Human Interface Guidelines
- Android: Material Design 3 (minimal)
- Merge Adaptation: Monochrome-first, geometric precision
```

### 5. **EKRAN DÜZENİ & LAYOUT**

```
Screen Dimensions:
- iPhone 13 Pro (standard): 390x844pt
- iPhone SE (small): 375x667pt
- iPhone 14 Pro Max (large): 430x932pt
- Android: Various (responsive design)

Safe Areas:
- iOS Top: 44pt (notch) / 54pt (Dynamic Island) / 20pt (no notch)
- iOS Bottom: 34pt (home indicator) / 0pt (button)
- Android Top: 24dp (status bar)
- Android Bottom: 48dp (3-button) / 0dp (gesture)

Layout Grid (Portrait):
├─ Safe Area Top: [Dynamic based on device]
├─ Header Zone: 56dp yükseklik
│  ├─ Status Bar: 24dp (Android) / Dynamic (iOS)
│  ├─ Header Content: 56dp
│  └─ Border: 1dp bottom border
├─ Content Zone: Flexible (flex: 1)
│  ├─ ScrollView: Vertical scroll
│  ├─ Padding: 16dp horizontal
│  └─ Gap: 16dp, 24dp between sections
├─ Action Zone: 64dp yükseklik (if sticky)
│  └─ Padding: 16dp
└─ Safe Area Bottom: [Dynamic based on device]

Spacing Sistemi:
- 8dp grid sistemi (base unit)
- Padding: 8dp, 12dp, 16dp, 24dp, 32dp
- Gap: 8dp, 12dp, 16dp, 24dp
- Margin: 8dp, 12dp, 16dp, 24dp, 32dp
- Touch targets: Minimum 44x44dp (iOS) / 48x48dp (Android)
```

### 6. **RENK PALETİ (HSL COLOR SPACE)**

```
Light Mode (Tailwind Config):
- background: hsl(0 0% 100%) - Pure white
- foreground: hsl(0 0% 0%) - Pure black
- card: hsl(0 0% 100%) - White
- card-foreground: hsl(0 0% 0%) - Black
- popover: hsl(0 0% 100%) - White
- popover-foreground: hsl(0 0% 0%) - Black
- primary: hsl(0 0% 0%) - Black
- primary-foreground: hsl(0 0% 100%) - White
- secondary: hsl(0 0% 96%) - Light gray
- secondary-foreground: hsl(0 0% 9%) - Near black
- muted: hsl(0 0% 96%) - Light gray
- muted-foreground: hsl(0 0% 45%) - Medium gray
- accent: hsl(0 0% 96%) - Light gray
- accent-foreground: hsl(0 0% 0%) - Black
- destructive: hsl(0 84.2% 60.2%) - Red
- border: hsl(0 0% 90%) - Light border
- input: hsl(0 0% 90%) - Input border
- ring: hsl(0 0% 0%) - Focus ring

Dark Mode (System):
- Handled by useColorScheme() hook
- Colors invert automatically
- Background: #000000 or #101022 (OLED optimized)
- Foreground: #FFFFFF

Kullanım:
- Tailwind classes: bg-background, text-foreground, border-border
- Dynamic: useColorScheme() for dark mode detection
- className prop: NativeWind handles conversion
```

### 7. **TİPOGRAFİ**

```
Font Family:
- Space Grotesk (via expo-font)
- Font names:
  - "SpaceGrotesk-Light" (300)
  - "SpaceGrotesk-Regular" (400)
  - "SpaceGrotesk-Medium" (500)
  - "SpaceGrotesk-Bold" (700)
- Tailwind: font-sans, font-light, font-medium, font-bold

Font Sizes (sp units - scale with system):
- H1: text-3xl (30sp) / text-4xl (36sp)
- H2: text-2xl (24sp) / text-3xl (30sp)
- H3: text-xl (20sp) / text-2xl (24sp)
- Body: text-base (16sp)
- Small: text-sm (14sp)
- Caption: text-xs (12sp)

Line Heights:
- H1: leading-tight (1.2)
- H2: leading-snug (1.375)
- Body: leading-normal (1.5)
- Small: leading-relaxed (1.625)

Letter Spacing:
- Headings: tracking-tight (-0.01em)
- Body: tracking-normal (0)
- Uppercase: tracking-wide (0.05em)

Colors:
- Primary: text-foreground
- Secondary: text-muted-foreground
- Accent: text-accent-foreground
```

### 8. **BİLEŞENLER & ELEMENTLER**

```
@merge/uim Bileşen Kütüphanesi:
- Accordion, Alert, Alert Dialog, Aspect Ratio, Avatar
- Badge, Button, Card, Checkbox, Collapsible
- Context Menu, Dialog, Dropdown Menu, Icon
- Input, Label, Menubar, Popover, Progress
- Radio Group, Select, Separator, Skeleton
- Switch, Tabs, Text, Textarea, Toggle, Toggle Group

Import Pattern:
import { Component } from "@merge/uim/components/component-name"

Icon Kütüphanesi:
- Lucide React Native (lucide-react-native)
- Import: import { IconName } from "lucide-react-native"
- Size: 16, 20, 24, 32 (w-4, w-5, w-6, w-8)

React Native Core:
- View: Container, layout
- Text: Text content
- ScrollView: Scrollable content
- Pressable: Buttons, touchable areas
- Image: Images
- TextInput: Text input (wrapped in Input component)
- StatusBar: Status bar styling (expo-status-bar)

Header:
- Component: Custom or Stack.Screen options
- Height: 56dp
- Background: bg-background
- Border: border-b border-border
- Safe area: useSafeAreaInsets()

Content Area:
- ScrollView: flex-1, vertical scroll
- Padding: px-4 (16dp)
- Gap: gap-4 (16dp) or gap-6 (24dp)

Action Buttons:
- Component: Button from @merge/uim
- Variants: default, destructive, outline, secondary, ghost
- Sizes: default (h-10), sm (h-9), lg (h-11)
- Touch target: Minimum 44x44dp

Empty States:
- Component: Custom View
- Icon: Lucide icon
- Message: Text component
- CTA: Button component

Loading States:
- Skeleton: Skeleton from @merge/uim
- Spinner: ActivityIndicator or custom
- Pattern: Match content layout
```

### 9. **ANİMASYON & GEÇİŞLER**

```
React Native Reanimated:
- useSharedValue: Shared values for animations
- useAnimatedStyle: Animated styles
- withTiming: Timing animations
- withSpring: Spring animations
- withSequence: Sequence animations
- runOnJS: Run JS functions from worklet

Entry Animation:
- FadeIn: Fade in effect
- SlideInRight: Slide from right
- SlideInLeft: Slide from left
- Duration: 200ms, 300ms, 400ms
- Easing: Easing.out(Easing.ease)

Exit Animation:
- FadeOut: Fade out effect
- SlideOutRight: Slide to right
- Duration: 150ms, 200ms

Gesture Animations:
- Pan gesture: react-native-gesture-handler
- Swipe: Horizontal pan gesture
- Pull to refresh: Vertical pan gesture
- Threshold: Screen width * 0.3

Micro Interactions:
- Button press: active:opacity-70
- Scale: transform: [{ scale: 0.95 }]
- Duration: 100ms

Page Transitions:
- Expo Router: Handles navigation
- Stack transitions: Default or custom
- Modal: Slide up animation

Haptic Feedback:
- expo-haptics (if needed)
- Light, medium, heavy impact
- iOS: Native haptics
- Android: Vibration API
```

### 10. **ETKİLEŞİM & DAVRANIŞ**

```
Kullanıcı Aksiyonları:
- Press: onPress handlers (Pressable)
- Long press: onLongPress
- Swipe: Gesture handler
- Scroll: ScrollView onScroll
- Pull to refresh: RefreshControl

Form Validasyonu:
- Real-time: onChangeText validation
- On submit: onSubmitEditing validation
- Error messages: Text below input
- Success feedback: Visual indicator

Pull to Refresh:
- RefreshControl component
- onRefresh handler
- refreshing state
- Colors: Primary color

Infinite Scroll:
- onEndReached: ScrollView prop
- onEndReachedThreshold: 0.5 (50% from bottom)
- Loading indicator: Bottom of list

Modal/Dialog:
- Component: Dialog from @merge/uim
- Backdrop: bg-black/50
- Close: onPress outside or button
- Animation: FadeIn/FadeOut

Sheet/Drawer:
- Custom implementation or library
- Slide from: bottom, left, right
- Backdrop: bg-black/50
- Gesture: Swipe to dismiss
```

### 11. **ERİŞİLEBİLİRLİK**

```
Screen Reader:
- accessibilityLabel: Button labels
- accessibilityHint: Action hints
- accessibilityRole: button, text, image, etc.
- accessibilityState: disabled, selected, etc.
- accessibilityValue: For sliders, etc.

Touch Targets:
- Minimum: 44x44dp (iOS) / 48x48dp (Android)
- Hit slop: hitSlop prop (extend touch area)
- Example: hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}

Keyboard Navigation:
- Not applicable (touch-first)
- Focus management: For modals, etc.

Contrast Ratios:
- Text: WCAG AA (4.5:1) minimum
- Large text: WCAG AA (3:1) minimum
- Test with: Accessibility Inspector (iOS) / Accessibility Scanner (Android)

Reduced Motion:
- Respect: AccessibilityInfo.isReduceMotionEnabled()
- Disable animations: If user prefers
- Static alternatives: Provide static states

Platform Accessibility:
- iOS: VoiceOver support
- Android: TalkBack support
- Dynamic Type: Support system font scaling
```

### 12. **PLATFORM SPESİFİK DETAYLAR**

```
iOS:
- Safe Area: useSafeAreaInsets() hook
- Status Bar: expo-status-bar, style="auto" or "light" or "dark"
- Gestures: Swipe back (native), pull to refresh
- Haptic: Native haptic feedback
- Dynamic Island: 54dp top safe area
- Notch: 44dp top safe area
- Home Indicator: 34dp bottom safe area
- Font: Supports Dynamic Type
- Appearance: useColorScheme() hook

Android:
- Status Bar: 24dp height
- Navigation Bar: 48dp (3-button) / 0dp (gesture)
- Edge-to-edge: react-native-is-edge-to-edge
- Material Design: Minimal (flat design)
- Back Button: Hardware back button handled
- Ripple: Material ripple effect (minimal)
- Font: System font scaling

Platform Detection:
- Platform.OS: "ios" or "android"
- Platform.select(): Platform-specific code
- Example: Platform.select({ ios: ..., android: ... })
```

### 13. **DURUMLAR & VARYANTLAR**

```
Loading State:
- Skeleton: Skeleton component
- Spinner: ActivityIndicator
- Pattern: Match content layout
- Position: Center or inline

Empty State:
- View: Centered container
- Icon: Lucide icon (large)
- Message: Text component
- CTA: Button component
- Design: Minimal, centered

Error State:
- Icon: Alert circle
- Message: Error description
- Retry button: Primary button
- Design: Centered, clear

Success State:
- Toast: Custom or library
- Message: Success description
- Auto-dismiss: 3-5 seconds
- Position: Top or bottom

Offline State:
- Banner: Top of screen
- Message: "No internet connection"
- Cached content: Show if available
- Retry: Button to reconnect

Dark Mode:
- Detection: useColorScheme() hook
- Colors: Auto-invert via Tailwind
- Images: No change (product photos)
- Icons: Auto-invert if needed
- Transition: Smooth (system handles)
```

### 14. **TEKNİK DETAYLAR**

```
Performance:
- FlatList: For long lists (virtualized)
- Image optimization: expo-image (if needed)
- Lazy loading: Lazy component loading
- Memoization: React.memo, useMemo, useCallback
- Code splitting: Metro bundler handles

Navigation:
- Expo Router: File-based routing
- Stack: Stack.Screen for navigation
- Tabs: (tabs)/_layout.tsx
- Deep linking: expo-linking
- URL scheme: com.turbo.example

Build & Deploy:
- Development: expo start
- Android: expo run:android
- iOS: expo run:ios
- Build: EAS Build (Expo Application Services)
- Updates: Expo Updates (OTA updates)

Environment:
- Constants: expo-constants
- Config: app.json
- Environment variables: .env files

Assets:
- Images: assets/ directory
- Fonts: expo-font
- Icons: Lucide React Native
- Splash: expo-splash-screen
```

### 15. **ÖZEL NOTLAR & KISITLAMALAR**

```
Özel Gereksinimler:
- New Architecture: Enabled
- NativeWind: CSS-in-JS via className
- Gesture Handler: Required for gestures
- Reanimated: Required for animations
- Safe Area: Always use useSafeAreaInsets()
- Status Bar: Always set style

Kaçınılması Gerekenler:
- Gradyan kullanma (flat design)
- Aşırı shadow/depth
- Renkli accent'ler (monochrome only)
- Aşırı animasyon (performance)
- Inline styles (use className)
- Platform.OS === "web" checks (mobile only)

Best Practices:
- 8dp grid sistemi
- Consistent spacing
- Touch targets: Minimum 44x44dp
- Safe area: Always respect
- Performance: Optimize lists
- Accessibility: Always add labels
- TypeScript: Strict mode
- NativeWind: Use className, not StyleSheet when possible
```

### 16. **KOD ÖRNEKLERİ**

```
Layout Structure:
```tsx
import { View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { StatusBar } from "expo-status-bar";
import { Stack } from "expo-router";

export default function Layout() {
  const insets = useSafeAreaInsets();
  
  return (
    <Stack
      screenOptions={{
        headerShown: false,
      }}
    />
  );
}
```

Screen Example:
```tsx
import { View, ScrollView } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { StatusBar } from "expo-status-bar";
import { Text } from "@merge/uim/components/text";
import { Button } from "@merge/uim/components/button";

export default function Screen() {
  const insets = useSafeAreaInsets();
  
  return (
    <View 
      className="flex-1 bg-background"
      style={{ paddingTop: insets.top, paddingBottom: insets.bottom }}
    >
      <StatusBar style="auto" />
      <ScrollView className="flex-1" contentContainerClassName="p-4 gap-4">
        <Text className="text-2xl font-bold">Title</Text>
        <Button onPress={() => {}}>Action</Button>
      </ScrollView>
    </View>
  );
}
```

Animation Example:
```tsx
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withTiming,
} from "react-native-reanimated";

function AnimatedComponent() {
  const opacity = useSharedValue(0);
  
  React.useEffect(() => {
    opacity.value = withTiming(1, { duration: 300 });
  }, []);
  
  const animatedStyle = useAnimatedStyle(() => ({
    opacity: opacity.value,
  }));
  
  return <Animated.View style={animatedStyle}>Content</Animated.View>;
}
```

Gesture Example:
```tsx
import { Gesture, GestureDetector } from "react-native-gesture-handler";
import Animated, { useSharedValue, useAnimatedStyle } from "react-native-reanimated";

function SwipeableComponent() {
  const translateX = useSharedValue(0);
  
  const panGesture = Gesture.Pan()
    .onUpdate((e) => {
      translateX.value = e.translationX;
    })
    .onEnd((e) => {
      // Handle swipe end
    });
  
  const animatedStyle = useAnimatedStyle(() => ({
    transform: [{ translateX: translateX.value }],
  }));
  
  return (
    <GestureDetector gesture={panGesture}>
      <Animated.View style={animatedStyle}>Content</Animated.View>
    </GestureDetector>
  );
}
```
```

---

## 🎯 KULLANIM TALİMATLARI

1. **Ekran Seçimi**: Hangi ekranı tasarlayacağınızı belirleyin
2. **Şablonu Doldurun**: Yukarıdaki şablonu ekran özelliklerine göre doldurun
3. **Platform Detayları**: iOS/Android spesifik özellikleri unutmayın
4. **Native Özellikler**: Safe areas, gestures, haptics, etc.
5. **Stitch'e Verin**: Tamamlanan prompt'u Stitch AI'a verin
6. **İterasyon**: Gerekirse prompt'u iyileştirin ve tekrar deneyin

---

## 📌 İPUÇLARI

- **React Native**: Native components kullanın (View, Text, ScrollView)
- **NativeWind**: Tailwind classes via className prop
- **Safe Areas**: Always use useSafeAreaInsets()
- **Gestures**: react-native-gesture-handler for swipe, pan, etc.
- **Animations**: react-native-reanimated for smooth animations
- **Performance**: FlatList for long lists, memoization
- **Accessibility**: accessibilityLabel, accessibilityRole
- **Touch Targets**: Minimum 44x44dp (iOS) / 48x48dp (Android)
- **Platform**: Platform.OS for platform-specific code
- **Dark Mode**: useColorScheme() hook
- **Font**: Space Grotesk, loaded via expo-font
- **Icons**: Lucide React Native
- **Components**: @merge/uim kütüphanesini kullanın

---

## 🔗 İLGİLİ DOSYALAR

- Layout: `apps/mobile/app/_layout.tsx`
- Components: `packages/uim/src/components/`
- Styles: `apps/mobile/global.css`
- Config: `apps/mobile/tailwind.config.js`
- Font Config: `apps/mobile/app/_layout.tsx` (useFonts)
- TypeScript: `apps/mobile/tsconfig.json`
