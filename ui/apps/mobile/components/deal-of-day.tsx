import { View, Pressable, Image } from "react-native";
import { Text } from "@merge/uim/components/text";
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
    <View style={{ marginHorizontal: 16, marginBottom: 20 }}>
      {/* Header */}
      <View style={{ flexDirection: "row", alignItems: "center", justifyContent: "space-between", marginBottom: 12 }}>
        <Text className="text-lg font-bold uppercase tracking-tight text-foreground dark:text-white">
          Gunun Firsati
        </Text>
        <View style={{ flexDirection: "row", alignItems: "center", gap: 6 }}>
          <View style={{ flexDirection: "row", alignItems: "center", gap: 4 }}>
            <View style={{ backgroundColor: "#111118", paddingHorizontal: 8, paddingVertical: 4, borderRadius: 6 }}>
              <Text style={{ color: "#fff", fontSize: 12, fontWeight: "700" }}>{formatNumber(timeLeft.hours)}</Text>
            </View>
            <Text style={{ color: "#111", fontWeight: "600" }}>:</Text>
            <View style={{ backgroundColor: "#111118", paddingHorizontal: 8, paddingVertical: 4, borderRadius: 6 }}>
              <Text style={{ color: "#fff", fontSize: 12, fontWeight: "700" }}>{formatNumber(timeLeft.minutes)}</Text>
            </View>
            <Text style={{ color: "#111", fontWeight: "600" }}>:</Text>
            <View style={{ backgroundColor: "#111118", paddingHorizontal: 8, paddingVertical: 4, borderRadius: 6 }}>
              <Text style={{ color: "#fff", fontSize: 12, fontWeight: "700" }}>{formatNumber(timeLeft.seconds)}</Text>
            </View>
          </View>
        </View>
      </View>

      {/* Card */}
      <Pressable
        style={{
          borderRadius: 16,
          overflow: "hidden",
          backgroundColor: "#fff",
          borderWidth: 1,
          borderColor: "rgba(0,0,0,0.08)",
        }}>
        <View style={{ flexDirection: "row" }}>
          {/* Product Image */}
          <View
            style={{
              width: 140,
              height: 180,
              backgroundColor: "#f8f8f8",
            }}>
            <Image
              source={{ uri: dealProduct.imageUrl }}
              style={{ width: "100%", height: "100%" }}
              resizeMode="cover"
            />
            <View
              style={{
                position: "absolute",
                top: 10,
                left: 10,
                backgroundColor: "#111118",
                paddingHorizontal: 10,
                paddingVertical: 5,
                borderRadius: 6,
              }}>
              <Text style={{ color: "#fff", fontSize: 12, fontWeight: "700" }}>%{dealProduct.discount}</Text>
            </View>
          </View>

          {/* Product Info */}
          <View style={{ flex: 1, padding: 14, justifyContent: "space-between" }}>
            <View style={{ gap: 8 }}>
              <Text style={{ fontSize: 14, fontWeight: "600", color: "#111" }} numberOfLines={2}>
                {dealProduct.name}
              </Text>

              {/* Rating */}
              <View style={{ flexDirection: "row", alignItems: "center", gap: 4 }}>
                <Icon as={Star} size={12} fill="#111" className="text-foreground" />
                <Text style={{ fontSize: 12, color: "#111", fontWeight: "500" }}>{dealProduct.rating}</Text>
                <Text style={{ fontSize: 11, color: "#888" }}>({dealProduct.reviewCount})</Text>
              </View>

              {/* Price */}
              <View style={{ gap: 2 }}>
                <Text style={{ fontSize: 22, fontWeight: "700", color: "#111" }}>
                  {dealProduct.price.toLocaleString("tr-TR")} TL
                </Text>
                <Text style={{ fontSize: 13, color: "#888", textDecorationLine: "line-through" }}>
                  {dealProduct.originalPrice.toLocaleString("tr-TR")} TL
                </Text>
              </View>
            </View>

            {/* Stock Progress */}
            <View style={{ gap: 6 }}>
              <View
                style={{
                  height: 6,
                  backgroundColor: "#E8E8E8",
                  borderRadius: 3,
                  overflow: "hidden",
                }}>
                <View
                  style={{
                    height: "100%",
                    width: `${progressPercent}%`,
                    backgroundColor: "#111118",
                    borderRadius: 3,
                  }}
                />
              </View>
              <View style={{ flexDirection: "row", justifyContent: "space-between" }}>
                <Text style={{ fontSize: 10, color: "#888" }}>
                  {dealProduct.soldCount} satildi
                </Text>
                <Text style={{ fontSize: 10, color: "#888" }}>
                  {dealProduct.totalStock - dealProduct.soldCount} kaldi
                </Text>
              </View>
            </View>
          </View>
        </View>
      </Pressable>
    </View>
  );
}
