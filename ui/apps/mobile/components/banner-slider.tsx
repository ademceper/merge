import { View, ScrollView, Dimensions, Image } from "react-native";
import { useRef, useState, useEffect } from "react";
import { Text } from "@merge/uim/components/text";

const { width: SCREEN_WIDTH } = Dimensions.get("window");
const BANNER_WIDTH = SCREEN_WIDTH - 32; // 16px padding on each side

interface Banner {
  id: string;
  imageUrl?: string;
  title?: string;
  subtitle?: string;
}

const mockBanners: Banner[] = [
  {
    id: "1",
    title: "YENI SEZON KOLEKSIYONU",
    subtitle: "Tüm ürünlerde %50'ye varan indirimler",
  },
  {
    id: "2",
    title: "FLASH SALE",
    subtitle: "Sınırlı süre, sınırlı stok",
  },
  {
    id: "3",
    title: "KAMPANYALI ÜRÜNLER",
    subtitle: "En iyi fırsatlar burada",
  },
];

export function BannerSlider() {
  const scrollViewRef = useRef<ScrollView>(null);
  const [currentIndex, setCurrentIndex] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      const nextIndex = (currentIndex + 1) % mockBanners.length;
      scrollViewRef.current?.scrollTo({
        x: nextIndex * BANNER_WIDTH,
        animated: true,
      });
      setCurrentIndex(nextIndex);
    }, 5000);

    return () => clearInterval(interval);
  }, [currentIndex]);

  const handleScroll = (event: any) => {
    const offsetX = event.nativeEvent.contentOffset.x;
    const index = Math.round(offsetX / BANNER_WIDTH);
    setCurrentIndex(index);
  };

  return (
    <View className="px-4 py-3">
      <ScrollView
        ref={scrollViewRef}
        horizontal
        nestedScrollEnabled={true}
        pagingEnabled
        showsHorizontalScrollIndicator={false}
        scrollEnabled={true}
        bounces={false}
        directionalLockEnabled={true}
        onScroll={handleScroll}
        scrollEventThrottle={16}>
        {mockBanners.map((banner) => (
          <View
            key={banner.id}
            className="rounded-xl border border-primary/10 dark:border-white/10 bg-white dark:bg-white/5 overflow-hidden"
            style={{ width: BANNER_WIDTH, marginRight: 16 }}>
            <View className="h-32 items-center justify-center p-6">
              {banner.imageUrl ? (
                <Image
                  source={{ uri: banner.imageUrl }}
                  className="w-full h-full"
                  resizeMode="cover"
                />
              ) : (
                <View className="items-center justify-center gap-2">
                  <Text className="text-xl font-bold uppercase tracking-tight text-foreground dark:text-white">
                    {banner.title}
                  </Text>
                  {banner.subtitle && (
                    <Text className="text-sm text-muted-foreground dark:text-white/70 text-center">
                      {banner.subtitle}
                    </Text>
                  )}
                </View>
              )}
            </View>
          </View>
        ))}
      </ScrollView>

      {/* Page Indicators */}
      <View className="flex-row items-center justify-center gap-2 mt-2">
        {mockBanners.map((_, index) => (
          <View
            key={index}
            className={`h-2 w-2 rounded-full ${
              index === currentIndex
                ? "bg-foreground dark:bg-white"
                : "border border-foreground/30 dark:border-white/30"
            }`}
          />
        ))}
      </View>
    </View>
  );
}
