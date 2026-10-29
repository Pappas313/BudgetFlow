import { apiClient } from '../api/apiClient'
import type {
  CreateTransactionRequest,
  PagedResult,
  Transaction,
  TransactionFilters,
  TransferRequest,
  TransferResult,
} from '../types/transactions'

export const transactionService = {
  async getAll(filters: TransactionFilters): Promise<PagedResult<Transaction>> {
    const { data } = await apiClient.get<PagedResult<Transaction>>('/transactions', { params: filters })
    return data
  },

  async create(request: CreateTransactionRequest): Promise<Transaction> {
    const { data } = await apiClient.post<Transaction>('/transactions', request)
    return data
  },

  async delete(id: string): Promise<void> {
    await apiClient.delete(`/transactions/${id}`)
  },

  async transfer(request: TransferRequest): Promise<TransferResult> {
    const { data } = await apiClient.post<TransferResult>('/transactions/transfer', request)
    return data
  },
}
