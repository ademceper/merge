import { View, ScrollView, Pressable } from "react-native";
import { Text } from "@merge/uim/components/text";
import { Card } from "@merge/uim/components/card";
import { Copy, Check } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";
import { useState } from "react";

interface Coupon {
  id: string;
  code: string;
  discount: string;
  description: string;
  minOrder?: number;
  expiresIn: string;
}

const coupons: Coupon[] = [
  {
    id: "1",
    code: "HOSGELDIN50",
    discount: "50 TL",
    description: "Ilk siparisine ozel",
    minOrder: 200,
    expiresIn: "3 gun",
  },
  {
    id: "2",
    code: "KARGO",
    discount: "Ucretsiz Kargo",
    description: "Kargo bedava",
    minOrder: 150,
    expiresIn: "7 gun",
  },
  {
    id: "3",
    code: "YUZDE10",
    discount: "%10",
    description: "Tum urunlerde gecerli",
    expiresIn: "2 gun",
  },
  {
    id: "4",
    code: "TEKNOLOJI",
    discount: "%15",
    description: "Elektronik urunlerde",
    minOrder: 500,
    expiresIn: "5 gun",
  },
];

export function CouponSection() {
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const handleCopy = (id: string) => {
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  return (
    <View className="mb-5">
      {/* Header */}
      <View className="flex-row items-center justify-between px-4 mb-3">
        <Text className="text-lg font-bold uppercase tracking-tight text-foreground">
          Kuponlar
        </Text>
        <Pressable>
          <Text className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            Tumu {">"}
          </Text>
        </Pressable>
      </View>

      {/* Coupons */}
      <ScrollView
        horizontal
        nestedScrollEnabled={true}
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={{ paddingHorizontal: 16 }}>
        {coupons.map((coupon, index) => (
          <Pressable
            key={coupon.id}
            onPress={() => handleCopy(coupon.id)}
            style={{
              width: 200,
              marginRight: index < coupons.length - 1 ? 12 : 0,
            }}>
            <Card className="overflow-hidden">
              {/* Top Section - Discount */}
              <View className="bg-primary p-4 items-center">
                <Text className="text-primary-foreground text-2xl font-bold uppercase">
                  {coupon.discount}
                </Text>
                <Text className="text-primary-foreground/60 text-[11px] mt-1">
                  {coupon.description}
                </Text>
              </View>

              {/* Bottom Section - Code */}
              <View className="p-3">
                <View className="flex-row items-center justify-between mb-2">
                  <View className="bg-secondary px-2.5 py-1.5 rounded-md border border-dashed border-border">
                    <Text className="text-foreground text-[11px] font-bold tracking-wider">
                      {coupon.code}
                    </Text>
                  </View>
                  <Pressable
                    onPress={() => handleCopy(coupon.id)}
                    className="flex-row items-center gap-1">
                    <Icon
                      as={copiedId === coupon.id ? Check : Copy}
                      size={14}
                      className="text-muted-foreground"
                    />
                    <Text className="text-muted-foreground text-[10px] font-medium">
                      {copiedId === coupon.id ? "Kopyalandi" : "Kopyala"}
                    </Text>
                  </Pressable>
                </View>

                <View className="flex-row items-center justify-between">
                  <Text className="text-muted-foreground text-[10px]">
                    {coupon.minOrder ? `Min. ${coupon.minOrder} TL` : "Min. yok"}
                  </Text>
                  <Text className="text-muted-foreground text-[10px]">
                    {coupon.expiresIn} kaldi
                  </Text>
                </View>
              </View>
            </Card>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}
