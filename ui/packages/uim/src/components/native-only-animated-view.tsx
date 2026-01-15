import * as React from 'react';
import Animated from 'react-native-reanimated';

/**
 * This component is used to wrap animated views that should only be animated on native.
 * @param props - The props for the animated view.
 * @returns The animated view.
 * @example
 * <NativeOnlyAnimatedView entering={FadeIn} exiting={FadeOut}>
 *   <Text>I am only animated on native</Text>
 * </NativeOnlyAnimatedView>
 */
function NativeOnlyAnimatedView(
  props: React.ComponentProps<typeof Animated.View> & React.RefAttributes<Animated.View>
) {
  return <Animated.View {...props} />;
}

export { NativeOnlyAnimatedView };
