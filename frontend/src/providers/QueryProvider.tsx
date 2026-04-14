import { MutationCache, QueryCache, QueryClient, QueryClientProvider } from '@tanstack/react-query'
import React from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'

import type { Problem } from '@/types/Problem'

function QueryProvider({ children }: { children: React.ReactNode }) {
  const { t } = useTranslation()

  /**
   * Global error handler for react-query
   */
  const onError = React.useCallback(
    (error: Problem) => {
      console.error('React Query Error:', error)
      if (error.status === 0) {
        toast.error(t('errors.NetworkError'))
      } else if (error.status === 400) {
        const validationMessages = Object.values(error.errors ?? {})
          .flat()
          .map(({ code, description }) =>
            t(`errors.${code}`, {
              defaultValue: description || code,
            })
          )
        const uniqueMessages = [...new Set(validationMessages)]
        if (uniqueMessages.length === 0) {
          toast.error(error.detail ?? t('errors.BadRequest'))
        } else {
          uniqueMessages.forEach((message) => toast.error(message))
        }
      } else if (error.status === 401) {
        toast.error(t('errors.User.InvalidCredentials'))
      } else if (error.status === 403) {
        toast.error(t('errors.Forbidden'))
      } else if (error.status === 404) {
        toast.error(error.detail ?? t('errors.NotFound'))
      } else if (error.status === 500) {
        toast.error(t('errors.InternalServerError'))
      } else {
        toast.error(t('errors.Unknown'))
      }
    },
    [t]
  )

  const queryClient = React.useMemo(() => {
    return new QueryClient({
      defaultOptions: {
        queries: {
          // Stale time: data is considered fresh for 5 minutes
          staleTime: 1000 * 60 * 5,
          // Cache time: unused data stays in cache for 10 minutes
          gcTime: 1000 * 60 * 10,
          retry: false,
          refetchOnWindowFocus: false,
          refetchOnReconnect: true,
        },
        mutations: {
          retry: false,
          throwOnError: false,
        },
      },
      queryCache: new QueryCache({ onError }),
      mutationCache: new MutationCache({ onError }),
    })
  }, [onError])

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

export default QueryProvider
