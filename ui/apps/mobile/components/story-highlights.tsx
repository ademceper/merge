import { View, ScrollView, Pressable, Image, Modal, Dimensions, TouchableOpacity } from "react-native";
import { Text } from "@merge/uim/components/text";
import { useState, useEffect, useRef } from "react";
import { X } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import Svg, { Circle } from "react-native-svg";

// Segmented Circle Border Component
function SegmentedBorder({
  segments,
  size,
  strokeWidth,
  color,
}: {
  segments: number;
  size: number;
  strokeWidth: number;
  color: string;
}) {
  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const gapAngle = segments > 1 ? 8 : 0; // gap in degrees
  const segmentAngle = (360 - gapAngle * segments) / segments;
  const segmentLength = (segmentAngle / 360) * circumference;

  return (
    <Svg width={size} height={size} style={{ position: "absolute" }}>
      {Array.from({ length: segments }).map((_, index) => {
        const rotationAngle = index * (segmentAngle + gapAngle) - 90;
        return (
          <Circle
            key={index}
            cx={size / 2}
            cy={size / 2}
            r={radius}
            stroke={color}
            strokeWidth={strokeWidth}
            fill="none"
            strokeDasharray={`${segmentLength} ${circumference - segmentLength}`}
            strokeLinecap="round"
            transform={`rotate(${rotationAngle}, ${size / 2}, ${size / 2})`}
          />
        );
      })}
    </Svg>
  );
}

const { width: SCREEN_WIDTH, height: SCREEN_HEIGHT } = Dimensions.get("window");

interface Story {
  id: string;
  title: string;
  imageUrl: string;
  isLive?: boolean;
  isNew?: boolean;
  contents: StoryContent[];
}

interface StoryContent {
  id: string;
  imageUrl: string;
  duration: number;
}

