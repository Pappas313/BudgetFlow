import axios from 'axios'
import type { AxiosError } from 'axios'
import type { ApiError } from '../types/api'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:5001/api'

export const apiClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('budgetflow_token')
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('budgetflow_token')
    }

    const apiError: ApiError = {
      status: error.response?.status ?? 0,
      message:
        (error.response?.data as { message?: string } | undefined)?.message ??
        error.message ??
        'An unexpected error occurred',
    }

    return Promise.reject(apiError)
  },
)
