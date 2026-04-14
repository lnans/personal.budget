import { ArrowDownToLine, ServerOff } from 'lucide-react'
import React from 'react'
import { useTranslation } from 'react-i18next'

import { useGetInfiniteAccountOperations } from '@/api/endpoints/AccountsEndpoints'
import { Avatar, AvatarFallback } from '@/components/ui/Avatar'
import { Button } from '@/components/ui/Button'
import { Skeleton } from '@/components/ui/Skeleton'
import { Table, TableBody, TableCell, TableRow } from '@/components/ui/Table'
import { Text } from '@/components/ui/Text'
import { Title } from '@/components/ui/Title'
import { ENV } from '@/env'
import { cn, cnOperationCurrencyColor, formatCurrency, formatDate } from '@/lib/utils'
import type { GetAccountOperationsResponseDto } from '@/types/accountOperations/responses/GetAccountOperationsResponseDto'
import type { InfiniteDataGrouped } from '@/types/InfiniteDataGrouped'

import { useAccountOperationsFilters } from '../hooks/useAccountOperationsFilters'

function AccountOperationsList() {
  const { t } = useTranslation()

  const filters = useAccountOperationsFilters()
  const query = useGetInfiniteAccountOperations(filters.data)

  const fetchNextPage = React.useCallback(() => {
    void query.fetchNextPage()
  }, [query])

  const isLoading = query.isLoading
  const hasData = !isLoading && query.data && Object.keys(query.data).length > 0
  const hasNoData = !isLoading && !hasData

  return (
    <div className="flex flex-col gap-4">
      <div>
        <Title variant="h4">{t('operations.title')}</Title>
        <Text variant="muted">{t('operations.lastActivity')}</Text>
      </div>
      {isLoading && <AccountOperationsTableSkeleton />}
      {hasData && (
        <AccountOperationsTable
          data={query.data}
          fetchNextPage={fetchNextPage}
          hasNextPage={!!query.hasNextPage}
          isFetching={query.isFetching}
        />
      )}
      {hasNoData && <AccountOperationsNoData />}
    </div>
  )
}

function AccountOperationsTable({
  data,
  hasNextPage,
  fetchNextPage,
  isFetching,
}: {
  data: InfiniteDataGrouped<GetAccountOperationsResponseDto>
  hasNextPage: boolean
  fetchNextPage: () => void
  isFetching: boolean
}) {
  const { t } = useTranslation()

  return (
    <>
      <Table>
        <TableBody>
          {Object.keys(data).map((date) => (
            <React.Fragment key={date}>
              <TableRow className="bg-muted/50">
                <TableCell className="font-medium" colSpan={4}>
                  {formatDate(date)}
                </TableCell>
              </TableRow>
              {data[date].map((operation) => (
                <TableRow key={operation.id} className="h-14">
                  <TableCell className="w-8">
                    <Avatar>
                      <AvatarFallback>?</AvatarFallback>
                    </Avatar>
                  </TableCell>
                  <TableCell>
                    <div className="flex flex-col">
                      <Text className="font-medium">{operation.description}</Text>
                      <Text variant="muted">{operation.accountName}</Text>
                    </div>
                  </TableCell>
                  <TableCell className={cn('text-right font-semibold', cnOperationCurrencyColor(operation.amount))}>
                    {formatCurrency(operation.amount)}
                  </TableCell>
                </TableRow>
              ))}
            </React.Fragment>
          ))}
        </TableBody>
      </Table>
      {hasNextPage && (
        <div className="inline-flex justify-center">
          <Button disabled={isFetching} loading={isFetching} size="xs" variant="outline" onClick={fetchNextPage}>
            <ArrowDownToLine />
            {t('common.pagination.loadMore')}
          </Button>
        </div>
      )}
    </>
  )
}

function AccountOperationsTableSkeleton() {
  return (
    <Table>
      <TableBody>
        {Array.from({ length: ENV.VITE_LIST_PAGE_SIZE }, (_, index) => (
          <TableRow key={index} className="h-14 animate-pulse hover:bg-transparent">
            <TableCell className="w-8">
              <Skeleton className="h-10 w-10 rounded-full" />
            </TableCell>
            <TableCell>
              <div className="flex flex-col gap-2">
                <Skeleton className="h-4 w-3/4" />
                <Skeleton className="h-3 w-1/2" />
              </div>
            </TableCell>
            <TableCell>
              <Skeleton className="h-4 w-20 ml-auto" />
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

function AccountOperationsNoData() {
  const { t } = useTranslation()

  return (
    <div className="flex flex-col items-center gap-2 py-10">
      <ServerOff className="h-12 w-12 text-muted-foreground" />
      <Text variant="muted">{t('operations.noOperations')}</Text>
    </div>
  )
}

export { AccountOperationsList }
