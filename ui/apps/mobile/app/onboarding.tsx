import { View, useColorScheme, Dimensions, Pressable, Image } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { StatusBar } from "expo-status-bar";
import { useRouter } from "expo-router";
import * as React from "react";
import Animated, {
  useAnimatedStyle,
  useSharedValue,
  withTiming,
  withSpring,
  runOnJS,
} from "react-native-reanimated";
import {
  Gesture,
  GestureDetector,
  GestureHandlerRootView,
} from "react-native-gesture-handler";
import { Text } from "@merge/uim/components/text";
import { LinearGradient } from "expo-linear-gradient";

const { width: SCREEN_WIDTH, height: SCREEN_HEIGHT } = Dimensions.get("window");
const SWIPE_THRESHOLD = SCREEN_WIDTH * 0.3;
const SWIPE_VELOCITY_THRESHOLD = 1000;

const ONBOARDING_PAGES = [
  {
    id: 2,
    title: "SONIC: A NEW STANDARD",
    subtitle: "Where innovative engineering meets minimalist elegance. Prepare for an unparalleled experience.",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
    buttonText: "Next",
    isLastPage: false,
    hasSkip: true,
    activeIndicatorIndex: 0,
    imageContrast: 175,
  },
  {
    id: 3,
    title: "UNRIVALED CRAFTSMANSHIP",
    subtitle: "Explore devices meticulously engineered for superior performance and timeless aesthetics.",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
    buttonText: "Next",
    isLastPage: false,
    hasSkip: true,
    activeIndicatorIndex: 1,
    imageContrast: 125,
  },
  {
    id: 1,
    title: "Begin Your Journey",
    subtitle: "Your personalized world of premium audio awaits. Tap 'Enter' to explore.",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4",
    buttonText: "Enter App",
    isLastPage: true,
    hasSkip: false,
    activeIndicatorIndex: 2,
    imageContrast: 125,
  },
];

