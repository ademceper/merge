import { View, ScrollView } from "react-native";
import { StatusBar } from "expo-status-bar";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Text } from "@merge/uim/components/text";
import { Button } from "@merge/uim/components/button";
import { Card, CardContent, CardHeader, CardTitle } from "@merge/uim/components/card";
import { Separator } from "@merge/uim/components/separator";
import { User } from "lucide-react-native";
import { Icon } from "@merge/uim/components/icon";

export default function Profile() {
  const insets = useSafeAreaInsets();

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <StatusBar style="auto" />
      <ScrollView
        className="flex-1"
        contentContainerClassName="p-4 gap-4"
        contentContainerStyle={{ paddingBottom: insets.bottom + 100 }}>
        <Text className="text-2xl font-bold">Profil</Text>

        <Card>
          <CardHeader>
            <View className="flex-row items-center gap-4">
              <View className="flex items-center justify-center p-4 rounded-full border border-primary/10 dark:border-white/10 bg-white dark:bg-white/5">
                <Icon
                  as={User}
                  size={32}
                  className="text-foreground"
                />
              </View>
              <View className="flex-1">
                <CardTitle>Kullanıcı</CardTitle>
                <Text className="text-sm text-muted-foreground mt-1">
                  user@example.com
                </Text>
              </View>
            </View>
          </CardHeader>
          <CardContent className="gap-4">
            <Separator />
            <Button variant="outline" className="w-full">
              Hesap Ayarları
            </Button>
            <Button variant="outline" className="w-full">
              Siparişlerim
            </Button>
            <Button variant="outline" className="w-full">
              Adreslerim
            </Button>
            <Separator />
            <Button variant="ghost" className="w-full">
              Çıkış Yap
            </Button>
          </CardContent>
        </Card>
      </ScrollView>
    </View>
  );
}
