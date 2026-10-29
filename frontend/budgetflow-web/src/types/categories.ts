export interface TransactionCategory {
  id: string
  name: string
  description: string | null
  createdAtUtc: string
}

export interface CreateTransactionCategoryRequest {
  name: string
  description?: string | null
}
