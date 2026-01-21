import { View, ScrollView, Pressable, Image } from "react-native";
import { Text } from "@merge/uim/components/text";
import { Card } from "@merge/uim/components/card";
import { Badge } from "@merge/uim/components/badge";

interface Brand {
  id: string;
  name: string;
  logoUrl: string;
  discount?: string;
}

const brands: Brand[] = [
  {
    id: "1",
    name: "Apple",
    logoUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
    discount: "%15",
  },
  {
    id: "2",
    name: "Samsung",
    logoUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
    discount: "%20",
  },
  {
    id: "3",
    name: "Nike",
    logoUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4",
    discount: "%30",
  },
  {
    id: "4",
    name: "Adidas",
    logoUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuDRdh2mvhqicHpbdbkp3QYD6lGxPCZa45GDzqJIB-l1d4FHNRz0sRo_Yd2hJPpzqkS7lDNTlMWf2Uk7lM9cJcVKbRQHgpBqrDn47cYYfZ-0hYjJkwJzPjZJo_qQT-BRWBq8vKm5L_qR",
    discount: "%25",
  },
  {
    id: "5",
    name: "Sony",
    logoUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
  },
  {
    id: "6",
    name: "LG",
    logoUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
    discount: "%10",
  },
];

export function BrandsCarousel() {
  return (
    <View className="mb-4">
      <View className="flex-row items-center justify-between px-4 mb-3">
        <Text className="text-lg font-bold uppercase tracking-tight text-foreground">
          Populer Markalar
        </Text>
        <Pressable>
          <Text className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            Tumunu Gor {">"}
          </Text>
        </Pressable>
      </View>

      <ScrollView
        horizontal
        nestedScrollEnabled={true}
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={{ paddingHorizontal: 16, gap: 12 }}>
        {brands.map((brand) => (
          <Pressable
            key={brand.id}
            style={{
              width: 90,
              alignItems: "center",
            }}>
            <Card className="w-20 h-20 rounded-xl overflow-hidden">
              <Image
                source={{ uri: brand.logoUrl }}
                style={{
                  width: "100%",
                  height: "100%",
                }}
                resizeMode="cover"
              />
              {brand.discount && (
                <View className="absolute bottom-0 left-0 right-0 bg-primary py-1 items-center">
                  <Text className="text-primary-foreground text-[10px] font-bold">
                    {brand.discount} INDIRIM
                  </Text>
                </View>
              )}
            </Card>
            <Text
              className="text-xs font-medium text-foreground/80 mt-2 text-center"
              numberOfLines={1}>
              {brand.name}
            </Text>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}
