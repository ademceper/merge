import { View, Pressable, Image } from "react-native";
import { Text } from "@merge/uim/components/text";
import { Badge } from "@merge/uim/components/badge";
import { Card } from "@merge/uim/components/card";
import { useState, useEffect } from "react";
import { Star } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";

interface DealProduct {
  id: string;
  name: string;
  price: number;
  originalPrice: number;
  discount: number;
  imageUrl: string;
  rating: number;
  reviewCount: number;
  soldCount: number;
  totalStock: number;
}

const dealProduct: DealProduct = {
  id: "deal-1",
  name: "Apple AirPods Pro 2. Nesil",
  price: 4999,
  originalPrice: 7999,
  discount: 38,
  imageUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuC_Ut-KHF1qiELT8FKyXUTkB49BCelIRoPUeJFLSe_-VFZX8j8nCAP-XNYZu1PDiiFTZfRNRZX7rPHsQXpFoKVzVnwZGB8_N3eSqMQqB2mbRFrq6TOzpt0x5K4KUW86yTWqY1xj2WPu-kSgeJZEuA5fMhMaKvGBQfh6lLLAkQSYOanYT1cLAPhRrNcbARHqF24efJ-3VmL1k7S6txZxPq1M2m0h_ClruxeJE4dwulFBP32GpxdoUrC0vrIJ742pNhUkIqZlwVWuuYo",
  rating: 4.8,
  reviewCount: 2547,
  soldCount: 847,
  totalStock: 1000,
};

export function DealOfDay() {
  const [timeLeft, setTimeLeft] = useState({
    hours: 23,
    minutes: 59,
    seconds: 59,
  });

  useEffect(() => {
    const timer = setInterval(() => {
      setTimeLeft((prev) => {
        let { hours, minutes, seconds } = prev;
        seconds--;
        if (seconds < 0) {
          seconds = 59;
          minutes--;
          if (minutes < 0) {
            minutes = 59;
            hours--;
            if (hours < 0) {
              hours = 23;
            }
          }
        }
        return { hours, minutes, seconds };
      });
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const formatNumber = (num: number) => num.toString().padStart(2, "0");
  const progressPercent = (dealProduct.soldCount / dealProduct.totalStock) * 100;

  return (
    <View className="px-4 mb-6">
      {/* Header */}
      <View className="flex-row items-center justify-between mb-4">
        <Text className="text-lg font-bold uppercase tracking-tight text-foreground dark:text-white">
          GUNUN FIRSATI
        </Text>
        <View className="flex-row items-center gap-1">
          <View className="bg-primary dark:bg-white px-2 py-1 rounded-md">
            <Text className="text-primary-foreground dark:text-[#111118] text-xs font-bold">
              {formatNumber(timeLeft.hours)}
            </Text>
          </View>
          <Text className="text-foreground dark:text-white font-semibold">:</Text>
          <View className="bg-primary dark:bg-white px-2 py-1 rounded-md">
            <Text className="text-primary-foreground dark:text-[#111118] text-xs font-bold">
              {formatNumber(timeLeft.minutes)}
            </Text>
          </View>
          <Text className="text-foreground dark:text-white font-semibold">:</Text>
          <View className="bg-primary dark:bg-white px-2 py-1 rounded-md">
            <Text className="text-primary-foreground dark:text-[#111118] text-xs font-bold">
              {formatNumber(timeLeft.seconds)}
            </Text>
          </View>
        </View>
      </View>

      {/* Card */}
      <Pressable>
        <Card className="flex-row overflow-hidden p-0 border-primary/10 dark:border-white/10 bg-white dark:bg-white/5">
          {/* Product Image */}
          <View className="w-[140px] h-[180px] bg-black overflow-hidden">
            <Image
              source={{ uri: dealProduct.imageUrl }}
              className="w-full h-full"
              resizeMode="cover"
            />
            <View className="absolute top-2.5 left-2.5">
              <Badge variant="destructive" className="text-xs font-bold">
                %{dealProduct.discount}
              </Badge>
            </View>
          </View>

          {/* Product Info */}
          <View className="flex-1 p-4 justify-between">
            <View className="gap-2">
              <Text
                className="text-sm font-semibold text-foreground dark:text-white"
                numberOfLines={2}>
                {dealProduct.name}
              </Text>

              {/* Rating */}
              <View className="flex-row items-center gap-1">
                <Icon
                  as={Star}
                  size={12}
                  fill="currentColor"
                  className="text-foreground dark:text-white"
                />
                <Text className="text-xs text-foreground dark:text-white font-medium">
                  {dealProduct.rating}
                </Text>
                <Text className="text-xs text-muted-foreground dark:text-white/70">
                  ({dealProduct.reviewCount})
                </Text>
              </View>

              {/* Price */}
              <View className="gap-0.5">
                <Text className="text-xl font-bold text-foreground dark:text-white">
                  {dealProduct.price.toLocaleString("tr-TR")} ₺
                </Text>
                <Text className="text-sm text-muted-foreground dark:text-white/70 line-through">
                  {dealProduct.originalPrice.toLocaleString("tr-TR")} ₺
                </Text>
              </View>
            </View>

            {/* Stock Progress */}
            <View className="gap-1.5">
              <View className="h-1.5 bg-primary/10 dark:bg-white/10 rounded-full overflow-hidden">
                <View
                  className="h-full bg-primary dark:bg-white rounded-full"
                  style={{ width: `${progressPercent}%` }}
                />
              </View>
              <View className="flex-row justify-between">
                <Text className="text-[10px] text-muted-foreground dark:text-white/70">
                  {dealProduct.soldCount} satildi
                </Text>
                <Text className="text-[10px] text-muted-foreground dark:text-white/70">
                  {dealProduct.totalStock - dealProduct.soldCount} kaldi
                </Text>
              </View>
            </View>
          </View>
        </Card>
      </Pressable>
    </View>
  );
}
