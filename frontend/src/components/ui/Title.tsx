import { Slot } from '@radix-ui/react-slot'
import { cva, type VariantProps } from 'class-variance-authority'
import * as React from 'react'

import { cn } from '@/lib/utils'

const titleVariants = cva('scroll-m-20 tracking-tight', {
  variants: {
    variant: {
      h1: 'text-center text-4xl font-extrabold text-balance',
      h2: 'border-b pb-2 text-3xl font-semibold first:mt-0',
      h3: 'text-2xl font-semibold',
      h4: 'text-xl font-semibold',
    },
  },
  defaultVariants: {
    variant: 'h1',
  },
})

function Title({
  className,
  variant = 'h1',
  asChild = false,
  ...props
}: React.ComponentProps<'h1'> & VariantProps<typeof titleVariants> & { asChild?: boolean }) {
  const titleVariant = variant ?? 'h1'
  const Comp: React.ElementType = asChild ? Slot : titleVariant

  return (
    <Comp
      className={cn(titleVariants({ variant: titleVariant, className }))}
      data-slot="title"
      data-variant={titleVariant}
      {...props}
    />
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export { Title, titleVariants }
