import { PlusCircleIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'

import { Item, ItemContent, ItemDescription, ItemHeader } from '@/components/ui/Item'
import { SidebarGroup, SidebarGroupContent, SidebarMenu, SidebarMenuItem } from '@/components/ui/Sidebar'

import { useAccountsStore } from '../stores/accountsStore'

function AccountsAddButton() {
  const { t } = useTranslation()
  const { toggleIsCreatingAccount } = useAccountsStore((state) => state.actions)

  return (
    <SidebarGroup>
      <SidebarGroupContent className="flex flex-col gap-2">
        <SidebarMenu>
          <SidebarMenuItem>
            <Item
              className="mt-2 p-2.5 gap-2 border-dashed opacity-50 hover:opacity-80 transition duration-150 ease-in-out hover:-translate-y-0.5"
              role="button"
              variant="outline"
              onClick={toggleIsCreatingAccount}
            >
              <ItemHeader className="justify-center">
                <PlusCircleIcon size={18} />
              </ItemHeader>
              <ItemContent>
                <ItemDescription className="text-center text-white text-xs">{t('accounts.actions.add.title')}</ItemDescription>
              </ItemContent>
            </Item>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  )
}

export { AccountsAddButton }
