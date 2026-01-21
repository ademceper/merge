import { View, Pressable, useColorScheme } from "react-native";
import { Icon } from "@merge/uim/components/icon";
import { ArrowUp } from "lucide-react-native";
import { useRef } from "react";
import { ScrollView } from "react-native";

interface ScrollToTopProps {
  scrollViewRef: React.RefObject<ScrollView | null>;
  show: boolean;
}

export function ScrollToTop({ scrollViewRef, show }: ScrollToTopProps) {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";

  if (!show) return null;

  const handlePress = () => {
    scrollViewRef.current?.scrollTo({ y: 0, animated: true });
  };

  return (
    <Pressable
      onPress={handlePress}
      style={{
        position: "absolute",
        bottom: 50,
        right: 16,
        width: 48,
        height: 48,
        borderRadius: 24,
        backgroundColor: isDark ? "#ffffff" : "#111118",
        alignItems: "center",
        justifyContent: "center",
        borderWidth: 1,
        borderColor: isDark ? "rgba(255, 255, 255, 0.1)" : "rgba(0, 0, 0, 0.1)",
        shadowColor: "#000",
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.1,
        shadowRadius: 4,
        elevation: 3,
      }}>
      <Icon
        as={ArrowUp}
        size={20}
        className="text-white"
      />
    </Pressable>
  );
}
