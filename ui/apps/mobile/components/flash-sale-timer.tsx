import { View, Pressable } from "react-native";
import { Text } from "@merge/uim/components/text";
import { useState, useEffect } from "react";
import { Zap } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";

interface FlashSaleTimerProps {
  endTime: Date;
  onPress?: () => void;
}

export function FlashSaleTimer({ endTime, onPress }: FlashSaleTimerProps) {
  const [timeLeft, setTimeLeft] = useState({
    hours: 0,
    minutes: 0,
    seconds: 0,
  });

  useEffect(() => {
    const timer = setInterval(() => {
      const now = new Date().getTime();
      const end = endTime.getTime();
      const difference = end - now;

      if (difference > 0) {
        setTimeLeft({
          hours: Math.floor((difference % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)),
          minutes: Math.floor((difference % (1000 * 60 * 60)) / (1000 * 60)),
          seconds: Math.floor((difference % (1000 * 60)) / 1000),
        });
      }
    }, 1000);

    return () => clearInterval(timer);
  }, [endTime]);

  const formatNumber = (num: number) => num.toString().padStart(2, "0");

  return (
    <Pressable onPress={onPress} style={{ marginHorizontal: 16, marginBottom: 16 }}>
      <View
        style={{
          backgroundColor: "#111118",
          borderRadius: 16,
          overflow: "hidden",
        }}>
        <View
          style={{
            flexDirection: "row",
            alignItems: "center",
            justifyContent: "space-between",
            padding: 16,
          }}>
          {/* Left - Icon and Text */}
          <View style={{ flexDirection: "row", alignItems: "center", gap: 12 }}>
            <View
              style={{
                backgroundColor: "#fff",
                padding: 10,
                borderRadius: 10,
              }}>
              <Icon as={Zap} size={20} fill="#111118" className="text-foreground" />
            </View>
            <View>
              <Text style={{ color: "#fff", fontWeight: "700", fontSize: 16, textTransform: "uppercase", letterSpacing: 0.5 }}>
                Flash Sale
              </Text>
              <Text style={{ color: "rgba(255,255,255,0.6)", fontSize: 12, marginTop: 2 }}>
                Kacirma, firsatlar bitiyor!
              </Text>
            </View>
          </View>

          {/* Right - Timer */}
          <View style={{ flexDirection: "row", alignItems: "center", gap: 4 }}>
            <View
              style={{
                backgroundColor: "#fff",
                paddingHorizontal: 10,
                paddingVertical: 8,
                borderRadius: 8,
                minWidth: 40,
                alignItems: "center",
              }}>
              <Text style={{ color: "#111118", fontWeight: "700", fontSize: 14 }}>
                {formatNumber(timeLeft.hours)}
              </Text>
            </View>
            <Text style={{ color: "#fff", fontWeight: "700", fontSize: 14 }}>:</Text>
            <View
              style={{
                backgroundColor: "#fff",
                paddingHorizontal: 10,
                paddingVertical: 8,
                borderRadius: 8,
                minWidth: 40,
                alignItems: "center",
              }}>
              <Text style={{ color: "#111118", fontWeight: "700", fontSize: 14 }}>
                {formatNumber(timeLeft.minutes)}
              </Text>
            </View>
            <Text style={{ color: "#fff", fontWeight: "700", fontSize: 14 }}>:</Text>
            <View
              style={{
                backgroundColor: "#fff",
                paddingHorizontal: 10,
                paddingVertical: 8,
                borderRadius: 8,
                minWidth: 40,
                alignItems: "center",
              }}>
              <Text style={{ color: "#111118", fontWeight: "700", fontSize: 14 }}>
                {formatNumber(timeLeft.seconds)}
              </Text>
            </View>
          </View>
        </View>
      </View>
    </Pressable>
  );
}
