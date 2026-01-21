import { View, Pressable } from "react-native";
import { Icon } from "@merge/uim/components/icon";
import { ArrowUp } from "lucide-react-native";
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
      className="absolute bg-primary items-center justify-center border border-border"
      style={{
        bottom: 50,
        right: 16,
        width: 48,
        height: 48,
        borderRadius: 24,
        shadowColor: "#000",
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.1,
        shadowRadius: 4,
        elevation: 3,
      }}>
      <Icon
        as={ArrowUp}
        size={20}
        className="text-primary-foreground"
      />
    </Pressable>
  );
}
