import { View, ScrollView, Pressable, Image } from "react-native";
import { Text } from "@merge/uim/components/text";
import { Clock } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";

interface RecentProduct {
  id: string;
  name: string;
  price: number;
  imageUrl: string;
}

const recentProducts: RecentProduct[] = [
  {
    id: "r1",
    name: "Kablosuz Kulaklik",
    price: 899,
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
  },
  {
    id: "r2",
    name: "Akilli Saat",
    price: 2499,
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
  },
  {
    id: "r3",
    name: "Spor Ayakkabi",
    price: 1299,
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4",
  },
  {
    id: "r4",
    name: "Laptop",
    price: 24999,
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuDRdh2mvhqicHpbdbkp3QYD6lGxPCZa45GDzqJIB-l1d4FHNRz0sRo_Yd2hJPpzqkS7lDNTlMWf2Uk7lM9cJcVKbRQHgpBqrDn47cYYfZ-0hYjJkwJzPjZJo_qQT-BRWBq8vKm5L_qR",
  },
  {
    id: "r5",
    name: "Tablet",
    price: 8999,
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
  },
];

export function RecentlyViewed() {
  return (
    <View className="mb-4">
      <View className="flex-row items-center justify-between px-4 mb-3">
        <View className="flex-row items-center" style={{ gap: 8 }}>
          <Icon as={Clock} size={18} className="text-foreground/60 dark:text-white/60" />
          <Text className="text-lg font-bold uppercase tracking-tight text-foreground dark:text-white">
            Son Gorilenler
          </Text>
        </View>
        <Pressable>
          <Text className="text-xs font-medium uppercase tracking-wide text-foreground/60 dark:text-white/60">
            Temizle
          </Text>
        </Pressable>
      </View>

      <ScrollView
        horizontal
        nestedScrollEnabled={true}
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={{ paddingHorizontal: 16, gap: 12 }}>
        {recentProducts.map((product) => (
          <Pressable
            key={product.id}
            style={{
              width: 100,
            }}>
            <View
              style={{
                width: 100,
                height: 100,
                borderRadius: 12,
                overflow: "hidden",
                backgroundColor: "#f5f5f5",
                marginBottom: 8,
              }}>
              <Image
                source={{ uri: product.imageUrl }}
                style={{ width: "100%", height: "100%" }}
                resizeMode="cover"
              />
            </View>
            <Text
              className="text-xs font-medium text-foreground/80 dark:text-white/80"
              numberOfLines={1}>
              {product.name}
            </Text>
            <Text className="text-sm font-bold text-foreground dark:text-white">
              {product.price.toLocaleString("tr-TR")} TL
            </Text>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}
