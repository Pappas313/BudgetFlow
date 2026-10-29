export interface Bill {
  id: string
  name: string
  amount: number
  dueDateUtc: string
  isRecurring: boolean
  isPaid: boolean
  isOverdue: boolean
  createdAtUtc: string
  lastModifiedAtUtc: string | null
}

export interface BillPayment {
  id: string
  billId: string
  amountPaid: number
  paidAtUtc: string
  createdAtUtc: string
}

export interface CreateBillRequest {
  name: string
  amount: number
  dueDateUtc: string
  isRecurring: boolean
}

export interface UpdateBillRequest {
  name: string
  amount: number
  dueDateUtc: string
  isRecurring: boolean
}

export interface RecordBillPaymentRequest {
  amountPaid: number
  paidAtUtc: string
}
