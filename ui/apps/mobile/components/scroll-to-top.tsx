import { View, Pressable } from "react-native";
import { Icon } from "@merge/uim/components/icon";
import { ArrowUp } from "lucide-react-native";
import { useRef } from "react";
import { ScrollView } from "react-native";

interface ScrollToTopProps {
  scrollViewRef: React.RefObject<ScrollView | null>;
  show: boolean;
}

export function ScrollToTop({ scrollViewRef, show }: ScrollToTopProps) {
  if (!show) return null;

  const handlePress = () => {
    scrollViewRef.current?.scrollTo({ y: 0, animated: true });
  };

  return (
    <Pressable
      onPress={handlePress}
      className="absolute bottom-24 right-4 flex items-center justify-center w-12 h-12 rounded-full bg-foreground dark:bg-white border border-primary/10 dark:border-white/10 shadow-lg">
      <Icon
        as={ArrowUp}
        size={20}
        className="text-background dark:text-[#111118]"
      />
    </Pressable>
  );
}
