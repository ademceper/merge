import { Icon } from './icon';
import { NativeOnlyAnimatedView } from './native-only-animated-view';
import { TextClassContext } from './text';
import { cn } from '../lib/utils';
import * as ContextMenuPrimitive from '@rn-primitives/context-menu';
import { Check, ChevronDown, ChevronRight, ChevronUp } from 'lucide-react-native';
import * as React from 'react';
import {
  Platform,
  type StyleProp,
  StyleSheet,
  Text,
  type TextProps,
  View,
  type ViewStyle,
} from 'react-native';
import { FadeIn } from 'react-native-reanimated';
import { FullWindowOverlay as RNFullWindowOverlay } from 'react-native-screens';

export const ContextMenu = ContextMenuPrimitive.Root;
export const ContextMenuTrigger = ContextMenuPrimitive.Trigger;
export const ContextMenuGroup = ContextMenuPrimitive.Group;
export const ContextMenuSub = ContextMenuPrimitive.Sub;
export const ContextMenuRadioGroup = ContextMenuPrimitive.RadioGroup;

export function ContextMenuSubTrigger({
  className,
  inset,
  children,
  iconClassName,
  ...props
}: ContextMenuPrimitive.SubTriggerProps &
  React.RefAttributes<ContextMenuPrimitive.SubTriggerRef> & {
    children?: React.ReactNode;
    iconClassName?: string;
    inset?: boolean;
  }) {
  const { open } = ContextMenuPrimitive.useSubContext();
  const icon = open ? ChevronUp : ChevronDown;
  return (
    <TextClassContext.Provider
      value={cn(
        'text-sm group-active:text-accent-foreground',
        open && 'text-accent-foreground'
      )}>
      <ContextMenuPrimitive.SubTrigger
        className={cn(
          'active:bg-accent group flex flex-row items-center rounded-sm px-2 py-2',
          open && cn('bg-accent', 'mb-1'),
          inset && 'pl-8'
        )}
        {...props}>
        <>{children}</>
        <Icon as={icon} className={cn('text-foreground ml-auto size-4 shrink-0', iconClassName)} />
      </ContextMenuPrimitive.SubTrigger>
    </TextClassContext.Provider>
  );
}

export function ContextMenuSubContent({
  className,
  ...props
}: ContextMenuPrimitive.SubContentProps & React.RefAttributes<ContextMenuPrimitive.SubContentRef>) {
  return (
    <NativeOnlyAnimatedView entering={FadeIn}>
      <ContextMenuPrimitive.SubContent
        className={cn(
          'bg-popover border-border overflow-hidden rounded-md border p-1 shadow-lg shadow-black/5',
          className
        )}
        {...props}
      />
    </NativeOnlyAnimatedView>
  );
}

const FullWindowOverlay = Platform.OS === 'ios' ? RNFullWindowOverlay : React.Fragment;

export function ContextMenuContent({
  className,
  overlayClassName,
  overlayStyle,
  portalHost,
  ...props
}: ContextMenuPrimitive.ContentProps &
  React.RefAttributes<ContextMenuPrimitive.ContentRef> & {
    overlayStyle?: StyleProp<ViewStyle>;
    overlayClassName?: string;
    portalHost?: string;
  }) {
  return (
    <ContextMenuPrimitive.Portal hostName={portalHost}>
      <FullWindowOverlay>
        <ContextMenuPrimitive.Overlay
          style={
            overlayStyle
              ? StyleSheet.flatten([
                  StyleSheet.absoluteFill,
                  overlayStyle as typeof StyleSheet.absoluteFill,
                ])
              : StyleSheet.absoluteFill
          }
          className={overlayClassName}>
          <NativeOnlyAnimatedView entering={FadeIn}>
            <TextClassContext.Provider value="text-popover-foreground">
              <ContextMenuPrimitive.Content
                className={cn(
                  'bg-popover border-border min-w-[8rem] overflow-hidden rounded-md border p-1 shadow-lg shadow-black/5',
                  className
                )}
                {...props}
              />
            </TextClassContext.Provider>
          </NativeOnlyAnimatedView>
        </ContextMenuPrimitive.Overlay>
      </FullWindowOverlay>
    </ContextMenuPrimitive.Portal>
  );
}

