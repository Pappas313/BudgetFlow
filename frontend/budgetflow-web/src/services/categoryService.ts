import { apiClient } from '../api/apiClient'
import type { TransactionCategory, CreateTransactionCategoryRequest } from '../types/categories'

export const categoryService = {
  async getAll(): Promise<TransactionCategory[]> {
    const { data } = await apiClient.get<TransactionCategory[]>('/categories')
    return data
  },

  async create(request: CreateTransactionCategoryRequest): Promise<TransactionCategory> {
    const { data } = await apiClient.post<TransactionCategory>('/categories', request)
    return data
  },
}
