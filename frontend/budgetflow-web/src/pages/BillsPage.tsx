import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorAlert } from '../components/ErrorAlert'
import { Modal } from '../components/Modal'
import { billService } from '../services/billService'
import type { Bill } from '../types/bills'
import type { ApiError } from '../types/api'

const billSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  amount: z.coerce.number().positive('Amount must be greater than zero'),
  dueDate: z.string().min(1, 'Due date is required'),
  isRecurring: z.boolean(),
})

type BillFormValues = z.infer<typeof billSchema>

const paymentSchema = z.object({
  amountPaid: z.coerce.number().positive('Amount must be greater than zero'),
  paidAt: z.string().min(1, 'Payment date is required'),
})

type PaymentFormValues = z.infer<typeof paymentSchema>

export function BillsPage() {
  const queryClient = useQueryClient()
  const [isBillModalOpen, setIsBillModalOpen] = useState(false)
  const [payingBill, setPayingBill] = useState<Bill | null>(null)

  const { data: bills, isLoading, isError, error, refetch } = useQuery({ queryKey: ['bills'], queryFn: billService.getAll })
  const { data: upcomingBills } = useQuery({ queryKey: ['bills', 'upcoming'], queryFn: () => billService.getUpcoming(14) })

  const billForm = useForm<BillFormValues>({
    resolver: zodResolver(billSchema),
    defaultValues: { isRecurring: false },
  })

  const paymentForm = useForm<PaymentFormValues>({
    resolver: zodResolver(paymentSchema),
  })

  const createMutation = useMutation({
    mutationFn: billService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bills'] })
      setIsBillModalOpen(false)
      billForm.reset()
    },
  })

  const archiveMutation = useMutation({
    mutationFn: billService.archive,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bills'] }),
  })

  const paymentMutation = useMutation({
    mutationFn: ({ id, request }: { id: string; request: PaymentFormValues }) =>
      billService.recordPayment(id, { amountPaid: request.amountPaid, paidAtUtc: new Date(request.paidAt).toISOString() }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['bills'] })
      setPayingBill(null)
      paymentForm.reset()
    },
  })

  function openBillModal() {
    billForm.reset({ name: '', amount: 0, dueDate: new Date().toISOString().slice(0, 10), isRecurring: false })
    setIsBillModalOpen(true)
  }

  function openPaymentModal(bill: Bill) {
    paymentForm.reset({ amountPaid: bill.amount, paidAt: new Date().toISOString().slice(0, 10) })
    setPayingBill(bill)
  }

  const onSubmitBill = billForm.handleSubmit((values) => {
    createMutation.mutate({
      name: values.name,
      amount: values.amount,
      dueDateUtc: new Date(values.dueDate).toISOString(),
      isRecurring: values.isRecurring,
    })
  })

  const onSubmitPayment = paymentForm.handleSubmit((values) => {
    if (payingBill) {
      paymentMutation.mutate({ id: payingBill.id, request: values })
    }
  })

  if (isLoading) {
    return <LoadingSpinner label="Loading bills..." />
  }

  if (isError) {
    const apiError = error as unknown as ApiError
    return <ErrorAlert message={apiError.message ?? 'Failed to load bills.'} onRetry={() => refetch()} />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h3 mb-0">Bills</h1>
        <button type="button" className="btn btn-primary" onClick={openBillModal}>
          <i className="bi bi-plus-lg me-1" aria-hidden="true"></i>
          New Bill
        </button>
      </div>

      {upcomingBills && upcomingBills.length > 0 && (
        <div className="alert alert-info">
          <i className="bi bi-bell-fill me-2" aria-hidden="true"></i>
          You have {upcomingBills.length} bill(s) due in the next 14 days.
        </div>
      )}

      {(archiveMutation.isError || paymentMutation.isError) && (
        <ErrorAlert
          message={
            ((archiveMutation.error ?? paymentMutation.error) as unknown as ApiError)?.message ?? 'An error occurred.'
          }
        />
      )}

      <div className="card">
        <div className="table-responsive">
          <table className="table table-hover align-middle mb-0">
            <thead>
              <tr>
                <th>Name</th>
                <th>Due Date</th>
                <th className="text-end">Amount</th>
                <th>Recurring</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {bills && bills.length > 0 ? (
                bills.map((bill) => (
                  <tr key={bill.id}>
                    <td>{bill.name}</td>
                    <td>{new Date(bill.dueDateUtc).toLocaleDateString()}</td>
                    <td className="text-end">{bill.amount.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}</td>
                    <td>{bill.isRecurring ? 'Yes' : 'No'}</td>
                    <td>
                      {bill.isPaid ? (
                        <span className="badge text-bg-success">Paid</span>
                      ) : bill.isOverdue ? (
                        <span className="badge text-bg-danger">Overdue</span>
                      ) : (
                        <span className="badge text-bg-warning">Pending</span>
                      )}
                    </td>
                    <td className="text-end d-flex gap-2 justify-content-end">
                      <button type="button" className="btn btn-sm btn-outline-primary" onClick={() => openPaymentModal(bill)}>
                        <i className="bi bi-cash-coin me-1" aria-hidden="true"></i>
                        Pay
                      </button>
                      <button
                        type="button"
                        className="btn btn-sm btn-outline-danger"
                        onClick={() => archiveMutation.mutate(bill.id)}
                        disabled={archiveMutation.isPending}
                      >
                        <i className="bi bi-archive" aria-hidden="true"></i>
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="text-center text-muted py-4">
                    No bills yet.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Modal
        title="New Bill"
        isOpen={isBillModalOpen}
        onClose={() => setIsBillModalOpen(false)}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setIsBillModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" form="bill-form" className="btn btn-primary" disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Saving...' : 'Save'}
            </button>
          </>
        }
      >
        {createMutation.isError && (
          <ErrorAlert message={(createMutation.error as unknown as ApiError).message ?? 'Failed to create bill.'} />
        )}
        <form id="bill-form" onSubmit={onSubmitBill} noValidate>
          <div className="mb-3">
            <label className="form-label">Name</label>
            <input className={`form-control ${billForm.formState.errors.name ? 'is-invalid' : ''}`} {...billForm.register('name')} />
            {billForm.formState.errors.name && <div className="invalid-feedback">{billForm.formState.errors.name.message}</div>}
          </div>
          <div className="row g-3">
            <div className="col-6">
              <label className="form-label">Amount</label>
              <input
                type="number"
                step="0.01"
                className={`form-control ${billForm.formState.errors.amount ? 'is-invalid' : ''}`}
                {...billForm.register('amount')}
              />
              {billForm.formState.errors.amount && <div className="invalid-feedback">{billForm.formState.errors.amount.message}</div>}
            </div>
            <div className="col-6">
              <label className="form-label">Due Date</label>
              <input
                type="date"
                className={`form-control ${billForm.formState.errors.dueDate ? 'is-invalid' : ''}`}
                {...billForm.register('dueDate')}
              />
              {billForm.formState.errors.dueDate && <div className="invalid-feedback">{billForm.formState.errors.dueDate.message}</div>}
            </div>
          </div>
          <div className="form-check mt-3">
            <input type="checkbox" className="form-check-input" id="isRecurring" {...billForm.register('isRecurring')} />
            <label className="form-check-label" htmlFor="isRecurring">
              Recurring bill
            </label>
          </div>
        </form>
      </Modal>

      <Modal
        title={`Record Payment${payingBill ? ` — ${payingBill.name}` : ''}`}
        isOpen={payingBill !== null}
        onClose={() => setPayingBill(null)}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setPayingBill(null)}>
              Cancel
            </button>
            <button type="submit" form="payment-form" className="btn btn-primary" disabled={paymentMutation.isPending}>
              {paymentMutation.isPending ? 'Recording...' : 'Record Payment'}
            </button>
          </>
        }
      >
        {paymentMutation.isError && (
          <ErrorAlert message={(paymentMutation.error as unknown as ApiError).message ?? 'Failed to record payment.'} />
        )}
        <form id="payment-form" onSubmit={onSubmitPayment} noValidate>
          <div className="mb-3">
            <label className="form-label">Amount Paid</label>
            <input
              type="number"
              step="0.01"
              className={`form-control ${paymentForm.formState.errors.amountPaid ? 'is-invalid' : ''}`}
              {...paymentForm.register('amountPaid')}
            />
            {paymentForm.formState.errors.amountPaid && (
              <div className="invalid-feedback">{paymentForm.formState.errors.amountPaid.message}</div>
            )}
          </div>
          <div className="mb-3">
            <label className="form-label">Payment Date</label>
            <input
              type="date"
              className={`form-control ${paymentForm.formState.errors.paidAt ? 'is-invalid' : ''}`}
              {...paymentForm.register('paidAt')}
            />
            {paymentForm.formState.errors.paidAt && <div className="invalid-feedback">{paymentForm.formState.errors.paidAt.message}</div>}
          </div>
        </form>
      </Modal>
    </div>
  )
}
