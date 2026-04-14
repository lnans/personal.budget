import { useMutation, useQuery } from '@tanstack/react-query'
import { useCallback } from 'react'
import { useNavigate } from 'react-router-dom'

import { apiClient } from '@/config/axios'
import { useAuthStore } from '@/features/authentication/stores/authStore'
import type { SignInFormDto } from '@/types/authentication/forms/SignInFormDto'
import type { GetCurrentUserResponseDto } from '@/types/authentication/responses/GetCurrentUserResponseDto'
import type { RefreshTokenResponseDto } from '@/types/authentication/responses/RefreshTokenResponseDto'
import type { SignInResponseDto } from '@/types/authentication/responses/SignInResponseDto'
import type { QueryParams } from '@/types/QueryParams'

import { queryKeys } from '../QueryKeys'

const basePath = '/auth'

export function useGetCurrentUser(params: QueryParams = { enabled: true }) {
  return useQuery({
    queryKey: queryKeys.auth.all,
    queryFn: async ({ signal }) => {
      const response = await apiClient.get<GetCurrentUserResponseDto>(basePath, { signal })
      return response.data
    },
    enabled: params.enabled,
  })
}

export function useSignIn() {
  const { setAuthTokens, clearAuth } = useAuthStore((state) => state.actions)

  return useMutation({
    mutationFn: async (data: SignInFormDto) => {
      const response = await apiClient.post<SignInResponseDto>(`${basePath}/signin`, data)
      return response.data
    },
    onSuccess: (data) => {
      setAuthTokens(data.bearer, data.refreshToken)
    },
    onError: () => {
      clearAuth()
    },
  })
}

export function useRefreshToken() {
  const navigate = useNavigate()
  const { setAuthTokens, clearAuth } = useAuthStore((state) => state.actions)

  const redirectToAuth = useCallback(() => {
    clearAuth()
    navigate('/auth', { replace: true })
  }, [clearAuth, navigate])

  return useMutation({
    mutationFn: async (refreshToken: string) => {
      const response = await apiClient.post<RefreshTokenResponseDto>(`${basePath}/refresh`, { refreshToken })
      return response.data
    },
    onSuccess: (data) => {
      setAuthTokens(data.bearer, data.refreshToken)
    },
    onError: redirectToAuth,
  })
}
