import * as AspectRatioPrimitive from '@rn-primitives/aspect-ratio';
import * as React from 'react';

function AspectRatio({
  ...props
}: AspectRatioPrimitive.RootProps & React.RefAttributes<AspectRatioPrimitive.RootRef>) {
  return <AspectRatioPrimitive.Root {...props} />;
}

export { AspectRatio };
