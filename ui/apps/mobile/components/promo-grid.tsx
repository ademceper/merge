import { View, Pressable, Image, Dimensions } from "react-native";
import { Text } from "@merge/uim/components/text";
import { LinearGradient } from "expo-linear-gradient";

const { width: SCREEN_WIDTH } = Dimensions.get("window");
const GRID_GAP = 10;
const GRID_PADDING = 16;
const ITEM_WIDTH = (SCREEN_WIDTH - GRID_PADDING * 2 - GRID_GAP) / 2;

interface PromoItem {
  id: string;
  title: string;
  subtitle: string;
  imageUrl: string;
}

const promoItems: PromoItem[] = [
  {
    id: "1",
    title: "SUPER FIRSAT",
    subtitle: "%70'e varan indirim",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
  },
  {
    id: "2",
    title: "UCRETSIZ KARGO",
    subtitle: "100 TL ustu",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
  },
  {
    id: "3",
    title: "OUTLET",
    subtitle: "Secili urunlerde",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4",
  },
  {
    id: "4",
    title: "YENI URUNLER",
    subtitle: "Haftanin yenileri",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuDRdh2mvhqicHpbdbkp3QYD6lGxPCZa45GDzqJIB-l1d4FHNRz0sRo_Yd2hJPpzqkS7lDNTlMWf2Uk7lM9cJcVKbRQHgpBqrDn47cYYfZ-0hYjJkwJzPjZJo_qQT-BRWBq8vKm5L_qR",
  },
];

export function PromoGrid() {
  return (
    <View className="px-4 mb-5">
      <View className="flex-row flex-wrap gap-2.5">
        {promoItems.map((item) => (
          <Pressable
            key={item.id}
            style={{
              width: ITEM_WIDTH,
              height: 110,
            }}
            className="rounded-2xl overflow-hidden">
            <Image
              source={{ uri: item.imageUrl }}
              style={{
                position: "absolute",
                width: "100%",
                height: "100%",
              }}
              resizeMode="cover"
            />
            <LinearGradient
              colors={["transparent", "rgba(0,0,0,0.7)"]}
              className="flex-1 justify-end p-3">
              <Text className="text-primary-foreground text-[13px] font-bold uppercase tracking-wide">
                {item.title}
              </Text>
              <Text className="text-primary-foreground/80 text-[11px] mt-0.5">
                {item.subtitle}
              </Text>
            </LinearGradient>
          </Pressable>
        ))}
      </View>
    </View>
  );
}
