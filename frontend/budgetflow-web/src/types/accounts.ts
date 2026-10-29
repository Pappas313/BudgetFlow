export const AccountType = {
  Checking: 0,
  Savings: 1,
  Credit: 2,
  Investment: 3,
  Cash: 4,
  Other: 5,
} as const

export type AccountType = (typeof AccountType)[keyof typeof AccountType]

export const accountTypeLabels: Record<AccountType, string> = {
  0: 'Checking',
  1: 'Savings',
  2: 'Credit',
  3: 'Investment',
  4: 'Cash',
  5: 'Other',
}

export interface BankAccount {
  id: string
  name: string
  institution: string | null
  accountType: AccountType
  currency: string
  currentBalance: number
  createdAtUtc: string
  lastModifiedAtUtc: string | null
}

export interface AccountBalance {
  accountId: string
  currency: string
  balance: number
  asOfUtc: string
}

export interface CreateBankAccountRequest {
  name: string
  currency: string
  accountType: AccountType
  institution?: string | null
  openingBalance: number
}

export interface UpdateBankAccountRequest {
  name: string
  currency: string
  accountType: AccountType
  institution?: string | null
}
