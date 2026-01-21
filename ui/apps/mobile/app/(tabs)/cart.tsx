import { View, ScrollView } from "react-native";
import { StatusBar } from "expo-status-bar";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Text } from "@merge/uim/components/text";
import { Button } from "@merge/uim/components/button";
import { ShoppingCart } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";

export default function Cart() {
  const insets = useSafeAreaInsets();

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <StatusBar style="auto" />
      <ScrollView
        className="flex-1"
        contentContainerClassName="p-4 gap-4"
        contentContainerStyle={{ paddingBottom: insets.bottom + 100 }}>
        <Text className="text-2xl font-bold">Sepet</Text>

        <View className="flex-1 items-center justify-center py-12">
          <View className="flex items-center justify-center p-6 rounded-full border border-primary/10 dark:border-white/10 bg-white dark:bg-white/5 mb-4">
            <Icon
              as={ShoppingCart}
              size={48}
              className="text-muted-foreground"
            />
          </View>
          <Text className="text-xl font-bold mb-2">Sepetiniz boş</Text>
          <Text className="text-base text-muted-foreground text-center mb-6">
            Alışverişe başlamak için ürünleri keşfedin
          </Text>
          <Button className="w-full max-w-xs">
            Alışverişe Başla
          </Button>
        </View>
      </ScrollView>
    </View>
  );
}
