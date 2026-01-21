import { View, ScrollView, Pressable, Image } from "react-native";
import { Text } from "@merge/uim/components/text";
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
        <View className="flex-row items-center" style={{ gap: 8 }}>
          <View
            style={{
              width: 8,
              height: 8,
              borderRadius: 4,
              backgroundColor: "#FF0000",
            }}
          />
          <Text className="text-lg font-bold uppercase tracking-tight text-foreground dark:text-white">
            Canli Yayinlar
          </Text>
        </View>
        <Pressable>
          <Text className="text-xs font-medium uppercase tracking-wide text-foreground/60 dark:text-white/60">
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
            <View
              style={{
                width: 160,
                height: 200,
                borderRadius: 12,
                overflow: "hidden",
                backgroundColor: "#000",
              }}>
              <Image
                source={{ uri: stream.thumbnailUrl }}
                style={{ width: "100%", height: "100%" }}
                resizeMode="cover"
              />

              {/* Gradient overlay */}
              <LinearGradient
                colors={["transparent", "rgba(0,0,0,0.8)"]}
                style={{
                  position: "absolute",
                  left: 0,
                  right: 0,
                  bottom: 0,
                  height: "50%",
                }}
              />

              {/* Live badge */}
              <View
                style={{
                  position: "absolute",
                  top: 8,
                  left: 8,
                  flexDirection: "row",
                  alignItems: "center",
                  gap: 4,
                  backgroundColor: "#FF0000",
                  paddingHorizontal: 8,
                  paddingVertical: 4,
                  borderRadius: 4,
                }}>
                <View
                  style={{
                    width: 6,
                    height: 6,
                    borderRadius: 3,
                    backgroundColor: "#fff",
                  }}
                />
                <Text className="text-white text-[10px] font-bold uppercase">
                  Canli
                </Text>
              </View>

              {/* Viewer count */}
              <View
                style={{
                  position: "absolute",
                  top: 8,
                  right: 8,
                  flexDirection: "row",
                  alignItems: "center",
                  gap: 4,
                  backgroundColor: "rgba(0,0,0,0.6)",
                  paddingHorizontal: 6,
                  paddingVertical: 3,
                  borderRadius: 4,
                }}>
                <Icon as={Users} size={10} className="text-white" />
                <Text className="text-white text-[10px] font-medium">
                  {stream.viewerCount.toLocaleString()}
                </Text>
              </View>

              {/* Play button */}
              <View
                style={{
                  position: "absolute",
                  top: "50%",
                  left: "50%",
                  transform: [{ translateX: -20 }, { translateY: -20 }],
                  width: 40,
                  height: 40,
                  borderRadius: 20,
                  backgroundColor: "rgba(255,255,255,0.9)",
                  alignItems: "center",
                  justifyContent: "center",
                }}>
                <Icon as={Play} size={20} className="text-black" fill="#000" />
              </View>

              {/* Host info */}
              <View
                style={{
                  position: "absolute",
                  bottom: 8,
                  left: 8,
                  right: 8,
                }}>
                <View className="flex-row items-center" style={{ gap: 6, marginBottom: 4 }}>
                  <Image
                    source={{ uri: stream.hostAvatar }}
                    style={{
                      width: 20,
                      height: 20,
                      borderRadius: 10,
                      borderWidth: 1,
                      borderColor: "#fff",
                    }}
                  />
                  <Text className="text-white text-xs font-medium">
                    {stream.hostName}
                  </Text>
                </View>
                <Text className="text-white text-xs" numberOfLines={1}>
                  {stream.title}
                </Text>
              </View>
            </View>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}
