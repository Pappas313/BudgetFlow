import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorAlert } from '../components/ErrorAlert'
import { Modal } from '../components/Modal'
import { accountService } from '../services/accountService'
import { AccountType, accountTypeLabels } from '../types/accounts'
import type { BankAccount } from '../types/accounts'
import type { ApiError } from '../types/api'

const accountSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  currency: z
    .string()
    .length(3, 'Use a 3-letter currency code')
    .transform((value) => value.toUpperCase()),
  accountType: z.coerce.number().int().min(0).max(5),
  institution: z.string().max(200).optional().or(z.literal('')),
  openingBalance: z.coerce.number().min(0, 'Opening balance cannot be negative'),
})

type AccountFormValues = z.infer<typeof accountSchema>

const accountTypeOptions = Object.entries(accountTypeLabels).map(([value, label]) => ({
  value: Number(value),
  label,
}))

export function AccountsPage() {
  const queryClient = useQueryClient()
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingAccount, setEditingAccount] = useState<BankAccount | null>(null)

  const { data: accounts, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['accounts'],
    queryFn: accountService.getAll,
  })

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AccountFormValues>({
    resolver: zodResolver(accountSchema),
    defaultValues: { accountType: AccountType.Checking, openingBalance: 0 },
  })

  const createMutation = useMutation({
    mutationFn: accountService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      closeModal()
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, values }: { id: string; values: AccountFormValues }) =>
      accountService.update(id, {
        name: values.name,
        currency: values.currency,
        accountType: values.accountType as AccountType,
        institution: values.institution || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      closeModal()
    },
  })

  const archiveMutation = useMutation({
    mutationFn: accountService.archive,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['accounts'] }),
  })

  function openCreateModal() {
    setEditingAccount(null)
    reset({ name: '', currency: 'USD', accountType: AccountType.Checking, institution: '', openingBalance: 0 })
    setIsModalOpen(true)
  }

  function openEditModal(account: BankAccount) {
    setEditingAccount(account)
    reset({
      name: account.name,
      currency: account.currency,
      accountType: account.accountType,
      institution: account.institution ?? '',
      openingBalance: account.currentBalance,
    })
    setIsModalOpen(true)
  }

  function closeModal() {
    setIsModalOpen(false)
    setEditingAccount(null)
  }

  const onSubmit = handleSubmit((values) => {
    if (editingAccount) {
      updateMutation.mutate({ id: editingAccount.id, values })
    } else {
      createMutation.mutate({
        name: values.name,
        currency: values.currency,
        accountType: values.accountType as AccountType,
        institution: values.institution || null,
        openingBalance: values.openingBalance,
      })
    }
  })

  const isSaving = createMutation.isPending || updateMutation.isPending
  const mutationError = (createMutation.error ?? updateMutation.error) as ApiError | null

  if (isLoading) {
    return <LoadingSpinner label="Loading accounts..." />
  }

  if (isError) {
    const apiError = error as unknown as ApiError
    return <ErrorAlert message={apiError.message ?? 'Failed to load accounts.'} onRetry={() => refetch()} />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h3 mb-0">Accounts</h1>
        <button type="button" className="btn btn-primary" onClick={openCreateModal}>
          <i className="bi bi-plus-lg me-1" aria-hidden="true"></i>
          New Account
        </button>
      </div>

      {archiveMutation.isError && (
        <ErrorAlert message={(archiveMutation.error as unknown as ApiError).message ?? 'Failed to archive account.'} />
      )}

      <div className="row g-3">
        {accounts && accounts.length > 0 ? (
          accounts.map((account) => (
            <div className="col-12 col-md-6 col-lg-4" key={account.id}>
              <div className="card h-100">
                <div className="card-body">
                  <div className="d-flex justify-content-between align-items-start">
                    <div>
                      <h5 className="card-title mb-1">{account.name}</h5>
                      <p className="text-muted small mb-2">
                        {accountTypeLabels[account.accountType]}
                        {account.institution ? ` · ${account.institution}` : ''}
                      </p>
                    </div>
                    <span className="badge text-bg-secondary">{account.currency}</span>
                  </div>
                  <p className="fs-4 fw-semibold mb-3">
                    {account.currentBalance.toLocaleString(undefined, { style: 'currency', currency: account.currency })}
                  </p>
                  <div className="d-flex gap-2">
                    <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => openEditModal(account)}>
                      <i className="bi bi-pencil me-1" aria-hidden="true"></i>
                      Edit
                    </button>
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-danger"
                      onClick={() => archiveMutation.mutate(account.id)}
                      disabled={archiveMutation.isPending}
                    >
                      <i className="bi bi-archive me-1" aria-hidden="true"></i>
                      Archive
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))
        ) : (
          <p className="text-muted">No accounts yet. Create one to get started.</p>
        )}
      </div>

      <Modal
        title={editingAccount ? 'Edit Account' : 'New Account'}
        isOpen={isModalOpen}
        onClose={closeModal}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={closeModal}>
              Cancel
            </button>
            <button type="submit" form="account-form" className="btn btn-primary" disabled={isSaving}>
              {isSaving ? 'Saving...' : 'Save'}
            </button>
          </>
        }
      >
        {mutationError && <ErrorAlert message={mutationError.message ?? 'Failed to save account.'} />}
        <form id="account-form" onSubmit={onSubmit} noValidate>
          <div className="mb-3">
            <label className="form-label" htmlFor="name">
              Name
            </label>
            <input id="name" className={`form-control ${errors.name ? 'is-invalid' : ''}`} {...register('name')} />
            {errors.name && <div className="invalid-feedback">{errors.name.message}</div>}
          </div>

          <div className="row g-3">
            <div className="col-6">
              <label className="form-label" htmlFor="currency">
                Currency
              </label>
              <input
                id="currency"
                className={`form-control ${errors.currency ? 'is-invalid' : ''}`}
                maxLength={3}
                {...register('currency')}
              />
              {errors.currency && <div className="invalid-feedback">{errors.currency.message}</div>}
            </div>
            <div className="col-6">
              <label className="form-label" htmlFor="accountType">
                Type
              </label>
              <select id="accountType" className="form-select" {...register('accountType')}>
                {accountTypeOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="mb-3 mt-3">
            <label className="form-label" htmlFor="institution">
              Institution
            </label>
            <input id="institution" className="form-control" {...register('institution')} />
          </div>

          {!editingAccount && (
            <div className="mb-3">
              <label className="form-label" htmlFor="openingBalance">
                Opening Balance
              </label>
              <input
                id="openingBalance"
                type="number"
                step="0.01"
                className={`form-control ${errors.openingBalance ? 'is-invalid' : ''}`}
                {...register('openingBalance')}
              />
              {errors.openingBalance && <div className="invalid-feedback">{errors.openingBalance.message}</div>}
            </div>
          )}
        </form>
      </Modal>
    </div>
  )
}
