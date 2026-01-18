import { View, useColorScheme, KeyboardAvoidingView, Platform } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { StatusBar } from "expo-status-bar";
import { useRouter } from "expo-router";
import * as React from "react";
import { Routes } from "./types/routes";
import { Text } from "@merge/uim/components/text";
import { Input } from "@merge/uim/components/input";
import { Button } from "@merge/uim/components/button";
import Animated, {
  useAnimatedStyle,
  useSharedValue,
  withTiming,
  Easing,
} from "react-native-reanimated";

export default function ForgotPasswordScreen() {
  const colorScheme = useColorScheme();
  const insets = useSafeAreaInsets();
  const router = useRouter();
  const [email, setEmail] = React.useState("");
  const [isLoading, setIsLoading] = React.useState(false);
  const [isEmailSent, setIsEmailSent] = React.useState(false);

  const isDark = colorScheme === "dark";
  const opacity = useSharedValue(0);
  const translateY = useSharedValue(20);

  React.useEffect(() => {
    opacity.value = withTiming(1, {
      duration: 300,
      easing: Easing.out(Easing.ease),
    });
    translateY.value = withTiming(0, {
      duration: 300,
      easing: Easing.out(Easing.ease),
    });
  }, []);

  const animatedStyle = useAnimatedStyle(() => ({
    opacity: opacity.value,
    transform: [{ translateY: translateY.value }],
  }));

  const handleResetPassword = async () => {
    if (!email.trim()) {
      return;
    }

    setIsLoading(true);
    // TODO: Implement actual password reset logic
    setTimeout(() => {
      setIsLoading(false);
      setIsEmailSent(true);
    }, 1000);
  };

  const handleBackToLogin = () => {
    router.push(Routes.login);
  };

  const handleResendEmail = () => {
    setIsEmailSent(false);
    handleResetPassword();
  };

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === "ios" ? "padding" : "height"}
      className={`flex-1 ${isDark ? "bg-[#101022]" : "bg-white"}`}
      style={{ paddingTop: 0, paddingBottom: 0 }}>
      <View
        className="flex-1"
        style={{ paddingTop: insets.top, paddingBottom: insets.bottom }}>
        <StatusBar style={isDark ? "light" : "dark"} />

        <Animated.View
          style={[animatedStyle]}
          className="flex-1 flex-col items-center justify-center px-6">
          <View className="w-full max-w-sm" style={{ gap: 32 }}>
            {/* Header */}
            <View className="items-center" style={{ gap: 8 }}>
              <Text
                className={`text-4xl font-bold uppercase tracking-tight leading-tight ${
                  isDark ? "text-white" : "text-[#111118]"
                }`}>
                {isEmailSent ? "Check Email" : "Forgot Password"}
              </Text>
              <Text
                className={`text-base font-light text-center leading-relaxed ${
                  isDark ? "text-white/70" : "text-[#111118]/70"
                }`}
                style={{ maxWidth: 280 }}>
                {isEmailSent
                  ? "We've sent a password reset link to your email address"
                  : "Enter your email address and we'll send you a link to reset your password"}
              </Text>
            </View>

            {!isEmailSent ? (
              <>
                {/* Form */}
                <View className="w-full" style={{ gap: 20 }}>
                  {/* Email Input */}
                  <View style={{ gap: 8 }}>
                    <Input
                      placeholder="Email"
                      value={email}
                      onChangeText={setEmail}
                      keyboardType="email-address"
                      autoCapitalize="none"
                      autoComplete="email"
                      className={`h-12 rounded-lg px-4 ${
                        isDark
                          ? "border-white/10 bg-white/5 text-white"
                          : "border-[#111118]/10 bg-transparent text-[#111118]"
                      }`}
                      placeholderTextColor={isDark ? "rgba(255, 255, 255, 0.4)" : "rgba(17, 17, 24, 0.4)"}
                    />
                  </View>
                </View>

                {/* Reset Password Button */}
                <Button
                  onPress={handleResetPassword}
                  disabled={isLoading || !email.trim()}
                  className={`w-full h-14 rounded-xl ${
                    isDark ? "bg-white" : "bg-[#111118]"
                  }`}>
                  <Text
                    className={`text-base font-bold uppercase tracking-wider ${
                      isDark ? "text-[#111118]" : "text-white"
                    }`}>
                    {isLoading ? "Sending..." : "Send Reset Link"}
                  </Text>
                </Button>
              </>
            ) : (
              <>
                {/* Success State */}
                <View className="w-full" style={{ gap: 24 }}>
                  <View className="items-center" style={{ gap: 16 }}>
                    <View
                      className={`w-20 h-20 rounded-full flex items-center justify-center ${
                        isDark ? "bg-white/10" : "bg-[#111118]/10"
                      }`}>
                      <Text
                        className={`text-4xl ${
                          isDark ? "text-white" : "text-[#111118]"
                        }`}>
                        ✓
                      </Text>
                    </View>
                  </View>

                  {/* Resend Email Button */}
                  <Button
                    variant="outline"
                    onPress={handleResendEmail}
                    disabled={isLoading}
                    className={`w-full h-14 rounded-xl border ${
                      isDark
                        ? "border-white/20 bg-transparent"
                        : "border-[#111118]/20 bg-transparent"
                    }`}>
                    <Text
                      className={`text-base font-bold uppercase tracking-wider ${
                        isDark ? "text-white" : "text-[#111118]"
                      }`}>
                      {isLoading ? "Sending..." : "Resend Email"}
                    </Text>
                  </Button>
                </View>
              </>
            )}

            {/* Back to Login Link */}
            <View className="flex-row items-center justify-center" style={{ gap: 4 }}>
              <Text
                className={`text-sm font-light ${
                  isDark ? "text-white/70" : "text-[#111118]/70"
                }`}>
                Remember your password?
              </Text>
              <Button
                variant="ghost"
                onPress={handleBackToLogin}
                hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}
                className="h-auto p-0">
                <Text
                  className={`text-sm font-medium ${
                    isDark ? "text-white" : "text-[#111118]"
                  }`}>
                  Login
                </Text>
              </Button>
            </View>
          </View>
        </Animated.View>
      </View>
    </KeyboardAvoidingView>
  );
}
