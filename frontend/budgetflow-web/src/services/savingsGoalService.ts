import { apiClient } from '../api/apiClient'
import type {
  CreateSavingsGoalRequest,
  RecordSavingsContributionRequest,
  SavingsContribution,
  SavingsGoal,
} from '../types/savingsGoals'

export const savingsGoalService = {
  async getAll(): Promise<SavingsGoal[]> {
    const { data } = await apiClient.get<SavingsGoal[]>('/savings-goals')
    return data
  },

  async create(request: CreateSavingsGoalRequest): Promise<SavingsGoal> {
    const { data } = await apiClient.post<SavingsGoal>('/savings-goals', request)
    return data
  },

  async recordContribution(id: string, request: RecordSavingsContributionRequest): Promise<SavingsContribution> {
    const { data } = await apiClient.post<SavingsContribution>(`/savings-goals/${id}/contributions`, request)
    return data
  },
}
