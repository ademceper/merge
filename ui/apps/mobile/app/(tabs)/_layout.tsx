import { Tabs } from "expo-router";
import { useColorScheme } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Home, Search, Heart, ShoppingCart, User } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";
import { View } from "react-native";

export default function TabsLayout() {
  const colorScheme = useColorScheme();
  const insets = useSafeAreaInsets();
  const isDark = colorScheme === "dark";

  const getTabIcon = (IconComponent: typeof Home) => {
    return ({ focused, color }: { focused: boolean; color: string; size: number }) => (
      <View className="items-center justify-center">
        <Icon as={IconComponent} size={22} color={color} />
      </View>
    );
  };

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: {
          height: 64 + insets.bottom,
          paddingBottom: insets.bottom + 4,
          paddingTop: 8,
          paddingHorizontal: 8,
          elevation: 0,
          shadowOpacity: 0,
        },
        tabBarActiveTintColor: isDark ? "#ffffff" : "#000000",
        tabBarInactiveTintColor: isDark ? "rgba(255, 255, 255, 0.6)" : "rgba(0, 0, 0, 0.6)",
        tabBarLabelStyle: {
          fontSize: 10,
          fontFamily: "SpaceGrotesk-Medium",
          textTransform: "uppercase",
          letterSpacing: 0.6,
          marginTop: 4,
          marginBottom: 0,
        },
        tabBarItemStyle: {
          paddingVertical: 6,
          paddingHorizontal: 4,
        },
        tabBarIconStyle: {
          marginTop: 0,
        },
        tabBarHideOnKeyboard: true,
        tabBarBackground: () => (
          <View className="flex-1 border-t bg-background dark:bg-[#101022] border-border dark:border-white/10" />
        ),
      }}>
      <Tabs.Screen
        name="home"
        options={{
          title: "Ana Sayfa",
          tabBarIcon: getTabIcon(Home),
        }}
      />
      <Tabs.Screen
        name="search"
        options={{
          title: "Ara",
          tabBarIcon: getTabIcon(Search),
        }}
      />
      <Tabs.Screen
        name="favorites"
        options={{
          title: "Favoriler",
          tabBarIcon: getTabIcon(Heart),
        }}
      />
      <Tabs.Screen
        name="cart"
        options={{
          title: "Sepet",
          tabBarIcon: getTabIcon(ShoppingCart),
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: "Profil",
          tabBarIcon: getTabIcon(User),
        }}
      />
    </Tabs>
  );
}
