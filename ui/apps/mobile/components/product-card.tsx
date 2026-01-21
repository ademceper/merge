import { View, Pressable, Image, ViewStyle } from "react-native";
import { Text } from "@merge/uim/components/text";

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
  style?: ViewStyle;
}

// Mock product images
const mockProductImages = [
  "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
  "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
  "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4",
  "https://lh3.googleusercontent.com/aida-public/AB6AXuDRdh2mvhqicHpbdbkp3QYD6lGxPCZa45GDzqJIB-l1d4FHNRz0sRo_Yd2hJPpzqkS7lDNTlMWf2Uk7lM9cJcVKbRQHgpBqrDn47cYYfZ-0hYjJkwJzPjZJo_qQT-BRWBq8vKm5L_qR",
];

function getProductImage(id: string): string {
  const index = parseInt(id, 10) % mockProductImages.length;
  return mockProductImages[index];
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
  style,
}: ProductCardProps) {
  const hasDiscount = discount && discount > 0;
  const discountPercentage = hasDiscount ? Math.round(discount) : 0;
  const productImage = imageUrl || getProductImage(id);

  return (
    <Pressable
      onPress={onPress}
      style={[{ width: 150 }, style]}>
      <View className="relative mb-2">
        {/* Product Image */}
        <View
          style={{
            width: 150,
            height: 150,
            borderRadius: 12,
            overflow: "hidden",
            backgroundColor: "#000",
          }}>
          <Image
            source={{ uri: productImage }}
            style={{
              width: "100%",
              height: "100%",
              opacity: 0.9,
            }}
            resizeMode="cover"
          />
        </View>

        {/* Badge */}
        {badge && (
          <View
            className="absolute top-2 left-2"
            style={{
              backgroundColor: "#111118",
              paddingHorizontal: 8,
              paddingVertical: 4,
              borderRadius: 4,
            }}>
            <Text className="text-[10px] font-bold uppercase tracking-wide text-white">
              {badge}
            </Text>
          </View>
        )}

        {/* Discount Badge */}
        {hasDiscount && (
          <View
            className="absolute top-2 right-2"
            style={{
              backgroundColor: "#111118",
              paddingHorizontal: 8,
              paddingVertical: 4,
              borderRadius: 4,
            }}>
            <Text className="text-[10px] font-bold text-white">
              %{discountPercentage}
            </Text>
          </View>
        )}
      </View>

      {/* Product Info */}
      <View style={{ gap: 4 }}>
        <Text
          className="text-sm font-medium text-foreground dark:text-white"
          numberOfLines={2}>
          {name}
        </Text>

        <View className="flex-row items-center" style={{ gap: 6 }}>
          <Text className="text-base font-bold text-foreground dark:text-white">
            {price.toLocaleString("tr-TR")} TL
          </Text>
          {originalPrice && originalPrice > price && (
            <Text className="text-xs text-foreground/40 dark:text-white/40 line-through">
              {originalPrice.toLocaleString("tr-TR")} TL
            </Text>
          )}
        </View>
      </View>
    </Pressable>
  );
}
