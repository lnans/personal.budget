import { useInfiniteQuery, useMutation, useQuery } from '@tanstack/react-query'

import { apiClient } from '@/config/axios'
import { ENV } from '@/env'
import type { GetAccountOperationsQueryDto } from '@/types/accountOperations/queries/GetAccountOperationsQueryDto'
import type { GetAccountOperationsResponseDto } from '@/types/accountOperations/responses/GetAccountOperationsResponseDto'
import type { CreateAccountFormDto } from '@/types/accounts/forms/CreateAccountFormDto'
import type { PatchAccountFormDto } from '@/types/accounts/forms/PatchAccountFormDto'
import type { CreateAccountResponseDto } from '@/types/accounts/responses/CreateAccountResponseDto'
import type { DeleteAccountResponseDto } from '@/types/accounts/responses/DeleteAccountResponseDto'
import type { GetAccountsResponseDto } from '@/types/accounts/responses/GetAccountsResponseDto'
import type { PatchAccountResponseDto } from '@/types/accounts/responses/PatchAccountResponseDto'
import type { InfiniteDataGrouped } from '@/types/InfiniteDataGrouped'
import type { PaginatedList } from '@/types/PaginatedList'

import { queryKeys } from '../QueryKeys'

const basePath = '/accounts'

export function useGetAccounts() {
  return useQuery({
    queryKey: queryKeys.accounts.all,
    queryFn: async ({ signal }) => {
      const response = await apiClient.get<GetAccountsResponseDto[]>(basePath, { signal })
      return response.data
    },
  })
}

export function useGetAccountById(id: string | null) {
  const accountsQuery = useGetAccounts()
  return accountsQuery.data?.find((account) => account.id === id)
}

export function useCreateAccount() {
  return useMutation({
    mutationFn: async (data: CreateAccountFormDto) => {
      const response = await apiClient.post<CreateAccountResponseDto>(basePath, data)
      return response.data
    },
    onSuccess: (_, __, ___, context) => {
      context.client.invalidateQueries({ queryKey: queryKeys.accounts.all })
    },
  })
}

export function usePatchAccount() {
  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: PatchAccountFormDto }) => {
      const response = await apiClient.patch<PatchAccountResponseDto>(`${basePath}/${id}`, data)
      return response.data
    },
    onSuccess: (_, __, ___, context) => {
      context.client.invalidateQueries({ queryKey: queryKeys.accounts.all })
    },
  })
}

export function useDeleteAccount() {
  return useMutation({
    mutationFn: async (id: string) => {
      const response = await apiClient.delete<DeleteAccountResponseDto>(`${basePath}/${id}`)
      return response.data
    },
    onSuccess: (_, __, ___, context) => {
      context.client.invalidateQueries({ queryKey: queryKeys.accounts.all })
    },
  })
}

export function useGetInfiniteAccountOperations(query: GetAccountOperationsQueryDto = {}) {
  return useInfiniteQuery({
    queryKey: queryKeys.accountOperations.list(query),
    initialPageParam: 1,
    queryFn: async ({ pageParam = 1, signal }) => {
      const response = await apiClient.get<PaginatedList<GetAccountOperationsResponseDto>>(`${basePath}/operations`, {
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
