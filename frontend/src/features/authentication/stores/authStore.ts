import { create } from 'zustand'
import { persist } from 'zustand/middleware'

import { getTokenExpiration, isTokenExpired } from '@/lib/utils'
import type { GetCurrentUserResponseDto } from '@/types/authentication/responses/GetCurrentUserResponseDto'

type Token = {
  token: string
  expireAt: string
}

type AuthStore = {
  authToken: Token | null
  refreshToken: Token | null
  user: GetCurrentUserResponseDto | null
  actions: {
    setAuthTokens: (authToken: string, refreshToken: string) => void
    isAuthTokenValid: () => boolean
    isRefreshTokenValid: () => boolean
    clearAuth: () => void
    setUser: (user: GetCurrentUserResponseDto) => void
  }
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      authToken: null,
      refreshToken: null,
      user: null,
      actions: {
        setAuthTokens: (authToken: string, refreshToken: string) => {
          const authTokenExpiration = getTokenExpiration(authToken)
          const refreshTokenExpiration = getTokenExpiration(refreshToken)
          set({
            authToken: {
              token: authToken,
              expireAt: authTokenExpiration?.toISOString() ?? '',
            },
            refreshToken: {
              token: refreshToken,
              expireAt: refreshTokenExpiration?.toISOString() ?? '',
            },
          })
        },
        isAuthTokenValid: () => !isTokenExpired(get().authToken?.token ?? null),
        isRefreshTokenValid: () => !isTokenExpired(get().refreshToken?.token ?? null),
        clearAuth: () => {
          set({
            authToken: null,
            refreshToken: null,
            user: null,
          })
        },
        setUser: (user: GetCurrentUserResponseDto) => {
          set({ user })
        },
      },
    }),
    {
      name: 'budget-auth-storage',
      partialize: (state) => ({
        authToken: state.authToken,
        refreshToken: state.refreshToken,
      }),
    }
  )
)
