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
import { FlashSaleTimer } from "../../components/flash-sale-timer";
import { StoryHighlights } from "../../components/story-highlights";
import { PromoGrid } from "../../components/promo-grid";
import { BrandsCarousel } from "../../components/brands-carousel";
import { DealOfDay } from "../../components/deal-of-day";
import { RecentlyViewed } from "../../components/recently-viewed";
import { LiveShopping } from "../../components/live-shopping";
import { CouponSection } from "../../components/coupon-section";

// Mock data
const flashProducts: ProductCardProps[] = [
  {
    id: "1",
    name: "Premium Kablosuz Kulaklik",
    price: 899,
    originalPrice: 1299,
    discount: 31,
    badge: "FLASH",
  },
  {
    id: "2",
    name: "Akilli Saat Pro",
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
    name: "Spor Ayakkabi",
    price: 799,
    originalPrice: 1199,
    discount: 33,
  },
  {
    id: "7",
    name: "Gunes Gozlugu",
    price: 399,
    originalPrice: 599,
    discount: 33,
  },
  {
    id: "8",
    name: "Canta",
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
    name: "Cuzdan",
    price: 299,
    originalPrice: 449,
    discount: 33,
  },
];

const recommendedProducts: ProductCardProps[] = [
  {
    id: "11",
    name: "Bluetooth Hoparlor",
    price: 1299,
  },
  {
    id: "12",
    name: "Powerbank",
    price: 499,
  },
  {
    id: "13",
    name: "Telefon Kilifi",
    price: 199,
  },
  {
    id: "14",
    name: "Ekran Koruyucu",
    price: 149,
  },
  {
    id: "15",
    name: "Kablosuz Sarj",
    price: 399,
  },
];

const bestSellerProducts: ProductCardProps[] = [
  {
    id: "16",
    name: "Yazilim Kitabi",
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
    name: "Masa Lambasi",
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
    name: "Yeni Koleksiyon Canta",
    price: 899,
  },
  {
    id: "22",
    name: "Yeni Tasarim Saat",
    price: 2499,
  },
  {
    id: "23",
    name: "Yeni Model Kulaklik",
    price: 1299,
  },
  {
    id: "24",
    name: "Yeni Seri Telefon",
    price: 8999,
  },
  {
    id: "25",
    name: "Yeni Koleksiyon Ayakkabi",
    price: 1499,
  },
];

// Flash sale end time (24 hours from now)
const flashSaleEndTime = new Date(Date.now() + 24 * 60 * 60 * 1000);

export default function Home() {
  const insets = useSafeAreaInsets();
  const scrollViewRef = useRef<ScrollView>(null);
  const [refreshing, setRefreshing] = useState(false);
  const [showScrollToTop, setShowScrollToTop] = useState(false);

  const onRefresh = () => {
    setRefreshing(true);
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
          paddingTop: 8,
          paddingBottom: insets.bottom,
        }}
        showsVerticalScrollIndicator={false}
        scrollEnabled={true}
        nestedScrollEnabled={true}
        bounces={true}
        alwaysBounceVertical={true}
        onScroll={handleScroll}
        scrollEventThrottle={16}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }>
        {/* Story Highlights */}
        <StoryHighlights />

        {/* Banner Slider */}
        <BannerSlider />

        {/* Flash Sale Timer */}
        <FlashSaleTimer endTime={flashSaleEndTime} onPress={() => {}} />

        {/* Category Icons */}
        <CategoryIcons />

        {/* Promo Grid */}
        <PromoGrid />

        {/* Coupon Section */}
        <CouponSection />

        {/* Deal of the Day */}
        <DealOfDay />

        {/* Flash Products */}
        <ProductSection
          title="FLASH URUNLER"
          products={flashProducts}
          onSeeAll={() => {}}
        />

        {/* Live Shopping */}
        <LiveShopping />

        {/* Campaign Products */}
        <ProductSection
          title="KAMPANYALI URUNLER"
          products={campaignProducts}
          onSeeAll={() => {}}
        />

        {/* Brands Carousel */}
        <BrandsCarousel />

        {/* Recommended Products */}
        <ProductSection
          title="SIZE OZEL ONERILER"
          products={recommendedProducts}
          onSeeAll={() => {}}
        />

        {/* Recently Viewed */}
        <RecentlyViewed />

        {/* Best Sellers */}
        <ProductSection
          title="COK SATANLAR"
          products={bestSellerProducts}
          onSeeAll={() => {}}
        />

        {/* New Products */}
        <ProductSection
          title="YENI GELENLER"
          products={newProducts}
          isLast
          onSeeAll={() => {}}
        />
      </ScrollView>

      {/* Scroll to Top Button */}
      <ScrollToTop scrollViewRef={scrollViewRef} show={showScrollToTop} />
    </View>
  );
}