const mockStories: Story[] = [
  {
    id: "1",
    title: "Yeni Sezon",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
    isNew: true,
    contents: [
      { id: "1-1", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo", duration: 5000 },
      { id: "1-2", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY", duration: 5000 },
    ],
  },
  {
    id: "2",
    title: "Canli",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
    isLive: true,
    contents: [
      { id: "2-1", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY", duration: 5000 },
    ],
  },
  {
    id: "3",
    title: "Indirim",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4",
    isNew: true,
    contents: [
      { id: "3-1", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuCVtEr3vAdt6DqVknFsqutJ5mcJyjwIIqGLXi-MYdFXWXrvmU3KnACp-XaF5OPLcPimTACC4KdRsFi2xrmiBQ_MihHUHAMDOH880LLd4tGQDh9HXKxVlBzMdn5TcOkX5KaeG1JvaJCgtOgRgZvcIRq1077PlU532TsJ5kh4qqDNqSh3cDmxymc_rT0ax50ssHLnad7CVcFIDMKFCL6XZBbySClF8AiDVWu0fWVaqbCgXg5v-aKv9HjukHmbyILJYi5v3x6K6VEVpe4", duration: 5000 },
      { id: "3-2", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuDRdh2mvhqicHpbdbkp3QYD6lGxPCZa45GDzqJIB-l1d4FHNRz0sRo_Yd2hJPpzqkS7lDNTlMWf2Uk7lM9cJcVKbRQHgpBqrDn47cYYfZ-0hYjJkwJzPjZJo_qQT-BRWBq8vKm5L_qR", duration: 5000 },
      { id: "3-3", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo", duration: 5000 },
    ],
  },
  {
    id: "4",
    title: "Trend",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuDRdh2mvhqicHpbdbkp3QYD6lGxPCZa45GDzqJIB-l1d4FHNRz0sRo_Yd2hJPpzqkS7lDNTlMWf2Uk7lM9cJcVKbRQHgpBqrDn47cYYfZ-0hYjJkwJzPjZJo_qQT-BRWBq8vKm5L_qR",
    contents: [
      { id: "4-1", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuDRdh2mvhqicHpbdbkp3QYD6lGxPCZa45GDzqJIB-l1d4FHNRz0sRo_Yd2hJPpzqkS7lDNTlMWf2Uk7lM9cJcVKbRQHgpBqrDn47cYYfZ-0hYjJkwJzPjZJo_qQT-BRWBq8vKm5L_qR", duration: 5000 },
    ],
  },
  {
    id: "5",
    title: "Teknoloji",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
    contents: [
      { id: "5-1", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo", duration: 5000 },
      { id: "5-2", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY", duration: 5000 },
    ],
  },
  {
    id: "6",
    title: "Moda",
    imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY",
    contents: [
      { id: "6-1", imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuB8s413GHRRT3waAu3rPGrQdZOqXuaoZUhyjV6dKx70ZblZnjpDYDhHuj5MyhmCAJ4D6acuVx_wa9iOQ9vXvCnns53M9hRbJCqyWGoRWFpQKBSJ7aFntsdmbOkbFscfshug-qKm7V20jTbEy3AkEUAdZIxE8jDVVo54oWdRAVVgml-xAe7fi008LPGN3EffvKREubQXI72scGB1xKxNvRMKT5YQVuubMJw4aBFW2CVk2N7lPk4jc74IVopwPZnIeS3V73ojg8wEzEY", duration: 5000 },
    ],
  },
];

// Story Viewer Component
function StoryViewer({
  story,
  visible,
  onClose,
  onNextStory,
  onPrevStory,
}: {
  story: Story;
  visible: boolean;
  onClose: () => void;
  onNextStory: () => void;
  onPrevStory: () => void;
}) {
  const insets = useSafeAreaInsets();
  const [currentIndex, setCurrentIndex] = useState(0);
  const [progress, setProgress] = useState(0);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const currentContent = story.contents[currentIndex];

  // Story değiştiğinde index'i sıfırla
  useEffect(() => {
    setCurrentIndex(0);
    setProgress(0);
  }, [story.id]);

  useEffect(() => {
    if (!visible) {
      setCurrentIndex(0);
      setProgress(0);
      return;
    }

    const duration = currentContent?.duration || 5000;
    const interval = 50;
    const increment = (interval / duration) * 100;
    let currentProgress = 0;

    timerRef.current = setInterval(() => {
      currentProgress += increment;

      if (currentProgress >= 100) {
        if (timerRef.current) clearInterval(timerRef.current);

        if (currentIndex < story.contents.length - 1) {
          setCurrentIndex((i) => i + 1);
          setProgress(0);
        } else {
          // Son story'nin son içeriği bitti, modalı kapat
          onNextStory();
        }
      } else {
        setProgress(currentProgress);
      }
    }, interval);

    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, [visible, currentIndex, story.contents.length, onNextStory]);

  const handlePress = (e: any) => {
    const x = e.nativeEvent.locationX;
    if (x < SCREEN_WIDTH / 3) {
      // Left tap - previous
      if (currentIndex > 0) {
        setCurrentIndex((i) => i - 1);
        setProgress(0);
      } else {
        onPrevStory();
      }
    } else if (x > (SCREEN_WIDTH * 2) / 3) {
      // Right tap - next
      if (currentIndex < story.contents.length - 1) {
        setCurrentIndex((i) => i + 1);
        setProgress(0);
      } else {
        onNextStory();
      }
    }
  };

  return (
    <Modal visible={visible} animationType="fade" statusBarTranslucent>
      <View style={{ flex: 1, backgroundColor: "#000" }}>
        <Pressable onPress={handlePress} style={{ flex: 1 }}>
          {/* Story Image */}
          <Image
            source={{ uri: currentContent?.imageUrl }}
            style={{
              width: SCREEN_WIDTH,
              height: SCREEN_HEIGHT,
              position: "absolute",
              top: 0,
              left: 0,
            }}
            resizeMode="cover"
          />

          {/* Progress Bars */}
          <View
            style={{
              position: "absolute",
              top: insets.top + 10,
              left: 10,
              right: 10,
              flexDirection: "row",
              gap: 4,
            }}>
            {story.contents.map((_, index) => (
              <View
                key={index}
                style={{
                  flex: 1,
                  height: 3,
                  backgroundColor: "rgba(255,255,255,0.3)",
                  borderRadius: 2,
                  overflow: "hidden",
                }}>
                <View
                  style={{
                    height: "100%",
                    backgroundColor: "#fff",
                    width:
                      index < currentIndex
                        ? "100%"
                        : index === currentIndex
                          ? `${progress}%`
                          : "0%",
                  }}
                />
              </View>
            ))}
          </View>

          {/* Header */}
          <View
            style={{
              position: "absolute",
              top: insets.top + 24,
              left: 16,
              right: 16,
              flexDirection: "row",
              alignItems: "center",
              justifyContent: "space-between",
            }}>
            <View style={{ flexDirection: "row", alignItems: "center", gap: 10 }}>
              <View
                style={{
                  width: 36,
                  height: 36,
                  borderRadius: 18,
                  overflow: "hidden",
                  borderWidth: 2,
                  borderColor: "#fff",
                }}>
                <Image
                  source={{ uri: story.imageUrl }}
                  style={{ width: 36, height: 36, aspectRatio: 1 }}
                  resizeMode="cover"
                />
              </View>
              <View>
                <Text style={{ color: "#fff", fontSize: 14, fontWeight: "600" }}>
                  {story.title}
                </Text>
                {story.isLive && (
                  <View
                    style={{
                      backgroundColor: "#FF0000",
                      paddingHorizontal: 6,
                      paddingVertical: 2,
                      borderRadius: 4,
                      marginTop: 2,
                      alignSelf: "flex-start",
                    }}>
                    <Text style={{ color: "#fff", fontSize: 9, fontWeight: "700" }}>CANLI</Text>
                  </View>
                )}
              </View>
            </View>

            <TouchableOpacity onPress={onClose} hitSlop={{ top: 10, bottom: 10, left: 10, right: 10 }}>
              <Icon as={X} size={28} className="text-white" />
            </TouchableOpacity>
          </View>
        </Pressable>
      </View>
    </Modal>
  );
}

export function StoryHighlights() {
  const [selectedStoryIndex, setSelectedStoryIndex] = useState<number | null>(null);

  const handleOpenStory = (index: number) => {
    setSelectedStoryIndex(index);
  };

  const handleCloseStory = () => {
    setSelectedStoryIndex(null);
  };

  const handleNextStory = () => {
    if (selectedStoryIndex !== null && selectedStoryIndex < mockStories.length - 1) {
      setSelectedStoryIndex(selectedStoryIndex + 1);
    } else {
      handleCloseStory();
    }
  };

  const handlePrevStory = () => {
    if (selectedStoryIndex !== null && selectedStoryIndex > 0) {
      setSelectedStoryIndex(selectedStoryIndex - 1);
    }
  };

  const selectedStory = selectedStoryIndex !== null ? mockStories[selectedStoryIndex] : null;

  return (
    <>
      <View style={{ paddingVertical: 12 }}>
        <ScrollView
          horizontal
          nestedScrollEnabled={true}
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={{ paddingHorizontal: 16 }}>
          {mockStories.map((story, index) => (
            <Pressable
              key={story.id}
              onPress={() => handleOpenStory(index)}
              style={{
                alignItems: "center",
                marginRight: index < mockStories.length - 1 ? 14 : 0,
              }}>
              <View
                style={{
                  width: 74,
                  height: 74,
                  alignItems: "center",
                  justifyContent: "center",
                }}>
                {/* Segmented Border */}
                <SegmentedBorder
                  segments={story.contents.length}
                  size={74}
                  strokeWidth={2.5}
                  color={story.isLive ? "#FF0000" : story.isNew ? "#111118" : "#E0E0E0"}
                />
                {/* Story Image */}
                <View
                  style={{
                    width: 64,
                    height: 64,
                    borderRadius: 32,
                    overflow: "hidden",
                    backgroundColor: "#f0f0f0",
                  }}>
                  <Image
                    source={{ uri: story.imageUrl }}
                    style={{
                      width: 64,
                      height: 64,
                      aspectRatio: 1,
                    }}
                    resizeMode="cover"
                  />
                </View>
              </View>
              {story.isLive && (
                <View
                  style={{
                    position: "absolute",
                    top: 58,
                    backgroundColor: "#FF0000",
                    paddingHorizontal: 6,
                    paddingVertical: 2,
                    borderRadius: 4,
                    borderWidth: 2,
                    borderColor: "#fff",
                  }}>
                  <Text style={{ color: "#fff", fontSize: 8, fontWeight: "700" }}>CANLI</Text>
                </View>
              )}
              <Text
                style={{
                  fontSize: 11,
                  fontWeight: "500",
                  color: "#666",
                  marginTop: 6,
                }}>
                {story.title}
              </Text>
            </Pressable>
          ))}
        </ScrollView>
      </View>

      {/* Story Viewer Modal */}
      {selectedStory && (
        <StoryViewer
          story={selectedStory}
          visible={selectedStoryIndex !== null}
          onClose={handleCloseStory}
          onNextStory={handleNextStory}
          onPrevStory={handlePrevStory}
        />
      )}
    </>
  );
}
