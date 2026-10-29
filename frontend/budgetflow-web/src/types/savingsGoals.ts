export interface SavingsGoal {
  id: string
  name: string
  targetAmount: number
  currentAmount: number
  progressPercentage: number
  targetDateUtc: string | null
  createdAtUtc: string
  lastModifiedAtUtc: string | null
}

export interface SavingsContribution {
  id: string
  savingsGoalId: string
  amount: number
  contributedAtUtc: string
}

export interface CreateSavingsGoalRequest {
  name: string
  targetAmount: number
  targetDateUtc?: string | null
}

export interface RecordSavingsContributionRequest {
  amount: number
  contributedAtUtc: string
}
