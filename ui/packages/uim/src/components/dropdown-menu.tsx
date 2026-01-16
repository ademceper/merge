import { Icon } from './icon';
import { NativeOnlyAnimatedView } from './native-only-animated-view';
import { TextClassContext } from './text';
import { cn } from '../lib/utils';
import * as DropdownMenuPrimitive from '@rn-primitives/dropdown-menu';
import { Check, ChevronDown, ChevronUp } from 'lucide-react-native';
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

export const DropdownMenu = DropdownMenuPrimitive.Root;

export const DropdownMenuTrigger = DropdownMenuPrimitive.Trigger;

export const DropdownMenuGroup = DropdownMenuPrimitive.Group;

export const DropdownMenuPortal = DropdownMenuPrimitive.Portal;

export const DropdownMenuSub = DropdownMenuPrimitive.Sub;

export const DropdownMenuRadioGroup = DropdownMenuPrimitive.RadioGroup;

export function DropdownMenuSubTrigger({
  className,
  inset,
  children,
  iconClassName,
  ...props
}: DropdownMenuPrimitive.SubTriggerProps &
  React.RefAttributes<DropdownMenuPrimitive.SubTriggerRef> & {
    children?: React.ReactNode;
    iconClassName?: string;
    inset?: boolean;
  }) {
  const { open } = DropdownMenuPrimitive.useSubContext();
  const icon = open ? ChevronUp : ChevronDown;
  return (
    <TextClassContext.Provider
      value={cn(
        'text-sm group-active:text-accent-foreground',
        open && 'text-accent-foreground'
      )}>
      <DropdownMenuPrimitive.SubTrigger
        className={cn(
          'active:bg-accent group flex flex-row items-center rounded-sm px-2 py-2',
          open && 'bg-accent',
          inset && 'pl-8'
        )}
        {...props}>
        <>{children}</>
        <Icon as={icon} className={cn('text-foreground ml-auto size-4 shrink-0', iconClassName)} />
      </DropdownMenuPrimitive.SubTrigger>
    </TextClassContext.Provider>
  );
}

export function DropdownMenuSubContent({
  className,
  ...props
}: DropdownMenuPrimitive.SubContentProps &
  React.RefAttributes<DropdownMenuPrimitive.SubContentRef>) {
  return (
    <NativeOnlyAnimatedView entering={FadeIn}>
      <DropdownMenuPrimitive.SubContent
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

export function DropdownMenuContent({
  className,
  overlayClassName,
  overlayStyle,
  portalHost,
  ...props
}: DropdownMenuPrimitive.ContentProps &
  React.RefAttributes<DropdownMenuPrimitive.ContentRef> & {
    overlayStyle?: StyleProp<ViewStyle>;
    overlayClassName?: string;
    portalHost?: string;
  }) {
  return (
    <DropdownMenuPrimitive.Portal hostName={portalHost}>
      <FullWindowOverlay>
        <DropdownMenuPrimitive.Overlay
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
              <DropdownMenuPrimitive.Content
                className={cn(
                  'bg-popover border-border min-w-[8rem] overflow-hidden rounded-md border p-1 shadow-lg shadow-black/5',
                  className
                )}
                {...props}
              />
            </TextClassContext.Provider>
          </NativeOnlyAnimatedView>
        </DropdownMenuPrimitive.Overlay>
      </FullWindowOverlay>
    </DropdownMenuPrimitive.Portal>
  );
}

export function DropdownMenuItem({
  className,
  inset,
  variant,
  ...props
}: DropdownMenuPrimitive.ItemProps &
  React.RefAttributes<DropdownMenuPrimitive.ItemRef> & {
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
      <DropdownMenuPrimitive.Item
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

export function DropdownMenuCheckboxItem({
  className,
  children,
  ...props
}: DropdownMenuPrimitive.CheckboxItemProps &
  React.RefAttributes<DropdownMenuPrimitive.CheckboxItemRef> & {
    children?: React.ReactNode;
  }) {
  return (
    <TextClassContext.Provider value="text-sm text-popover-foreground group-active:text-accent-foreground">
      <DropdownMenuPrimitive.CheckboxItem
        className={cn(
          'active:bg-accent group relative flex flex-row items-center gap-2 rounded-sm py-2 pl-8 pr-2',
          props.disabled && 'opacity-50',
          className
        )}
        {...props}>
        <View className="absolute left-2 flex h-3.5 w-3.5 items-center justify-center">
          <DropdownMenuPrimitive.ItemIndicator>
            <Icon
              as={Check}
              className={cn(
                'text-foreground size-4'
              )}
            />
          </DropdownMenuPrimitive.ItemIndicator>
        </View>
        <>{children}</>
      </DropdownMenuPrimitive.CheckboxItem>
    </TextClassContext.Provider>
  );
}

export function DropdownMenuRadioItem({
  className,
  children,
  ...props
}: DropdownMenuPrimitive.RadioItemProps &
  React.RefAttributes<DropdownMenuPrimitive.RadioItemRef> & {
    children?: React.ReactNode;
  }) {
  return (
    <TextClassContext.Provider value="text-sm text-popover-foreground group-active:text-accent-foreground">
      <DropdownMenuPrimitive.RadioItem
        className={cn(
          'active:bg-accent group relative flex flex-row items-center gap-2 rounded-sm py-2 pl-8 pr-2',
          props.disabled && 'opacity-50',
          className
        )}
        {...props}>
        <View className="absolute left-2 flex h-3.5 w-3.5 items-center justify-center">
          <DropdownMenuPrimitive.ItemIndicator>
            <View className="bg-foreground h-2 w-2 rounded-full" />
          </DropdownMenuPrimitive.ItemIndicator>
        </View>
        <>{children}</>
      </DropdownMenuPrimitive.RadioItem>
    </TextClassContext.Provider>
  );
}

export function DropdownMenuLabel({
  className,
  inset,
  ...props
}: DropdownMenuPrimitive.LabelProps &
  React.RefAttributes<DropdownMenuPrimitive.LabelRef> & {
    className?: string;
    inset?: boolean;
  }) {
  return (
    <DropdownMenuPrimitive.Label
      className={cn(
        'text-foreground px-2 py-2 text-sm font-medium',
        inset && 'pl-8',
        className
      )}
      {...props}
    />
  );
}

export function DropdownMenuSeparator({
  className,
  ...props
}: DropdownMenuPrimitive.SeparatorProps & React.RefAttributes<DropdownMenuPrimitive.SeparatorRef>) {
  return (
    <DropdownMenuPrimitive.Separator
      className={cn('bg-border -mx-1 my-1 h-px', className)}
      {...props}
    />
  );
}

export function DropdownMenuShortcut({ className, ...props }: TextProps & React.RefAttributes<Text>) {
  return (
    <Text
      className={cn('text-muted-foreground ml-auto text-xs tracking-widest', className)}
      {...props}
    />
  );
}
