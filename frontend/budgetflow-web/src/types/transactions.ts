export const TransactionType = {
  Income: 0,
  Expense: 1,
  Transfer: 2,
} as const

export type TransactionType = (typeof TransactionType)[keyof typeof TransactionType]

export const transactionTypeLabels: Record<TransactionType, string> = {
  0: 'Income',
  1: 'Expense',
  2: 'Transfer',
}

export interface Transaction {
  id: string
  bankAccountId: string
  transactionCategoryId: string | null
  categoryName: string | null
  transactionType: TransactionType
  amount: number
  description: string | null
  occurredAtUtc: string
  transferGroupId: string | null
  externalReference: string | null
  createdAtUtc: string
  lastModifiedAtUtc: string | null
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface TransactionFilters {
  bankAccountId?: string
  transactionCategoryId?: string
  transactionType?: TransactionType
  fromUtc?: string
  toUtc?: string
  page?: number
  pageSize?: number
}

export interface CreateTransactionRequest {
  bankAccountId: string
  transactionCategoryId: string
  transactionType: TransactionType
  amount: number
  description?: string | null
  occurredAtUtc: string
  externalReference?: string | null
}

export interface TransferRequest {
  sourceBankAccountId: string
  destinationBankAccountId: string
  amount: number
  description?: string | null
  occurredAtUtc: string
  externalReference?: string | null
}

export interface TransferResult {
  transferGroupId: string
  debitTransaction: Transaction
  creditTransaction: Transaction
}

export interface TransactionSummary {
  totalIncome: number
  totalExpense: number
  netAmount: number
  fromUtc: string | null
  toUtc: string | null
}
