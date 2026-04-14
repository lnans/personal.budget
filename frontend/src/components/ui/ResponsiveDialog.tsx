import { useMediaQuery } from '@/hooks/useMediaQuery'

import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from './Dialog'
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle } from './Drawer'

type ResponsiveDialog = {
  title: string
  description?: string
  children: React.ReactNode
  open: boolean
  onOpenChange: (open: boolean) => void
}

function ResponsiveDialog({ title, description, children, open, onOpenChange }: ResponsiveDialog) {
  const isDesktop = useMediaQuery('(min-width: 768px)')

  if (isDesktop) {
    return (
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>{title}</DialogTitle>
            {description && <DialogDescription>{description}</DialogDescription>}
          </DialogHeader>

          {children}
        </DialogContent>
      </Dialog>
    )
  }

  return (
    <Drawer open={open} onOpenChange={onOpenChange}>
      <DrawerContent>
        <DrawerHeader className="text-left">
          <DrawerTitle>{title}</DrawerTitle>
          {description && <DrawerDescription>{description}</DrawerDescription>}
        </DrawerHeader>

        {children}
      </DrawerContent>
    </Drawer>
  )
}

export { ResponsiveDialog }
