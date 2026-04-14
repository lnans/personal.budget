import React from 'react'
import { useTranslation } from 'react-i18next'

import { useGetAccounts } from '@/api/endpoints/AccountsEndpoints'
import {
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from '@/components/ui/Sidebar'
import { cn, cnAccountCurrencyColor, formatCurrency } from '@/lib/utils'
import type { GetAccountsResponseDto } from '@/types/accounts/responses/GetAccountsResponseDto'

import { useAccountsStore } from '../stores/accountsStore'

import { AccountsSettingsMenu } from './AccountsSettingsMenu'

function AccountsList() {
  const { t } = useTranslation()

  const accountsQuery = useGetAccounts()
  const checkingAccounts = accountsQuery.data?.filter((account) => account.type === 'Checking') ?? []
  const savingsAccounts = accountsQuery.data?.filter((account) => account.type === 'Savings') ?? []

  return (
    <>
      <AccountGroup accounts={checkingAccounts} label={t('accounts.type.Checking')} labelNoData={t('common.noData')} />
      <AccountGroup accounts={savingsAccounts} label={t('accounts.type.Savings')} labelNoData={t('common.noData')} />
    </>
  )
}

type AccountGroupProps = {
  accounts: GetAccountsResponseDto[]
  label: string
  labelNoData: string
}

function AccountGroup({ accounts, label, labelNoData }: AccountGroupProps) {
  const { setSelectedAccountId } = useAccountsStore((state) => state.actions)
  const selectedAccountId = useAccountsStore((state) => state.selectedAccountId)
  const [openMenuAccountId, setOpenMenuAccountId] = React.useState<string | null>(null)

  return (
    <SidebarGroup className="py-0">
      <SidebarGroupLabel>{label}</SidebarGroupLabel>
      <SidebarGroupContent className="flex flex-col gap-2">
        <SidebarMenu>
          {accounts.length === 0 ? (
            <SidebarMenuItem>
              <SidebarMenuButton disabled>{labelNoData}</SidebarMenuButton>
            </SidebarMenuItem>
          ) : (
            accounts.map((account) => (
              <SidebarMenuItem key={account.id}>
                <SidebarMenuButton
                  className="group/menu-button relative pl-4"
                  isActive={selectedAccountId === account.id}
                  size="lg"
                  onClick={() => setSelectedAccountId(account.id)}
                >
                  <div className="flex flex-col">
                    <span>{account.name}</span>
                    <span className="text-xs text-muted-foreground">{account.bank}</span>
                  </div>
                  <span
                    className={cn(
                      cnAccountCurrencyColor(account.balance),
                      'ms-auto pr-8 font-semibold transition-[padding,transform] md:pr-0 md:translate-x-0 md:group-hover/menu-button:-translate-x-4 md:group-focus-visible/menu-button:-translate-x-4 md:group-active/menu-button:-translate-x-4 md:group-hover/menu-button:pr-3 md:group-focus-visible/menu-button:pr-3 md:group-active/menu-button:pr-3',
                      openMenuAccountId === account.id && 'md:-translate-x-4 md:pr-3'
                    )}
                  >
                    {formatCurrency(account.balance)}
                  </span>

                  <AccountsSettingsMenu
                    accountId={account.id}
                    onOpenChange={(open) => {
                      setOpenMenuAccountId((current) => (open ? account.id : current === account.id ? null : current))
                    }}
                  />
                </SidebarMenuButton>
              </SidebarMenuItem>
            ))
          )}
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  )
}

export { AccountsList }
