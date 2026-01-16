import { cn } from "@merge/ui/lib/utils"
import { SpinnerIcon } from "@phosphor-icons/react"

export function Spinner({ className, ...props }: React.ComponentProps<"svg">) {
  return (
    <SpinnerIcon role="status" aria-label="Loading" className={cn("size-4 animate-spin", className)} {...props} />
  )
}
