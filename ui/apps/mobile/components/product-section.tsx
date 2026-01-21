import { View, ScrollView, Pressable } from "react-native";
import { Text } from "@merge/uim/components/text";
import { ProductCard, ProductCardProps } from "./product-card";

interface ProductSectionProps {
  title: string;
  products: ProductCardProps[];
  onSeeAll?: () => void;
  isLast?: boolean;
}

export function ProductSection({ title, products, onSeeAll, isLast }: ProductSectionProps) {
  return (
    <View style={{ marginBottom: isLast ? 0 : 24 }}>
      {/* Section Header */}
      <View className="flex-row items-center justify-between px-4 mb-4">
        <Text className="text-lg font-bold uppercase tracking-tight text-foreground dark:text-white">
          {title}
        </Text>
        {onSeeAll && (
          <Pressable
            onPress={onSeeAll}
            className="flex-row items-center"
            style={{ gap: 4 }}>
            <Text className="text-xs font-medium uppercase tracking-wide text-foreground/60 dark:text-white/60">
              Tumunu Gor
            </Text>
            <Text className="text-xs font-medium text-foreground/60 dark:text-white/60">
              {">"}
            </Text>
          </Pressable>
        )}
      </View>

      {/* Products Scroll */}
      <View style={{ height: 240 }}>
        <ScrollView
          horizontal
          nestedScrollEnabled={true}
          showsHorizontalScrollIndicator={false}
          scrollEnabled={true}
          bounces={false}
          directionalLockEnabled={true}
          contentContainerStyle={{ paddingHorizontal: 16 }}>
          {products.map((product, index) => (
            <ProductCard
              key={product.id}
              {...product}
              style={{ marginRight: index < products.length - 1 ? 12 : 0 }}
              onPress={() => {
                product.onPress?.();
              }}
            />
          ))}
        </ScrollView>
      </View>
    </View>
  );
}
