import { useInfiniteQuery } from '@tanstack/react-query'

import { apiClient } from '@/config/axios'
import { ENV } from '@/env'
import type { GetAccountOperationsQueryDto } from '@/types/accountOperations/queries/GetAccountOperationsQueryDto'
import type { GetAccountOperationsResponseDto } from '@/types/accountOperations/responses/GetAccountOperationsResponseDto'
import type { InfiniteDataGrouped } from '@/types/InfiniteDataGrouped'
import type { PaginatedList } from '@/types/PaginatedList'

import { queryKeys } from '../QueryKeys'

const basePath = '/operations'

export function useGetInfiniteAccountOperations(query: GetAccountOperationsQueryDto = {}) {
  return useInfiniteQuery({
    queryKey: queryKeys.accountOperations.list(query),
    initialPageParam: 1,
    queryFn: async ({ pageParam = 1, signal }) => {
      const response = await apiClient.get<PaginatedList<GetAccountOperationsResponseDto>>(basePath, {
        params: { ...query, pageNumber: pageParam, pageSize: ENV.VITE_LIST_PAGE_SIZE },
        signal,
      })
      return response.data
    },
    getNextPageParam: (lastPage) => {
      if (lastPage.pageNumber < lastPage.totalPages) {
        return lastPage.pageNumber + 1
      }
      return undefined
    },
    select: (data): InfiniteDataGrouped<GetAccountOperationsResponseDto> => {
      const allItems = data.pages.flatMap((page) => page.items)
      return allItems.reduce<InfiniteDataGrouped<GetAccountOperationsResponseDto>>((groups, operation) => {
        const operationDate = new Date(operation.createdAt)
        const year = operationDate.getFullYear()
        const month = operationDate.getMonth()
        const day = operationDate.getDate()
        const dateKey = new Date(year, month, day).toISOString()

        ;(groups[dateKey] ??= []).push(operation)
        return groups
      }, {})
    },
  })
}
