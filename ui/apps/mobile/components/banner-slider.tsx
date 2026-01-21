import { View, ScrollView, Dimensions, Image, TouchableOpacity, Pressable } from "react-native";
import { useRef, useState, useEffect, useCallback } from "react";
import { Text } from "@merge/uim/components/text";
import { LinearGradient } from "expo-linear-gradient";

const { width: SCREEN_WIDTH } = Dimensions.get("window");
const BANNER_WIDTH = SCREEN_WIDTH - 32;
const BANNER_HEIGHT = 180;
const AUTO_SCROLL_INTERVAL = 4000;

interface Banner {
  id: string;
  imageUrl: string;
  title: string;
  subtitle?: string;
}

const mockBanners: Banner[] = [
  {
    id: "1",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuDRdh2mvhqicHpbdbkp3QYD6lGxPCZa45GDzqJIB-l1d4FHNRz0sRo_Yd2hJPpzqkS7lDNTlMWf2Uk7lM9cJcVKbRQHgpBqrDn47cYYfZ-0hYjJkwJzPjZJo_qQT-BRWBq8vKm5L_qR",
    title: "YENI SEZON",
    subtitle: "%50'ye varan indirimler",
  },
  {
    id: "2",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
    title: "FLASH SALE",
    subtitle: "Sinirli sure, sinirli stok",
  },
  {
    id: "3",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
    title: "PREMIUM KOLEKSIYON",
    subtitle: "En iyi firsatlar burada",
  },
];

export function BannerSlider() {
  const scrollViewRef = useRef<ScrollView>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isUserScrolling, setIsUserScrolling] = useState(false);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const scrollToIndex = useCallback((index: number) => {
    scrollViewRef.current?.scrollTo({
      x: index * (BANNER_WIDTH + 12),
      animated: true,
    });
    setCurrentIndex(index);
  }, []);

  // Auto-scroll
  useEffect(() => {
    if (isUserScrolling) return;

    timerRef.current = setInterval(() => {
      const nextIndex = (currentIndex + 1) % mockBanners.length;
      scrollToIndex(nextIndex);
    }, AUTO_SCROLL_INTERVAL);

    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, [currentIndex, isUserScrolling, scrollToIndex]);

  const handleScrollBegin = () => {
    setIsUserScrolling(true);
    if (timerRef.current) clearInterval(timerRef.current);
  };

  const handleScrollEnd = (event: any) => {
    const offsetX = event.nativeEvent.contentOffset.x;
    const index = Math.round(offsetX / (BANNER_WIDTH + 12));
    setCurrentIndex(index);

    // Resume auto-scroll after user interaction
    setTimeout(() => {
      setIsUserScrolling(false);
    }, 2000);
  };

  const handleIndicatorPress = (index: number) => {
    scrollToIndex(index);
    setIsUserScrolling(true);
    setTimeout(() => {
      setIsUserScrolling(false);
    }, 2000);
  };

  const handleBannerPress = (banner: Banner) => {
    // Banner tıklandığında yapılacak işlem
    console.log("Banner pressed:", banner.id);
  };

  return (
    <View style={{ paddingVertical: 16 }}>
      <ScrollView
        ref={scrollViewRef}
        horizontal
        nestedScrollEnabled={true}
        pagingEnabled={false}
        snapToInterval={BANNER_WIDTH + 12}
        decelerationRate="fast"
        showsHorizontalScrollIndicator={false}
        scrollEnabled={true}
        bounces={false}
        directionalLockEnabled={true}
        onScrollBeginDrag={handleScrollBegin}
        onMomentumScrollEnd={handleScrollEnd}
        scrollEventThrottle={16}
        contentContainerStyle={{ paddingHorizontal: 16 }}>
        {mockBanners.map((banner, index) => (
          <TouchableOpacity
            key={banner.id}
            activeOpacity={0.9}
            delayPressIn={50}
            onPress={() => handleBannerPress(banner)}
            style={{
              width: BANNER_WIDTH,
              height: BANNER_HEIGHT,
              marginRight: index < mockBanners.length - 1 ? 12 : 0,
            }}>
            <View
              style={{
                width: "100%",
                height: "100%",
                borderRadius: 16,
                backgroundColor: "#111118",
                overflow: "hidden",
              }}>
              <Image
                source={{ uri: banner.imageUrl }}
                style={{
                  width: BANNER_WIDTH,
                  height: BANNER_HEIGHT,
                  position: "absolute",
                }}
                resizeMode="cover"
              />
              <LinearGradient
                colors={[
                  "rgba(0, 0, 0, 0)",
                  "rgba(0, 0, 0, 0.3)",
                  "rgba(0, 0, 0, 0.8)",
                ]}
                locations={[0, 0.5, 1]}
                style={{
                  position: "absolute",
                  left: 0,
                  right: 0,
                  top: 0,
                  bottom: 0,
                }}
              />
              <View
                style={{
                  position: "absolute",
                  bottom: 0,
                  left: 0,
                  right: 0,
                  padding: 16,
                  gap: 4,
                }}>
                <Text style={{ fontSize: 20, fontWeight: "700", color: "#fff", textTransform: "uppercase", letterSpacing: 0.5 }}>
                  {banner.title}
                </Text>
                {banner.subtitle && (
                  <Text style={{ fontSize: 14, fontWeight: "300", color: "rgba(255,255,255,0.8)" }}>
                    {banner.subtitle}
                  </Text>
                )}
              </View>
            </View>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Page Indicators */}
      <View style={{ flexDirection: "row", alignItems: "center", justifyContent: "center", marginTop: 12, gap: 8 }}>
        {mockBanners.map((_, index) => (
          <Pressable
            key={index}
            onPress={() => handleIndicatorPress(index)}
            hitSlop={{ top: 10, bottom: 10, left: 5, right: 5 }}>
            <View
              style={{
                width: index === currentIndex ? 24 : 8,
                height: 8,
                borderRadius: 4,
                backgroundColor: index === currentIndex ? "#111118" : "#E0E0E0",
              }}
            />
          </Pressable>
        ))}
      </View>
    </View>
  );
}
