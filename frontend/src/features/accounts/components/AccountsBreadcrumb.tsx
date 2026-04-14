import { useTranslation } from 'react-i18next'

import { useGetAccountById } from '@/api/endpoints/AccountsEndpoints'
import { Breadcrumb, BreadcrumbItem, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/Breadcrumb'

import { useAccountsStore } from '../stores/accountsStore'

function AccountsBreadcrumb() {
  const { t } = useTranslation()

  const selectedAccountId = useAccountsStore((state) => state.selectedAccountId)
  const account = useGetAccountById(selectedAccountId)

  return (
    <Breadcrumb>
      <BreadcrumbList>
        <BreadcrumbItem>
          <BreadcrumbPage>{t('operations.title')}</BreadcrumbPage>
        </BreadcrumbItem>
        <BreadcrumbSeparator />
        {account ? (
          <>
            <BreadcrumbItem>
              <BreadcrumbPage>{account.bank}</BreadcrumbPage>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbPage>{account.name}</BreadcrumbPage>
            </BreadcrumbItem>
          </>
        ) : (
          <BreadcrumbItem>
            <BreadcrumbPage>{t('accounts.allAccounts')}</BreadcrumbPage>
          </BreadcrumbItem>
        )}
      </BreadcrumbList>
    </Breadcrumb>
  )
}

export { AccountsBreadcrumb }
