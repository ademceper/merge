import { ScrollView, View } from "react-native";
import { StatusBar } from "expo-status-bar";
import { Button } from "@merge/uim/components/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@merge/uim/components/card";
import { Text } from "@merge/uim/components/text";
import { Badge } from "@merge/uim/components/badge";
import { Switch } from "@merge/uim/components/switch";
import { Checkbox } from "@merge/uim/components/checkbox";
import { Input } from "@merge/uim/components/input";
import { Separator } from "@merge/uim/components/separator";
import * as React from "react";
import { useSafeAreaInsets } from "react-native-safe-area-context";

export default function Home() {
  const [checkboxValue, setCheckboxValue] = React.useState(false);
  const insets = useSafeAreaInsets();

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <StatusBar style="auto" />
      <ScrollView
        className="flex-1"
        contentContainerClassName="p-4 gap-4"
        contentContainerStyle={{ paddingBottom: insets.bottom + 100 }}>
        <Text className="text-3xl font-bold text-center mb-4">Mobile UI Components</Text>

        <Card>
          <CardHeader>
            <CardTitle>Button Variants</CardTitle>
            <CardDescription>Different button styles and sizes</CardDescription>
          </CardHeader>
          <CardContent className="gap-2">
            <Button onPress={() => alert("Default button pressed!")}>Default Button</Button>
            <Button variant="destructive" onPress={() => alert("Destructive button pressed!")}>
              Destructive
            </Button>
            <Button variant="outline" onPress={() => alert("Outline button pressed!")}>
              Outline
            </Button>
            <Button variant="secondary" size="sm" onPress={() => alert("Small button pressed!")}>
              Small Button
            </Button>
            <Button variant="ghost" size="lg" onPress={() => alert("Large button pressed!")}>
              Large Ghost
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Form Components</CardTitle>
            <CardDescription>Input, Switch, and Checkbox examples</CardDescription>
          </CardHeader>
          <CardContent className="gap-4">
            <View className="gap-2">
              <Text className="text-sm font-medium">Input</Text>
              <Input placeholder="Enter text here..." />
            </View>

            <Separator />

            <View className="flex-row items-center justify-between">
              <Text className="text-sm font-medium">Switch</Text>
              {/* Switch component requires development build (react-native-reanimated) */}
              {/* <Switch checked={switchValue} onCheckedChange={setSwitchValue} /> */}
              <Text className="text-xs text-muted-foreground">Requires dev build</Text>
            </View>

            <View className="flex-row items-center gap-2">
              <Checkbox checked={checkboxValue} onCheckedChange={setCheckboxValue} />
              <Text className="text-sm">Checkbox</Text>
            </View>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Badges</CardTitle>
            <CardDescription>Different badge variants</CardDescription>
          </CardHeader>
          <CardContent>
            <View className="flex-row flex-wrap gap-2">
              <Badge>Default</Badge>
              <Badge variant="secondary">Secondary</Badge>
              <Badge variant="destructive">Destructive</Badge>
              <Badge variant="outline">Outline</Badge>
            </View>
          </CardContent>
        </Card>
      </ScrollView>
    </View>
  );
}
