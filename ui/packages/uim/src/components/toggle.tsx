import { Icon } from './icon';
import { TextClassContext } from './text';
import { cn } from '../lib/utils';
import * as TogglePrimitive from '@rn-primitives/toggle';
import { cva, type VariantProps } from 'class-variance-authority';
import * as React from 'react';

const toggleVariants = cva(
  'active:bg-muted group flex flex-row items-center justify-center gap-2 rounded-md',
  {
    variants: {
      variant: {
        default: 'bg-transparent',
        outline: 'border-input active:bg-accent border bg-transparent shadow-sm shadow-black/5',
      },
      size: {
        default: 'h-10 min-w-10 px-2.5',
        sm: 'h-9 min-w-9 px-2',
        lg: 'h-11 min-w-11 px-3',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'default',
    },
  }
);

function Toggle({
  className,
  variant,
  size,
  ...props
}: TogglePrimitive.RootProps &
  VariantProps<typeof toggleVariants> &
  React.RefAttributes<TogglePrimitive.RootRef>) {
  return (
    <TextClassContext.Provider
      value={cn(
        'text-sm text-foreground font-medium',
        props.pressed && 'text-accent-foreground',
        className
      )}>
      <TogglePrimitive.Root
        className={cn(
          toggleVariants({ variant, size }),
          props.disabled && 'opacity-50',
          props.pressed && 'bg-accent',
          className
        )}
        {...props}
      />
    </TextClassContext.Provider>
  );
}

function ToggleIcon({ className, ...props }: React.ComponentProps<typeof Icon>) {
  const textClass = React.useContext(TextClassContext);
  return <Icon className={cn('size-4 shrink-0', textClass, className)} {...props} />;
}

export { Toggle, ToggleIcon, toggleVariants };
