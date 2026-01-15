import { Stack } from "expo-router"
import "../global.css"
import { useFonts } from "expo-font"
import * as SplashScreen from "expo-splash-screen"
import { useEffect } from "react"
import {
  SpaceGrotesk_300Light,
  SpaceGrotesk_400Regular,
  SpaceGrotesk_500Medium,
  SpaceGrotesk_700Bold,
} from "@expo-google-fonts/space-grotesk"

SplashScreen.preventAutoHideAsync()

const AppLayout = () => {
  const [fontsLoaded, fontError] = useFonts({
    "SpaceGrotesk-Light": SpaceGrotesk_300Light,
    "SpaceGrotesk-Regular": SpaceGrotesk_400Regular,
    "SpaceGrotesk-Medium": SpaceGrotesk_500Medium,
    "SpaceGrotesk-Bold": SpaceGrotesk_700Bold,
  })

  useEffect(() => {
    if (fontsLoaded || fontError) {
      SplashScreen.hideAsync()
    }
  }, [fontsLoaded, fontError])

  if (!fontsLoaded && !fontError) {
    return null
  }

  return (
    <Stack
      screenOptions={{
        headerShown: false,
      }}
    />
  )
}

export default AppLayout
