import { View, Pressable } from "react-native";
import { Text } from "@merge/uim/components/text";
import { Card } from "@merge/uim/components/card";
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
    <Pressable onPress={onPress} className="mx-4 mb-4">
      <Card className="bg-primary rounded-2xl overflow-hidden">
        <View className="flex-row items-center justify-between p-4">
          {/* Left - Icon and Text */}
          <View className="flex-row items-center gap-3">
            <View className="bg-primary-foreground p-2.5 rounded-xl">
              <Icon as={Zap} size={20} fill="currentColor" className="text-primary" />
            </View>
            <View>
              <Text className="text-primary-foreground font-bold text-base uppercase tracking-wide">
                Flash Sale
              </Text>
              <Text className="text-primary-foreground/60 text-xs mt-0.5">
                Kacirma, firsatlar bitiyor!
              </Text>
            </View>
          </View>

          {/* Right - Timer */}
          <View className="flex-row items-center gap-1">
            <View className="bg-primary-foreground px-2.5 py-2 rounded-lg min-w-[40px] items-center">
              <Text className="text-primary font-bold text-sm">
                {formatNumber(timeLeft.hours)}
              </Text>
            </View>
            <Text className="text-primary-foreground font-bold text-sm">:</Text>
            <View className="bg-primary-foreground px-2.5 py-2 rounded-lg min-w-[40px] items-center">
              <Text className="text-primary font-bold text-sm">
                {formatNumber(timeLeft.minutes)}
              </Text>
            </View>
            <Text className="text-primary-foreground font-bold text-sm">:</Text>
            <View className="bg-primary-foreground px-2.5 py-2 rounded-lg min-w-[40px] items-center">
              <Text className="text-primary font-bold text-sm">
                {formatNumber(timeLeft.seconds)}
              </Text>
            </View>
          </View>
        </View>
      </Card>
    </Pressable>
  );
}