export default function OnboardingScreen() {
  const colorScheme = useColorScheme();
  const insets = useSafeAreaInsets();
  const router = useRouter();
  const [currentPage, setCurrentPage] = React.useState(0);
  const translateX = useSharedValue(0);
  const startX = useSharedValue(0);

  const isDark = colorScheme === "dark";
  const totalPages = ONBOARDING_PAGES.length;
  const currentPageData = ONBOARDING_PAGES[currentPage];
  
  const TOP_SECTION_HEIGHT = SCREEN_HEIGHT * 0.6;

  const panGesture = Gesture.Pan()
    .activeOffsetX([-10, 10])
    .failOffsetY([-10, 10])
    .onStart((e) => {
      startX.value = translateX.value;
    })
    .onUpdate((e) => {
      const newTranslateX = startX.value + e.translationX;
      
      if (currentPage === 0 && newTranslateX > 0) {
        translateX.value = newTranslateX * 0.3;
      } else if (currentPage === totalPages - 1 && newTranslateX < -(totalPages - 1) * SCREEN_WIDTH) {
        const maxTranslate = -(totalPages - 1) * SCREEN_WIDTH;
        const overflow = newTranslateX - maxTranslate;
        translateX.value = maxTranslate + overflow * 0.3;
      } else {
        translateX.value = newTranslateX;
      }
    })
    .onEnd((e) => {
      const velocity = e.velocityX;
      const deltaX = e.translationX;
      
      let targetPage = currentPage;
      
      const adjustedThreshold = SWIPE_THRESHOLD * 1.2;
      const adjustedVelocity = SWIPE_VELOCITY_THRESHOLD * 1.3;
      
      if (Math.abs(deltaX) > adjustedThreshold || Math.abs(velocity) > adjustedVelocity) {
        if (deltaX < 0 || velocity < 0) {
          targetPage = Math.min(currentPage + 1, totalPages - 1);
        } else {
          targetPage = Math.max(currentPage - 1, 0);
        }
      }
      
      translateX.value = withSpring(-targetPage * SCREEN_WIDTH, {
        damping: 22,
        stiffness: 100,
        mass: 0.4,
      });
      
      runOnJS(setCurrentPage)(targetPage);
    });

  const pageStyle = (index: number) =>
    useAnimatedStyle(() => {
      const pageTranslateX = translateX.value + index * SCREEN_WIDTH;
      
      return {
        transform: [{ translateX: pageTranslateX }],
      };
    });

  const handleSkip = () => {
    router.replace("/home" as any);
  };

  const handleButtonPress = () => {
    if (currentPageData.isLastPage) {
      router.replace("/home" as any);
    } else {
      const nextPage = currentPage + 1;
      translateX.value = withSpring(-nextPage * SCREEN_WIDTH, {
        damping: 18,
        stiffness: 80,
        mass: 0.5,
      });
      setCurrentPage(nextPage);
    }
  };

  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <View
        className={`flex-1 ${isDark ? "bg-[#101022]" : "bg-white"}`}
        style={{ paddingTop: 0, paddingBottom: 0 }}>
        <StatusBar style="light" />

        <GestureDetector gesture={panGesture}>
          <View className="flex-1" style={{ overflow: "hidden" }}>
            {ONBOARDING_PAGES.map((page, index) => (
              <Animated.View
                key={page.id}
                style={[
                  {
                    position: "absolute",
                    width: SCREEN_WIDTH,
                    height: "100%",
                  },
                  pageStyle(index),
                ]}>
                <View className="flex-1" style={{ paddingBottom: insets.bottom }}>
                  {page.hasSkip && (
                    <View 
                      className="absolute top-0 right-0 z-20 flex-row justify-end" 
                      style={{ 
                        top: insets.top + 24,
                        right: 24,
                        width: "100%",
                      }}>
                      <Pressable
                        onPress={handleSkip}
                        className="px-4 py-2"
                        hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}>
                        <Text 
                          className={`text-sm font-medium uppercase tracking-wide ${
                            index === 0 ? "text-white/60" : "text-white/80"
                          }`}>
                          Skip
                        </Text>
                      </Pressable>
                    </View>
                  )}

                  <View
                    style={{
                      height: TOP_SECTION_HEIGHT,
                      backgroundColor: "#000000",
                      overflow: "hidden",
                      borderBottomLeftRadius: 32,
                      borderBottomRightRadius: 32,
                    }}>
                    <Image
                      source={{ uri: page.imageUrl }}
                      style={{
                        width: "100%",
                        height: "100%",
                        opacity: 0.9,
                        // Grayscale ve contrast efektleri React Native'de doğrudan yok
                        // Ancak görsel olarak benzer bir efekt için opacity kullanıyoruz
                      }}
                      resizeMode="cover"
                    />
                    <LinearGradient
                      colors={[
                        "rgba(0, 0, 0, 0.2)",
                        "transparent",
                        "rgba(0, 0, 0, 0.1)",
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
                  </View>

                  <View 
                    className="flex-1 flex-col justify-between px-8"
                    style={{ 
                      paddingTop: 40,
                      paddingBottom: Math.max(insets.bottom, 40),
                    }}>
                    <View className="items-center" style={{ gap: 16 }}>
                      <Text
                        className={`text-3xl font-bold text-center uppercase tracking-tight leading-none ${
                          isDark ? "text-white" : "text-[#111118]"
                        }`}
                        style={{ maxWidth: 320 }}
                        numberOfLines={2}>
                        {page.title}
                      </Text>
                      <Text
                        className={`text-base font-light text-center leading-relaxed ${
                          isDark ? "text-white/70" : "text-[#111118]/70"
                        }`}
                        style={{ maxWidth: 320 }}
                        numberOfLines={3}>
                        {page.subtitle}
                      </Text>
                    </View>

                    <View 
                      className="flex-col items-center w-full" 
                      style={{ 
                        gap: 32,
                      }}>
                      <View className="flex-row items-center justify-center" style={{ gap: 12 }}>
                        {ONBOARDING_PAGES.map((_, idx) => {
                          const isActive = idx === currentPage;
                          return (
                            <View
                              key={idx}
                              style={{
                                width: 10,
                                height: 10,
                                borderRadius: 5,
                                borderWidth: isActive ? 0 : 1,
                                borderColor: isDark
                                  ? "rgba(255, 255, 255, 0.3)"
                                  : "rgba(17, 17, 24, 0.3)",
                                backgroundColor: isActive
                                  ? isDark
                                    ? "#FFFFFF"
                                    : "#111118"
                                  : "transparent",
                              }}
                            />
                          );
                        })}
                      </View>

                      <Pressable
                        onPress={handleButtonPress}
                        className={`w-full h-14 rounded-xl flex-row items-center justify-center ${
                          isDark ? "bg-white" : "bg-[#111118]"
                        }`}
                        style={{ gap: 8 }}
                      >
                        <Text
                          className={`text-base font-bold uppercase tracking-wider ${
                            isDark ? "text-[#111118]" : "text-white"
                          }`}>
                          {page.buttonText}
                        </Text>
                        {!page.isLastPage && (
                          <Text
                            className={`text-lg ${
                              isDark ? "text-[#111118]" : "text-white"
                            }`}>
                            →
                          </Text>
                        )}
                      </Pressable>
                    </View>
                  </View>
                </View>
              </Animated.View>
            ))}
          </View>
        </GestureDetector>
      </View>
    </GestureHandlerRootView>
  );
}
