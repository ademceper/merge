import { View, ScrollView, Pressable } from "react-native";
import { Text } from "@merge/uim/components/text";
import { Icon } from "@merge/uim/components/icon";
import { ChevronRight } from "lucide-react-native";
import { ProductCard, ProductCardProps } from "./product-card";

interface ProductSectionProps {
  title: string;
  products: ProductCardProps[];
  onSeeAll?: () => void;
}

export function ProductSection({ title, products, onSeeAll }: ProductSectionProps) {
  return (
    <View className="mb-6">
      {/* Section Header */}
      <View className="flex-row items-center justify-between px-4 mb-3">
        <Text className="text-xl font-bold uppercase tracking-tight text-foreground dark:text-white">
          {title}
        </Text>
        {onSeeAll && (
          <Pressable
            onPress={onSeeAll}
            className="flex-row items-center gap-0.5">
            <Text className="text-sm font-medium text-muted-foreground dark:text-white/70">
              Tümünü Gör
            </Text>
            <Text className="text-sm font-medium text-muted-foreground dark:text-white/70">
              {" >"}
            </Text>
          </Pressable>
        )}
      </View>

      {/* Products Scroll */}
      <ScrollView
        horizontal
        nestedScrollEnabled={true}
        showsHorizontalScrollIndicator={false}
        scrollEnabled={true}
        bounces={false}
        directionalLockEnabled={true}
        contentContainerStyle={{ paddingHorizontal: 16, paddingRight: 16 }}>
        {products.map((product) => (
          <ProductCard
            key={product.id}
            {...product}
            onPress={() => {
              // Navigate to product detail
              product.onPress?.();
            }}
          />
        ))}
      </ScrollView>
    </View>
  );
}
