import { View, ScrollView, Pressable } from "react-native";
import { Text } from "@merge/uim/components/text";
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
    <View style={{ marginBottom: 20 }}>
      {/* Header */}
      <View style={{ flexDirection: "row", alignItems: "center", justifyContent: "space-between", paddingHorizontal: 16, marginBottom: 12 }}>
        <Text className="text-lg font-bold uppercase tracking-tight text-foreground dark:text-white">
          Kuponlar
        </Text>
        <Pressable>
          <Text className="text-xs font-medium uppercase tracking-wide text-foreground/50 dark:text-white/50">
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
            <View
              style={{
                borderRadius: 16,
                borderWidth: 1,
                borderColor: "rgba(0,0,0,0.1)",
                backgroundColor: "#fff",
                overflow: "hidden",
              }}>
              {/* Top Section - Discount */}
              <View
                style={{
                  backgroundColor: "#111118",
                  padding: 16,
                  alignItems: "center",
                }}>
                <Text style={{ color: "#fff", fontSize: 24, fontWeight: "700", textTransform: "uppercase" }}>
                  {coupon.discount}
                </Text>
                <Text style={{ color: "rgba(255,255,255,0.6)", fontSize: 11, marginTop: 4 }}>
                  {coupon.description}
                </Text>
              </View>

              {/* Bottom Section - Code */}
              <View style={{ padding: 12 }}>
                <View style={{ flexDirection: "row", alignItems: "center", justifyContent: "space-between", marginBottom: 8 }}>
                  <View
                    style={{
                      backgroundColor: "#f5f5f5",
                      paddingHorizontal: 10,
                      paddingVertical: 6,
                      borderRadius: 6,
                      borderWidth: 1,
                      borderColor: "#e0e0e0",
                      borderStyle: "dashed",
                    }}>
                    <Text style={{ color: "#111", fontSize: 11, fontWeight: "700", letterSpacing: 1 }}>
                      {coupon.code}
                    </Text>
                  </View>
                  <Pressable
                    onPress={() => handleCopy(coupon.id)}
                    style={{
                      flexDirection: "row",
                      alignItems: "center",
                      gap: 4,
                    }}>
                    <Icon
                      as={copiedId === coupon.id ? Check : Copy}
                      size={14}
                      className="text-foreground/60"
                    />
                    <Text style={{ color: "#666", fontSize: 10, fontWeight: "500" }}>
                      {copiedId === coupon.id ? "Kopyalandi" : "Kopyala"}
                    </Text>
                  </Pressable>
                </View>

                <View style={{ flexDirection: "row", alignItems: "center", justifyContent: "space-between" }}>
                  <Text style={{ color: "#999", fontSize: 10 }}>
                    {coupon.minOrder ? `Min. ${coupon.minOrder} TL` : "Min. yok"}
                  </Text>
                  <Text style={{ color: "#999", fontSize: 10 }}>
                    {coupon.expiresIn} kaldi
                  </Text>
                </View>
              </View>
            </View>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}
