import { apiClient } from '../api/apiClient'
import type { BudgetSummary, CreateMonthlyBudgetRequest, UpdateMonthlyBudgetRequest } from '../types/budgets'

export const budgetService = {
  async getSummary(year?: number, month?: number): Promise<BudgetSummary> {
    const { data } = await apiClient.get<BudgetSummary>('/budgets', { params: { year, month } })
    return data
  },

  async create(request: CreateMonthlyBudgetRequest) {
    const { data } = await apiClient.post('/budgets', request)
    return data
  },

  async update(id: string, request: UpdateMonthlyBudgetRequest) {
    const { data } = await apiClient.put(`/budgets/${id}`, request)
    return data
  },
}
