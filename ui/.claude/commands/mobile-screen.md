---
description: Create a new mobile screen with Expo Router
---

Create a new mobile screen: $ARGUMENTS

**Expo Router Structure:**
```
apps/mobile/app/
├── (auth)/           # Auth group
├── (tabs)/           # Tab navigation
│   ├── _layout.tsx   # Tab bar config
│   └── [screen].tsx
├── (stack)/          # Stack screens
│   └── [screen].tsx
└── _layout.tsx       # Root layout
```

**Screen Template:**
```tsx
import { View } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { Text } from '@merge/uim/components/text'

export default function ScreenName() {
  const insets = useSafeAreaInsets()

  return (
    <View
      className="flex-1 bg-background"
      style={{ paddingTop: insets.top }}
    >
      {/* Content */}
    </View>
  )
}
```

**Checklist:**
1. Create screen file in appropriate group
2. Add proper safe area handling
3. Use @merge/uim components
4. Add to navigation if needed
5. Handle dark/light mode
6. Add loading and error states