export function ContextMenuItem({
  className,
  inset,
  variant,
  ...props
}: ContextMenuPrimitive.ItemProps &
  React.RefAttributes<ContextMenuPrimitive.ItemRef> & {
    className?: string;
    inset?: boolean;
    variant?: 'default' | 'destructive';
  }) {
  return (
    <TextClassContext.Provider
      value={cn(
        'text-sm text-popover-foreground group-active:text-popover-foreground',
        variant === 'destructive' && 'text-destructive group-active:text-destructive'
      )}>
      <ContextMenuPrimitive.Item
        className={cn(
          'active:bg-accent group relative flex flex-row items-center gap-2 rounded-sm px-2 py-2',
          variant === 'destructive' && 'active:bg-destructive/10 dark:active:bg-destructive/20',
          props.disabled && 'opacity-50',
          inset && 'pl-8',
          className
        )}
        {...props}
      />
    </TextClassContext.Provider>
  );
}

export function ContextMenuCheckboxItem({
  className,
  children,
  ...props
}: ContextMenuPrimitive.CheckboxItemProps &
  React.RefAttributes<ContextMenuPrimitive.CheckboxItemRef> & {
    children?: React.ReactNode;
  }) {
  return (
    <TextClassContext.Provider value="text-sm text-popover-foreground group-active:text-accent-foreground">
      <ContextMenuPrimitive.CheckboxItem
        className={cn(
          'active:bg-accent group relative flex flex-row items-center gap-2 rounded-sm py-2 pl-8 pr-2',
          props.disabled && 'opacity-50',
          className
        )}
        {...props}>
        <View className="absolute left-2 flex h-3.5 w-3.5 items-center justify-center">
          <ContextMenuPrimitive.ItemIndicator>
            <Icon
              as={Check}
              className={cn(
                'text-foreground size-4'
              )}
            />
          </ContextMenuPrimitive.ItemIndicator>
        </View>
        <>{children}</>
      </ContextMenuPrimitive.CheckboxItem>
    </TextClassContext.Provider>
  );
}

export function ContextMenuRadioItem({
  className,
  children,
  ...props
}: ContextMenuPrimitive.RadioItemProps &
  React.RefAttributes<ContextMenuPrimitive.RadioItemRef> & {
    children?: React.ReactNode;
  }) {
  return (
    <TextClassContext.Provider value="text-sm text-popover-foreground group-active:text-accent-foreground">
      <ContextMenuPrimitive.RadioItem
        className={cn(
          'active:bg-accent group relative flex flex-row items-center gap-2 rounded-sm py-2 pl-8 pr-2',
          props.disabled && 'opacity-50',
          className
        )}
        {...props}>
        <View className="absolute left-2 flex h-3.5 w-3.5 items-center justify-center">
          <ContextMenuPrimitive.ItemIndicator>
            <View className="bg-foreground h-2 w-2 rounded-full" />
          </ContextMenuPrimitive.ItemIndicator>
        </View>
        <>{children}</>
      </ContextMenuPrimitive.RadioItem>
    </TextClassContext.Provider>
  );
}

export function ContextMenuLabel({
  className,
  inset,
  ...props
}: ContextMenuPrimitive.LabelProps &
  React.RefAttributes<ContextMenuPrimitive.LabelRef> & {
    className?: string;
    inset?: boolean;
  }) {
  return (
    <ContextMenuPrimitive.Label
      className={cn(
        'text-foreground px-2 py-2 text-sm font-medium',
        inset && 'pl-8',
        className
      )}
      {...props}
    />
  );
}

export function ContextMenuSeparator({
  className,
  ...props
}: ContextMenuPrimitive.SeparatorProps & React.RefAttributes<ContextMenuPrimitive.SeparatorRef>) {
  return (
    <ContextMenuPrimitive.Separator
      className={cn('bg-border -mx-1 my-1 h-px', className)}
      {...props}
    />
  );
}

export function ContextMenuShortcut({ className, ...props }: TextProps & React.RefAttributes<Text>) {
  return (
    <Text
      className={cn('text-muted-foreground ml-auto text-xs tracking-widest', className)}
      {...props}
    />
  );
}