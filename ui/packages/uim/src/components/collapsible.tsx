import * as CollapsiblePrimitive from '@rn-primitives/collapsible';
import * as React from 'react';

export function Collapsible({
  ...props
}: CollapsiblePrimitive.RootProps & React.RefAttributes<CollapsiblePrimitive.RootRef>) {
  return <CollapsiblePrimitive.Root {...props} />;
}

export function CollapsibleTrigger({
  ...props
}: CollapsiblePrimitive.TriggerProps & React.RefAttributes<CollapsiblePrimitive.TriggerRef>) {
  return <CollapsiblePrimitive.Trigger {...props} />;
}

export function CollapsibleContent({
  ...props
}: CollapsiblePrimitive.ContentProps & React.RefAttributes<CollapsiblePrimitive.ContentRef>) {
  return <CollapsiblePrimitive.Content {...props} />;
}

