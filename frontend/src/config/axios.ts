import axios, { AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'

import { ENV } from '@/env'
import { useAuthStore } from '@/features/authentication/stores/authStore'
import type { Problem } from '@/types/Problem'

export const apiClient: AxiosInstance = axios.create({
  baseURL: ENV.VITE_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const authToken = useAuthStore.getState().authToken

  if (authToken?.token) {
    config.headers.Authorization = `Bearer ${authToken.token}`
  }

  return config
})

apiClient.interceptors.response.use(
  (response) => {
    return response
  },
  async (error: AxiosError<Problem>) => {
    const apiError = error.response?.data
    const fallbackError: Problem = {
      type: 'errors.NetworkError',
      title: 'errors.NetworkError',
      status: 0,
    }
    return Promise.reject(apiError ?? fallbackError)
  }
)
