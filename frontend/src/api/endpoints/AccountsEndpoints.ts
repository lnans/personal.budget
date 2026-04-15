import { useMutation, useQuery } from '@tanstack/react-query'

import { apiClient } from '@/config/axios'
import type { CreateAccountFormDto } from '@/types/accounts/forms/CreateAccountFormDto'
import type { PutAccountFormDto } from '@/types/accounts/forms/PutAccountFormDto'
import type { CreateAccountResponseDto } from '@/types/accounts/responses/CreateAccountResponseDto'
import type { DeleteAccountResponseDto } from '@/types/accounts/responses/DeleteAccountResponseDto'
import type { GetAccountsResponseDto } from '@/types/accounts/responses/GetAccountsResponseDto'
import type { PutAccountResponseDto } from '@/types/accounts/responses/PutAccountResponseDto'

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

export function usePutAccount() {
  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: PutAccountFormDto }) => {
      const response = await apiClient.put<PutAccountResponseDto>(`${basePath}/${id}`, data)
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
