import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorAlert } from '../components/ErrorAlert'
import { Modal } from '../components/Modal'
import { accountService } from '../services/accountService'
import { categoryService } from '../services/categoryService'
import { transactionService } from '../services/transactionService'
import { TransactionType, transactionTypeLabels } from '../types/transactions'
import type { TransactionFilters } from '../types/transactions'
import type { ApiError } from '../types/api'

const transactionSchema = z.object({
  bankAccountId: z.string().min(1, 'Account is required'),
  transactionCategoryId: z.string().min(1, 'Category is required'),
  transactionType: z.coerce.number().int().min(0).max(1),
  amount: z.coerce.number().positive('Amount must be greater than zero'),
  description: z.string().max(500).optional().or(z.literal('')),
  occurredAt: z.string().min(1, 'Date is required'),
})

type TransactionFormValues = z.infer<typeof transactionSchema>

const transferSchema = z
  .object({
    sourceBankAccountId: z.string().min(1, 'Source account is required'),
    destinationBankAccountId: z.string().min(1, 'Destination account is required'),
    amount: z.coerce.number().positive('Amount must be greater than zero'),
    description: z.string().max(500).optional().or(z.literal('')),
    occurredAt: z.string().min(1, 'Date is required'),
  })
  .refine((values) => values.sourceBankAccountId !== values.destinationBankAccountId, {
    message: 'Source and destination accounts must be different',
    path: ['destinationBankAccountId'],
  })

type TransferFormValues = z.infer<typeof transferSchema>

