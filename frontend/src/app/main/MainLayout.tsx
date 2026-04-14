import { GalleryVerticalEnd } from 'lucide-react'
import * as React from 'react'
import { useTranslation } from 'react-i18next'
import { Link, Outlet } from 'react-router-dom'

import { Separator } from '@/components/ui/Separator'
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
} from '@/components/ui/Sidebar'
import { AccountsAddButton } from '@/features/accounts/components/AccountsAddButton'
import { AccountsBreadcrumb } from '@/features/accounts/components/AccountsBreadcrumb'
import { AccountsList } from '@/features/accounts/components/AccountsList'
import { AccountsNetWorth } from '@/features/accounts/components/AccountsNetWorth'
import { AccountsAddFormDialog } from '@/features/accounts/components/forms/AccountsAddFormDialog'
import { AccountsDeleteFormDialog } from '@/features/accounts/components/forms/AccountsDeleteFormDialog'
import { AccountsPatchFormDialog } from '@/features/accounts/components/forms/AccountsPatchFormDialog'
import { useAccountsStore } from '@/features/accounts/stores/accountsStore'
import { AuthProfileButton } from '@/features/authentication/components/AuthProfileButton'
import { useSearchParams } from '@/hooks/useSearchParams'

const SIDEBAR_SCALED_WIDTH = 80
const HEADER_SCALED_HEIGHT = 12

function MainLayout() {
  const { t } = useTranslation()

  const { setSelectedAccountId } = useAccountsStore((state) => state.actions)
  const selectedAccountId = useAccountsStore((state) => state.selectedAccountId)
  const [queryParamAccountId, setQueryParamAccountId] = useSearchParams<string | null>('accountId')

  React.useEffect(() => {
    setSelectedAccountId(queryParamAccountId)
  }, [queryParamAccountId, setSelectedAccountId])

  React.useEffect(() => {
    setQueryParamAccountId(selectedAccountId)
  }, [selectedAccountId, setQueryParamAccountId])

  return (
    <SidebarProvider
      style={
        {
          '--sidebar-width': `calc(var(--spacing) * ${SIDEBAR_SCALED_WIDTH})`,
          '--header-height': `calc(var(--spacing) * ${HEADER_SCALED_HEIGHT})`,
        } as React.CSSProperties
      }
    >
      <Sidebar collapsible="offcanvas" variant="inset">
        <SidebarHeader>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton asChild className="data-[slot=sidebar-menu-button]:p-1.5!" size="lg">
                <Link to="/">
                  <GalleryVerticalEnd className="size-5!" />
                  <span className="text-base font-semibold">{t('app.brand')}</span>
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarHeader>

        <SidebarContent>
          <AccountsNetWorth />
          <AccountsList />

          <AccountsAddButton />
          <AccountsAddFormDialog />
          <AccountsPatchFormDialog />
          <AccountsDeleteFormDialog />
        </SidebarContent>

        <SidebarFooter>
          <AuthProfileButton />
        </SidebarFooter>
      </Sidebar>

      <SidebarInset>
        <header className="flex h-(--header-height) shrink-0 items-center gap-2 border-b transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-(--header-height)">
          <div className="flex w-full items-center gap-1 px-4 lg:gap-2 lg:px-6">
            <SidebarTrigger className="-ml-1" />
            <Separator className="mx-2 data-[orientation=vertical]:h-4" orientation="vertical" />
            <AccountsBreadcrumb />
          </div>
        </header>
        <div className="flex flex-1 flex-col">
          <div className="@container/main flex flex-1 flex-col gap-2">
            <div className="flex flex-col gap-4 p-4 md:gap-6 md:py-6">
              <div>
                <Outlet />
              </div>
            </div>
          </div>
        </div>
      </SidebarInset>
    </SidebarProvider>
  )
}

export { MainLayout }
