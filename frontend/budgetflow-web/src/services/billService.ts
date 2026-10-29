import { apiClient } from '../api/apiClient'
import type { Bill, BillPayment, CreateBillRequest, RecordBillPaymentRequest, UpdateBillRequest } from '../types/bills'

export const billService = {
  async getAll(): Promise<Bill[]> {
    const { data } = await apiClient.get<Bill[]>('/bills')
    return data
  },

  async getUpcoming(withinDays = 7): Promise<Bill[]> {
    const { data } = await apiClient.get<Bill[]>('/bills/upcoming', { params: { withinDays } })
    return data
  },

  async create(request: CreateBillRequest): Promise<Bill> {
    const { data } = await apiClient.post<Bill>('/bills', request)
    return data
  },

  async update(id: string, request: UpdateBillRequest): Promise<Bill> {
    const { data } = await apiClient.put<Bill>(`/bills/${id}`, request)
    return data
  },

  async archive(id: string): Promise<void> {
    await apiClient.delete(`/bills/${id}`)
  },

  async recordPayment(id: string, request: RecordBillPaymentRequest): Promise<BillPayment> {
    const { data } = await apiClient.post<BillPayment>(`/bills/${id}/payments`, request)
    return data
  },
}
