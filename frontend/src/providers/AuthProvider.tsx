import React from 'react'
import { useNavigate } from 'react-router-dom'

import { useGetCurrentUser, useRefreshToken } from '@/api/endpoints/AuthenticationEndpoints'
import { AppLoader } from '@/components/ui/AppLoader'
import { useAuthStore } from '@/features/authentication/stores/authStore'

const REFRESH_TOKEN_BEFORE_EXPIRY_MS = 5 * 60 * 1000 // 5 minutes

function AuthProvider({ children }: { children: React.ReactNode }) {
  const navigate = useNavigate()

  const { authToken, refreshToken } = useAuthStore((state) => state)
  const { isAuthTokenValid, isRefreshTokenValid, clearAuth, setUser } = useAuthStore((state) => state.actions)

  const isAuthValid = authToken && isAuthTokenValid()
  const isRefreshValid = refreshToken && isRefreshTokenValid()
  const hasNoTokens = !authToken && !refreshToken

  const currentUserQuery = useGetCurrentUser({ enabled: isAuthValid === true })
  const refreshTokenMutation = useRefreshToken()

  const refreshTimerRef = React.useRef<ReturnType<typeof setTimeout> | null>(null)

  const clearRefreshTimer = React.useCallback(() => {
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current)
      refreshTimerRef.current = null
    }
  }, [])

  const scheduleTokenRefresh = React.useCallback(() => {
    const authValid = authToken && isAuthTokenValid()
    const refreshValid = refreshToken && isRefreshTokenValid()

    if (!authValid || !authToken?.expireAt || !refreshValid) {
      return
    }

    const expirationTime = new Date(authToken.expireAt).getTime()
    const now = Date.now()
    const timeUntilRefresh = expirationTime - now - REFRESH_TOKEN_BEFORE_EXPIRY_MS

    if (timeUntilRefresh > 0) {
      refreshTimerRef.current = setTimeout(() => {
        refreshTokenMutation.mutate(refreshToken.token)
      }, timeUntilRefresh)
    } else {
      refreshTokenMutation.mutate(refreshToken.token)
    }
  }, [authToken, refreshToken, isAuthTokenValid, isRefreshTokenValid, refreshTokenMutation])

  React.useEffect(() => {
    if (hasNoTokens) {
      navigate('/auth', { replace: true })
      return
    }

    if (isAuthValid) {
      return
    }

    if (isRefreshValid) {
      refreshTokenMutation.mutate(refreshToken.token)
      return
    }

    clearAuth()
    navigate('/auth', { replace: true })
  }, [hasNoTokens, isAuthValid, isRefreshValid, navigate, refreshTokenMutation, refreshToken, clearAuth])

  React.useEffect(() => {
    clearRefreshTimer()
    scheduleTokenRefresh()

    return clearRefreshTimer
  }, [clearRefreshTimer, scheduleTokenRefresh])

  React.useEffect(() => {
    if (isAuthValid && currentUserQuery.data) {
      setUser(currentUserQuery.data)
    }
  }, [isAuthValid, currentUserQuery.data, setUser])

  const isLoading = refreshTokenMutation.isPending || (isAuthValid && currentUserQuery.isLoading)

  if (isLoading) {
    return <AppLoader />
  }

  if (currentUserQuery.isError) {
    clearAuth()
    navigate('/auth', { replace: true })
    return null
  }

  if (isAuthValid && currentUserQuery.data) {
    return <>{children}</>
  }

  return null
}

export default AuthProvider
