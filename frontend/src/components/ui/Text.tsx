import { Slot } from '@radix-ui/react-slot'
import { cva, type VariantProps } from 'class-variance-authority'
import * as React from 'react'

import { cn } from '@/lib/utils'

const textVariants = cva('', {
  variants: {
    variant: {
      default: 'leading-7 [&:not(:first-child)]:mt-6',
      muted: 'text-muted-foreground text-sm',
    },
  },
  defaultVariants: {
    variant: 'default',
  },
})

function Text({
  className,
  variant = 'default',
  asChild = false,
  ...props
}: React.ComponentProps<'p'> & VariantProps<typeof textVariants> & { asChild?: boolean }) {
  const textVariant = variant ?? 'default'
  const Comp: React.ElementType = asChild ? Slot : 'p'

  return (
    <Comp
      className={cn(textVariants({ variant: textVariant, className }))}
      data-slot="text"
      data-variant={textVariant}
      {...props}
    />
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export { Text, textVariants }
