import { View, Pressable, Image } from "react-native";
import { Text } from "@merge/uim/components/text";
import { Badge } from "@merge/uim/components/badge";
import { cn } from "@merge/uim/lib/utils";

export interface ProductCardProps {
  id: string;
  name: string;
  price: number;
  originalPrice?: number;
  discount?: number;
  imageUrl?: string;
  badge?: string;
  onPress?: () => void;
  className?: string;
}

export function ProductCard({
  id,
  name,
  price,
  originalPrice,
  discount,
  imageUrl,
  badge,
  onPress,
  className,
}: ProductCardProps) {
  const hasDiscount = discount && discount > 0;
  const discountPercentage = hasDiscount ? Math.round(discount) : 0;

  return (
    <Pressable
      onPress={onPress}
      className={cn("w-40", className)}
      style={{ marginRight: 12 }}>
      <View className="relative mb-2">
        {/* Product Image */}
        <View className="w-full h-40 rounded-xl border border-primary/10 dark:border-white/10 bg-white dark:bg-white/5 overflow-hidden">
          {imageUrl ? (
            <Image
              source={{ uri: imageUrl }}
              className="w-full h-full"
              resizeMode="cover"
            />
          ) : (
            <View className="w-full h-full items-center justify-end pb-2">
              <Text className="text-[10px] text-muted-foreground/60 dark:text-white/40">
                Ürün Görseli
              </Text>
            </View>
          )}
        </View>

        {/* Badge */}
        {badge && (
          <View className="absolute top-2 left-2">
            <Badge variant="destructive" className="text-xs font-bold">
              {badge}
            </Badge>
          </View>
        )}

        {/* Discount Badge */}
        {hasDiscount && (
          <View className="absolute top-2 right-2">
            <Badge variant="destructive" className="text-xs font-bold">
              %{discountPercentage}
            </Badge>
          </View>
        )}
      </View>

      {/* Product Info */}
      <View className="gap-1">
        <Text
          className="text-sm font-medium text-foreground"
          numberOfLines={2}>
          {name}
        </Text>

        <View className="flex-row items-center gap-2">
          <Text className="text-base font-bold text-foreground">
            {price.toLocaleString("tr-TR")} ₺
          </Text>
          {originalPrice && originalPrice > price && (
            <Text className="text-xs text-muted-foreground line-through">
              {originalPrice.toLocaleString("tr-TR")} ₺
            </Text>
          )}
        </View>
      </View>
    </Pressable>
  );
}
