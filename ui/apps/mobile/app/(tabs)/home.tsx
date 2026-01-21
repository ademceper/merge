import { ScrollView, View, RefreshControl } from "react-native";
import { StatusBar } from "expo-status-bar";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { useRef, useState } from "react";
import { HomeHeader } from "../../components/home-header";
import { CategoryIcons } from "../../components/category-icons";
import { BannerSlider } from "../../components/banner-slider";
import { ProductSection } from "../../components/product-section";
import { ScrollToTop } from "../../components/scroll-to-top";
import { ProductCardProps } from "../../components/product-card";

// Mock data
const flashProducts: ProductCardProps[] = [
  {
    id: "1",
    name: "Premium Kablosuz Kulaklık",
    price: 899,
    originalPrice: 1299,
    discount: 31,
    badge: "FLASH",
  },
  {
    id: "2",
    name: "Akıllı Saat Pro",
    price: 2499,
    originalPrice: 3499,
    discount: 29,
    badge: "FLASH",
  },
  {
    id: "3",
    name: "Laptop Stand",
    price: 299,
    originalPrice: 499,
    discount: 40,
    badge: "FLASH",
  },
  {
    id: "4",
    name: "Mekanik Klavye",
    price: 1299,
    originalPrice: 1799,
    discount: 28,
    badge: "FLASH",
  },
  {
    id: "5",
    name: "Gaming Mouse",
    price: 599,
    originalPrice: 899,
    discount: 33,
    badge: "FLASH",
  },
];

const campaignProducts: ProductCardProps[] = [
  {
    id: "6",
    name: "Spor Ayakkabı",
    price: 799,
    originalPrice: 1199,
    discount: 33,
  },
  {
    id: "7",
    name: "Güneş Gözlüğü",
    price: 399,
    originalPrice: 599,
    discount: 33,
  },
  {
    id: "8",
    name: "Çanta",
    price: 599,
    originalPrice: 899,
    discount: 33,
  },
  {
    id: "9",
    name: "Saat",
    price: 1499,
    originalPrice: 1999,
    discount: 25,
  },
  {
    id: "10",
    name: "Cüzdan",
    price: 299,
    originalPrice: 449,
    discount: 33,
  },
];

const recommendedProducts: ProductCardProps[] = [
  {
    id: "11",
    name: "Bluetooth Hoparlör",
    price: 1299,
  },
  {
    id: "12",
    name: "Powerbank",
    price: 499,
  },
  {
    id: "13",
    name: "Telefon Kılıfı",
    price: 199,
  },
  {
    id: "14",
    name: "Ekran Koruyucu",
    price: 149,
  },
  {
    id: "15",
    name: "Kablosuz Şarj",
    price: 399,
  },
];

const bestSellerProducts: ProductCardProps[] = [
  {
    id: "16",
    name: "Yazılım Kitabı",
    price: 199,
  },
  {
    id: "17",
    name: "Not Defteri Seti",
    price: 149,
  },
  {
    id: "18",
    name: "Kalem Seti",
    price: 299,
  },
  {
    id: "19",
    name: "Masa Lambası",
    price: 599,
  },
  {
    id: "20",
    name: "Organizer",
    price: 399,
  },
];

const newProducts: ProductCardProps[] = [
  {
    id: "21",
    name: "Yeni Koleksiyon Çanta",
    price: 899,
  },
  {
    id: "22",
    name: "Yeni Tasarım Saat",
    price: 2499,
  },
  {
    id: "23",
    name: "Yeni Model Kulaklık",
    price: 1299,
  },
  {
    id: "24",
    name: "Yeni Seri Telefon",
    price: 8999,
  },
  {
    id: "25",
    name: "Yeni Koleksiyon Ayakkabı",
    price: 1499,
  },
];

export default function Home() {
  const insets = useSafeAreaInsets();
  const scrollViewRef = useRef<ScrollView>(null);
  const [refreshing, setRefreshing] = useState(false);
  const [showScrollToTop, setShowScrollToTop] = useState(false);

  const onRefresh = () => {
    setRefreshing(true);
    // Simulate API call
    setTimeout(() => {
      setRefreshing(false);
    }, 1000);
  };

  const handleScroll = (event: any) => {
    const offsetY = event.nativeEvent.contentOffset.y;
    setShowScrollToTop(offsetY > 500);
  };

  return (
    <View className="flex-1 bg-background">
      <StatusBar style="auto" />
      <HomeHeader />

      <ScrollView
        ref={scrollViewRef}
        style={{ flex: 1 }}
        contentContainerStyle={{
          paddingBottom: insets.bottom + 200,
        }}
        showsVerticalScrollIndicator={false}
        scrollEnabled={true}
        bounces={true}
        alwaysBounceVertical={true}
        onScroll={handleScroll}
        scrollEventThrottle={16}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }>
        {/* Category Icons */}
        <CategoryIcons />

        {/* Banner Slider */}
        <BannerSlider />

        {/* Flash Products */}
        <ProductSection
          title="FLASH ÜRÜNLER"
          products={flashProducts}
          onSeeAll={() => {
            // Navigate to flash sale page
          }}
        />

        {/* Campaign Products */}
        <ProductSection
          title="KAMPANYALI ÜRÜNLER"
          products={campaignProducts}
          onSeeAll={() => {
            // Navigate to campaign page
          }}
        />

        {/* Recommended Products */}
        <ProductSection
          title="SIZE ÖZEL ÖNERILER"
          products={recommendedProducts}
          onSeeAll={() => {
            // Navigate to recommendations
          }}
        />

        {/* Best Sellers */}
        <ProductSection
          title="ÇOK SATANLAR"
          products={bestSellerProducts}
          onSeeAll={() => {
            // Navigate to best sellers
          }}
        />

        {/* New Products */}
        <ProductSection
          title="YENI GELENLER"
          products={newProducts}
          onSeeAll={() => {
            // Navigate to new products
          }}
        />
      </ScrollView>

      {/* Scroll to Top Button */}
      <ScrollToTop scrollViewRef={scrollViewRef} show={showScrollToTop} />
    </View>
  );
}
