import { View, ScrollView, Pressable, Image } from "react-native";
import { Text } from "@merge/uim/components/text";
import { Card } from "@merge/uim/components/card";
import { Badge } from "@merge/uim/components/badge";
import { Play, Users } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";
import { LinearGradient } from "expo-linear-gradient";

interface LiveStream {
  id: string;
  title: string;
  hostName: string;
  hostAvatar: string;
  thumbnailUrl: string;
  viewerCount: number;
  isLive: boolean;
}

const liveStreams: LiveStream[] = [
  {
    id: "1",
    title: "Teknoloji Urunleri Tanitimi",
    hostName: "TechStore",
    hostAvatar: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
    thumbnailUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
    viewerCount: 1247,
    isLive: true,
  },
  {
    id: "2",
    title: "Moda Trendleri 2024",
    hostName: "FashionHub",
    hostAvatar: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
    thumbnailUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
    viewerCount: 892,
    isLive: true,
  },
  {
    id: "3",
    title: "Ev Dekorasyon Fikirleri",
    hostName: "HomeDesign",
    hostAvatar: "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4",
    thumbnailUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4",
    viewerCount: 534,
    isLive: true,
  },
];

export function LiveShopping() {
  return (
    <View className="mb-4">
      <View className="flex-row items-center justify-between px-4 mb-3">
        <View className="flex-row items-center gap-2">
          <View className="w-2 h-2 rounded-full bg-destructive" />
          <Text className="text-lg font-bold uppercase tracking-tight text-foreground">
            Canli Yayinlar
          </Text>
        </View>
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
        {liveStreams.map((stream) => (
          <Pressable
            key={stream.id}
            style={{
              width: 160,
            }}>
            <Card className="w-[160px] h-[200px] rounded-xl overflow-hidden bg-primary">
              <Image
                source={{ uri: stream.thumbnailUrl }}
                style={{ width: "100%", height: "100%" }}
                resizeMode="cover"
              />

              {/* Gradient overlay */}
              <LinearGradient
                colors={["transparent", "rgba(0,0,0,0.8)"]}
                className="absolute left-0 right-0 bottom-0 h-1/2"
              />

              {/* Live badge */}
              <Badge className="absolute top-2 left-2 bg-destructive flex-row items-center gap-1 px-2 py-1">
                <View className="w-1.5 h-1.5 rounded-full bg-primary-foreground" />
                <Text className="text-primary-foreground text-[10px] font-bold uppercase">
                  Canli
                </Text>
              </Badge>

              {/* Viewer count */}
              <View className="absolute top-2 right-2 flex-row items-center gap-1 bg-primary/60 px-1.5 py-1 rounded">
                <Icon as={Users} size={10} className="text-primary-foreground" />
                <Text className="text-primary-foreground text-[10px] font-medium">
                  {stream.viewerCount.toLocaleString()}
                </Text>
              </View>

              {/* Play button */}
              <View className="absolute top-1/2 left-1/2 -translate-x-5 -translate-y-5 w-10 h-10 rounded-full bg-primary-foreground/90 items-center justify-center">
                <Icon as={Play} size={20} className="text-primary" fill="currentColor" />
              </View>

              {/* Host info */}
              <View className="absolute bottom-2 left-2 right-2">
                <View className="flex-row items-center gap-1.5 mb-1">
                  <Image
                    source={{ uri: stream.hostAvatar }}
                    className="w-5 h-5 rounded-full border border-primary-foreground"
                  />
                  <Text className="text-primary-foreground text-xs font-medium">
                    {stream.hostName}
                  </Text>
                </View>
                <Text className="text-primary-foreground text-xs" numberOfLines={1}>
                  {stream.title}
                </Text>
              </View>
            </Card>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}
