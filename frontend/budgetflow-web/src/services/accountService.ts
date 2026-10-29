import { apiClient } from '../api/apiClient'
import type { BankAccount, AccountBalance, CreateBankAccountRequest, UpdateBankAccountRequest } from '../types/accounts'

export const accountService = {
  async getAll(): Promise<BankAccount[]> {
    const { data } = await apiClient.get<BankAccount[]>('/accounts')
    return data
  },

  async create(request: CreateBankAccountRequest): Promise<BankAccount> {
    const { data } = await apiClient.post<BankAccount>('/accounts', request)
    return data
  },

  async update(id: string, request: UpdateBankAccountRequest): Promise<BankAccount> {
    const { data } = await apiClient.put<BankAccount>(`/accounts/${id}`, request)
    return data
  },

  async archive(id: string): Promise<void> {
    await apiClient.delete(`/accounts/${id}`)
  },

  async getBalance(id: string): Promise<AccountBalance> {
    const { data } = await apiClient.get<AccountBalance>(`/accounts/${id}/balance`)
    return data
  },
}
