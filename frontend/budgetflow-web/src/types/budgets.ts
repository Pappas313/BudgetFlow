export interface MonthlyBudget {
  id: string
  transactionCategoryId: string
  categoryName: string
  year: number
  month: number
  allocatedBudget: number
  actualExpenses: number
  remainingBudget: number
  budgetUtilization: number
  budgetVariance: number
  createdAtUtc: string
  lastModifiedAtUtc: string | null
}

export interface BudgetSummary {
  year: number
  month: number
  totalAllocatedBudget: number
  totalActualExpenses: number
  totalRemainingBudget: number
  overallBudgetUtilization: number
  budgets: MonthlyBudget[]
}

export interface CreateMonthlyBudgetRequest {
  transactionCategoryId: string
  year: number
  month: number
  budgetedAmount: number
}

export interface UpdateMonthlyBudgetRequest {
  budgetedAmount: number
}
