import { View, ScrollView } from "react-native";
import { StatusBar } from "expo-status-bar";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Text } from "@merge/uim/components/text";
import { Input } from "@merge/uim/components/input";
import { Search } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";

export default function SearchScreen() {
  const insets = useSafeAreaInsets();

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <StatusBar style="auto" />
      <ScrollView
        className="flex-1"
        contentContainerClassName="p-4 gap-4"
        contentContainerStyle={{ paddingBottom: insets.bottom + 100 }}>
        <View className="gap-2">
          <Text className="text-2xl font-bold">Ara</Text>
          <View className="relative">
            <Input
              placeholder="Ürün, kategori veya marka ara..."
              className="pl-10"
            />
            <View className="absolute left-3 top-3">
              <Icon
                as={Search}
                size={20}
                className="text-muted-foreground"
              />
            </View>
          </View>
        </View>

        <View className="flex-1 items-center justify-center py-12">
          <Icon
            as={Search}
            size={48}
            className="text-muted-foreground/30 mb-4"
          />
          <Text className="text-base text-muted-foreground text-center">
            Arama yapmak için yukarıdaki alana yazın
          </Text>
        </View>
      </ScrollView>
    </View>
  );
}
