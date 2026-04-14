import { ChartPieIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'

import { useGetAccounts } from '@/api/endpoints/AccountsEndpoints'
import { SidebarGroup, SidebarGroupContent, SidebarMenu, SidebarMenuButton } from '@/components/ui/Sidebar'
import { cn, cnAccountCurrencyColor, formatCurrency } from '@/lib/utils'

import { useAccountsStore } from '../stores/accountsStore'

function AccountsNetWorth() {
  const { setSelectedAccountId } = useAccountsStore((state) => state.actions)
  const selectedAccountId = useAccountsStore((state) => state.selectedAccountId)
  const { t } = useTranslation()

  const accountsQuery = useGetAccounts()
  const netWorth = accountsQuery.data?.reduce((acc, account) => acc + account.balance, 0) ?? 0

  return (
    <SidebarGroup>
      <SidebarGroupContent>
        <SidebarMenu>
          <SidebarMenuButton
            className="border"
            isActive={selectedAccountId === null}
            size="lg"
            onClick={() => setSelectedAccountId(null)}
          >
            <ChartPieIcon />
            <span>{t('accounts.netWorth')}</span>
            <span className={cn(cnAccountCurrencyColor(netWorth), 'ms-auto font-semibold')}>{formatCurrency(netWorth)}</span>
          </SidebarMenuButton>
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  )
}

export { AccountsNetWorth }
