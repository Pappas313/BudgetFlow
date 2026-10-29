import { apiClient } from '../api/apiClient'

export interface BudgetSummary {
  totalIncome: number
  totalExpenses: number
  totalSavings: number
  month: string
}

export const dashboardService = {
  async getSummary(): Promise<BudgetSummary> {
    const { data } = await apiClient.get<BudgetSummary>('/dashboard/summary')
    return data
  },
}
