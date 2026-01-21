import { View, ScrollView } from "react-native";
import { Pressable } from "react-native";
import { Icon } from "@merge/uim/components/icon";
import { Text } from "@merge/uim/components/text";
import {
  Shirt,
  Laptop,
  Smartphone,
  Home,
  Baby,
  Dumbbell,
  Gamepad2,
  Book,
  Car,
  Watch,
  Camera,
  Heart,
} from "lucide-react-native";

const categories = [
  { id: "1", name: "Giyim", icon: Shirt },
  { id: "2", name: "Elektronik", icon: Laptop },
  { id: "3", name: "Telefon", icon: Smartphone },
  { id: "4", name: "Ev & Yaşam", icon: Home },
  { id: "5", name: "Bebek", icon: Baby },
  { id: "6", name: "Spor", icon: Dumbbell },
  { id: "7", name: "Oyun", icon: Gamepad2 },
  { id: "8", name: "Kitap", icon: Book },
  { id: "9", name: "Otomotiv", icon: Car },
  { id: "10", name: "Saat", icon: Watch },
  { id: "11", name: "Fotoğraf", icon: Camera },
  { id: "12", name: "Sağlık", icon: Heart },
];

export function CategoryIcons() {
  return (
    <View className="py-3">
      <ScrollView
        horizontal
        nestedScrollEnabled={true}
        showsHorizontalScrollIndicator={false}
        scrollEnabled={true}
        bounces={false}
        directionalLockEnabled={true}
        contentContainerStyle={{ paddingHorizontal: 16, gap: 12 }}>
        {categories.map((category) => (
          <Pressable
            key={category.id}
            className="items-center justify-center gap-1.5 w-18">
            <View className="flex items-center justify-center w-14 h-14 rounded-xl border border-primary/10 dark:border-white/10 bg-white dark:bg-white/5">
              <Icon
                as={category.icon}
                size={22}
                className="text-foreground dark:text-white"
              />
            </View>
            <Text className="text-xs font-medium text-center text-foreground/70 dark:text-white/70">
              {category.name}
            </Text>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}