export function TransactionsPage() {
  const queryClient = useQueryClient()
  const [isTransactionModalOpen, setIsTransactionModalOpen] = useState(false)
  const [isTransferModalOpen, setIsTransferModalOpen] = useState(false)
  const [filters, setFilters] = useState<TransactionFilters>({ page: 1, pageSize: 20 })

  const { data: accounts } = useQuery({ queryKey: ['accounts'], queryFn: accountService.getAll })
  const { data: categories } = useQuery({ queryKey: ['categories'], queryFn: categoryService.getAll })

  const {
    data: pagedTransactions,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ['transactions', filters],
    queryFn: () => transactionService.getAll(filters),
  })

  const transactionForm = useForm<TransactionFormValues>({
    resolver: zodResolver(transactionSchema),
    defaultValues: { transactionType: TransactionType.Expense },
  })

  const transferForm = useForm<TransferFormValues>({
    resolver: zodResolver(transferSchema),
  })

  const createMutation = useMutation({
    mutationFn: transactionService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] })
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      setIsTransactionModalOpen(false)
      transactionForm.reset()
    },
  })

  const transferMutation = useMutation({
    mutationFn: transactionService.transfer,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] })
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      setIsTransferModalOpen(false)
      transferForm.reset()
    },
  })

  const deleteMutation = useMutation({
    mutationFn: transactionService.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] })
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
    },
  })

  const accountNameById = useMemo(() => {
    const map = new Map<string, string>()
    accounts?.forEach((account) => map.set(account.id, account.name))
    return map
  }, [accounts])

  function openTransactionModal() {
    transactionForm.reset({ transactionType: TransactionType.Expense, occurredAt: new Date().toISOString().slice(0, 10) })
    setIsTransactionModalOpen(true)
  }

  function openTransferModal() {
    transferForm.reset({ occurredAt: new Date().toISOString().slice(0, 10) })
    setIsTransferModalOpen(true)
  }

  const onSubmitTransaction = transactionForm.handleSubmit((values) => {
    createMutation.mutate({
      bankAccountId: values.bankAccountId,
      transactionCategoryId: values.transactionCategoryId,
      transactionType: values.transactionType as TransactionType,
      amount: values.amount,
      description: values.description || null,
      occurredAtUtc: new Date(values.occurredAt).toISOString(),
    })
  })

  const onSubmitTransfer = transferForm.handleSubmit((values) => {
    transferMutation.mutate({
      sourceBankAccountId: values.sourceBankAccountId,
      destinationBankAccountId: values.destinationBankAccountId,
      amount: values.amount,
      description: values.description || null,
      occurredAtUtc: new Date(values.occurredAt).toISOString(),
    })
  })

  if (isLoading) {
    return <LoadingSpinner label="Loading transactions..." />
  }

  if (isError) {
    const apiError = error as unknown as ApiError
    return <ErrorAlert message={apiError.message ?? 'Failed to load transactions.'} onRetry={() => refetch()} />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <h1 className="h3 mb-0">Transactions</h1>
        <div className="d-flex gap-2">
          <button type="button" className="btn btn-outline-primary" onClick={openTransferModal}>
            <i className="bi bi-arrow-left-right me-1" aria-hidden="true"></i>
            Transfer
          </button>
          <button type="button" className="btn btn-primary" onClick={openTransactionModal}>
            <i className="bi bi-plus-lg me-1" aria-hidden="true"></i>
            New Transaction
          </button>
        </div>
      </div>

      <div className="card mb-3">
        <div className="card-body">
          <div className="row g-2 align-items-end">
            <div className="col-12 col-md-3">
              <label className="form-label small">Account</label>
              <select
                className="form-select form-select-sm"
                value={filters.bankAccountId ?? ''}
                onChange={(e) => setFilters((prev) => ({ ...prev, bankAccountId: e.target.value || undefined, page: 1 }))}
              >
                <option value="">All accounts</option>
                {accounts?.map((account) => (
                  <option key={account.id} value={account.id}>
                    {account.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-12 col-md-3">
              <label className="form-label small">Type</label>
              <select
                className="form-select form-select-sm"
                value={filters.transactionType ?? ''}
                onChange={(e) =>
                  setFilters((prev) => ({
                    ...prev,
                    transactionType: e.target.value === '' ? undefined : (Number(e.target.value) as TransactionType),
                    page: 1,
                  }))
                }
              >
                <option value="">All types</option>
                {Object.entries(transactionTypeLabels).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-6 col-md-3">
              <label className="form-label small">From</label>
              <input
                type="date"
                className="form-control form-control-sm"
                onChange={(e) =>
                  setFilters((prev) => ({
                    ...prev,
                    fromUtc: e.target.value ? new Date(e.target.value).toISOString() : undefined,
                    page: 1,
                  }))
                }
              />
            </div>
            <div className="col-6 col-md-3">
              <label className="form-label small">To</label>
              <input
                type="date"
                className="form-control form-control-sm"
                onChange={(e) =>
                  setFilters((prev) => ({
                    ...prev,
                    toUtc: e.target.value ? new Date(e.target.value).toISOString() : undefined,
                    page: 1,
                  }))
                }
              />
            </div>
          </div>
        </div>
      </div>

      {deleteMutation.isError && (
        <ErrorAlert message={(deleteMutation.error as unknown as ApiError).message ?? 'Failed to delete transaction.'} />
      )}

      <div className="card">
        <div className="table-responsive">
          <table className="table table-hover align-middle mb-0">
            <thead>
              <tr>
                <th>Date</th>
                <th>Account</th>
                <th>Category</th>
                <th>Type</th>
                <th className="text-end">Amount</th>
                <th>Description</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {pagedTransactions?.items.length ? (
                pagedTransactions.items.map((transaction) => (
                  <tr key={transaction.id}>
                    <td>{new Date(transaction.occurredAtUtc).toLocaleDateString()}</td>
                    <td>{accountNameById.get(transaction.bankAccountId) ?? '—'}</td>
                    <td>{transaction.categoryName ?? '—'}</td>
                    <td>
                      <span
                        className={`badge ${
                          transaction.transactionType === TransactionType.Income
                            ? 'text-bg-success'
                            : transaction.transactionType === TransactionType.Expense
                              ? 'text-bg-danger'
                              : 'text-bg-secondary'
                        }`}
                      >
                        {transactionTypeLabels[transaction.transactionType]}
                      </span>
                    </td>
                    <td className="text-end">{transaction.amount.toLocaleString(undefined, { style: 'currency', currency: 'USD' })}</td>
                    <td>{transaction.description ?? '—'}</td>
                    <td className="text-end">
                      <button
                        type="button"
                        className="btn btn-sm btn-outline-danger"
                        onClick={() => deleteMutation.mutate(transaction.id)}
                        disabled={deleteMutation.isPending}
                      >
                        <i className="bi bi-trash" aria-hidden="true"></i>
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={7} className="text-center text-muted py-4">
                    No transactions found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {pagedTransactions && pagedTransactions.totalPages > 1 && (
        <nav className="mt-3">
          <ul className="pagination justify-content-center">
            {Array.from({ length: pagedTransactions.totalPages }, (_, i) => i + 1).map((pageNumber) => (
              <li key={pageNumber} className={`page-item ${pageNumber === filters.page ? 'active' : ''}`}>
                <button type="button" className="page-link" onClick={() => setFilters((prev) => ({ ...prev, page: pageNumber }))}>
                  {pageNumber}
                </button>
              </li>
            ))}
          </ul>
        </nav>
      )}

      <Modal
        title="New Transaction"
        isOpen={isTransactionModalOpen}
        onClose={() => setIsTransactionModalOpen(false)}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setIsTransactionModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" form="transaction-form" className="btn btn-primary" disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Saving...' : 'Save'}
            </button>
          </>
        }
      >
        {createMutation.isError && (
          <ErrorAlert message={(createMutation.error as unknown as ApiError).message ?? 'Failed to create transaction.'} />
        )}
        <form id="transaction-form" onSubmit={onSubmitTransaction} noValidate>
          <div className="mb-3">
            <label className="form-label">Type</label>
            <select className="form-select" {...transactionForm.register('transactionType')}>
              <option value={TransactionType.Income}>Income</option>
              <option value={TransactionType.Expense}>Expense</option>
            </select>
          </div>
          <div className="mb-3">
            <label className="form-label">Account</label>
            <select
              className={`form-select ${transactionForm.formState.errors.bankAccountId ? 'is-invalid' : ''}`}
              {...transactionForm.register('bankAccountId')}
            >
              <option value="">Select an account</option>
              {accounts?.map((account) => (
                <option key={account.id} value={account.id}>
                  {account.name}
                </option>
              ))}
            </select>
            {transactionForm.formState.errors.bankAccountId && (
              <div className="invalid-feedback">{transactionForm.formState.errors.bankAccountId.message}</div>
            )}
          </div>
          <div className="mb-3">
            <label className="form-label">Category</label>
            <select
              className={`form-select ${transactionForm.formState.errors.transactionCategoryId ? 'is-invalid' : ''}`}
              {...transactionForm.register('transactionCategoryId')}
            >
              <option value="">Select a category</option>
              {categories?.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
            {transactionForm.formState.errors.transactionCategoryId && (
              <div className="invalid-feedback">{transactionForm.formState.errors.transactionCategoryId.message}</div>
            )}
          </div>
          <div className="row g-3">
            <div className="col-6">
              <label className="form-label">Amount</label>
              <input
                type="number"
                step="0.01"
                className={`form-control ${transactionForm.formState.errors.amount ? 'is-invalid' : ''}`}
                {...transactionForm.register('amount')}
              />
              {transactionForm.formState.errors.amount && (
                <div className="invalid-feedback">{transactionForm.formState.errors.amount.message}</div>
              )}
            </div>
            <div className="col-6">
              <label className="form-label">Date</label>
              <input
                type="date"
                className={`form-control ${transactionForm.formState.errors.occurredAt ? 'is-invalid' : ''}`}
                {...transactionForm.register('occurredAt')}
              />
              {transactionForm.formState.errors.occurredAt && (
                <div className="invalid-feedback">{transactionForm.formState.errors.occurredAt.message}</div>
              )}
            </div>
          </div>
          <div className="mb-3 mt-3">
            <label className="form-label">Description</label>
            <input className="form-control" {...transactionForm.register('description')} />
          </div>
        </form>
      </Modal>

      <Modal
        title="Transfer Money"
        isOpen={isTransferModalOpen}
        onClose={() => setIsTransferModalOpen(false)}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setIsTransferModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" form="transfer-form" className="btn btn-primary" disabled={transferMutation.isPending}>
              {transferMutation.isPending ? 'Transferring...' : 'Transfer'}
            </button>
          </>
        }
      >
        {transferMutation.isError && (
          <ErrorAlert message={(transferMutation.error as unknown as ApiError).message ?? 'Failed to transfer funds.'} />
        )}
        <form id="transfer-form" onSubmit={onSubmitTransfer} noValidate>
          <div className="mb-3">
            <label className="form-label">From Account</label>
            <select
              className={`form-select ${transferForm.formState.errors.sourceBankAccountId ? 'is-invalid' : ''}`}
              {...transferForm.register('sourceBankAccountId')}
            >
              <option value="">Select an account</option>
              {accounts?.map((account) => (
                <option key={account.id} value={account.id}>
                  {account.name}
                </option>
              ))}
            </select>
            {transferForm.formState.errors.sourceBankAccountId && (
              <div className="invalid-feedback">{transferForm.formState.errors.sourceBankAccountId.message}</div>
            )}
          </div>
          <div className="mb-3">
            <label className="form-label">To Account</label>
            <select
              className={`form-select ${transferForm.formState.errors.destinationBankAccountId ? 'is-invalid' : ''}`}
              {...transferForm.register('destinationBankAccountId')}
            >
              <option value="">Select an account</option>
              {accounts?.map((account) => (
                <option key={account.id} value={account.id}>
                  {account.name}
                </option>
              ))}
            </select>
            {transferForm.formState.errors.destinationBankAccountId && (
              <div className="invalid-feedback">{transferForm.formState.errors.destinationBankAccountId.message}</div>
            )}
          </div>
          <div className="row g-3">
            <div className="col-6">
              <label className="form-label">Amount</label>
              <input
                type="number"
                step="0.01"
                className={`form-control ${transferForm.formState.errors.amount ? 'is-invalid' : ''}`}
                {...transferForm.register('amount')}
              />
              {transferForm.formState.errors.amount && (
                <div className="invalid-feedback">{transferForm.formState.errors.amount.message}</div>
              )}
            </div>
            <div className="col-6">
              <label className="form-label">Date</label>
              <input
                type="date"
                className={`form-control ${transferForm.formState.errors.occurredAt ? 'is-invalid' : ''}`}
                {...transferForm.register('occurredAt')}
              />
              {transferForm.formState.errors.occurredAt && (
                <div className="invalid-feedback">{transferForm.formState.errors.occurredAt.message}</div>
              )}
            </div>
          </div>
          <div className="mb-3 mt-3">
            <label className="form-label">Description</label>
            <input className="form-control" {...transferForm.register('description')} />
          </div>
        </form>
      </Modal>
    </div>
  )
}
