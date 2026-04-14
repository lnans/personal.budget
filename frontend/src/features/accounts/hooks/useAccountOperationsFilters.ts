import React from 'react'

import type { GetAccountOperationsQueryDto } from '@/types/accountOperations/queries/GetAccountOperationsQueryDto'

import { useAccountsStore } from '../stores/accountsStore'

export function useAccountOperationsFilters() {
  const selectedAccountId = useAccountsStore((state) => state.selectedAccountId)

  const data = React.useMemo<GetAccountOperationsQueryDto>(() => ({ accountId: selectedAccountId }), [selectedAccountId])

  return { data }
}
