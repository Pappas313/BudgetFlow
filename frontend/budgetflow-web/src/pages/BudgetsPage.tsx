import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorAlert } from '../components/ErrorAlert'
import { Modal } from '../components/Modal'
import { budgetService } from '../services/budgetService'
import { categoryService } from '../services/categoryService'
import type { ApiError } from '../types/api'

const budgetSchema = z.object({
  transactionCategoryId: z.string().min(1, 'Category is required'),
  budgetedAmount: z.coerce.number().positive('Budgeted amount must be greater than zero'),
})

type BudgetFormValues = z.infer<typeof budgetSchema>

const now = new Date()

function utilizationColor(utilization: number) {
  if (utilization >= 100) return 'bg-danger'
  if (utilization >= 80) return 'bg-warning'
  return 'bg-success'
}

export function BudgetsPage() {
  const queryClient = useQueryClient()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const [isModalOpen, setIsModalOpen] = useState(false)

  const { data: categories } = useQuery({ queryKey: ['categories'], queryFn: categoryService.getAll })

  const { data: summary, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['budgets', year, month],
    queryFn: () => budgetService.getSummary(year, month),
  })

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<BudgetFormValues>({ resolver: zodResolver(budgetSchema) })

  const createMutation = useMutation({
    mutationFn: (values: BudgetFormValues) =>
      budgetService.create({ transactionCategoryId: values.transactionCategoryId, year, month, budgetedAmount: values.budgetedAmount }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['budgets'] })
      setIsModalOpen(false)
      reset()
    },
  })

  function openModal() {
    reset({ transactionCategoryId: '', budgetedAmount: 0 })
    setIsModalOpen(true)
  }

  const onSubmit = handleSubmit((values) => createMutation.mutate(values))

  if (isLoading) {
    return <LoadingSpinner label="Loading budgets..." />
  }

  if (isError) {
    const apiError = error as unknown as ApiError
    return <ErrorAlert message={apiError.message ?? 'Failed to load budgets.'} onRetry={() => refetch()} />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <h1 className="h3 mb-0">Budgets</h1>
        <div className="d-flex gap-2 align-items-center">
          <select className="form-select form-select-sm" value={month} onChange={(e) => setMonth(Number(e.target.value))}>
            {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (
              <option key={m} value={m}>
                {new Date(2000, m - 1, 1).toLocaleString(undefined, { month: 'long' })}
              </option>
            ))}
          </select>
          <input
            type="number"
            className="form-control form-control-sm"
            style={{ width: '100px' }}
            value={year}
            onChange={(e) => setYear(Number(e.target.value))}
          />
          <button type="button" className="btn btn-primary btn-sm" onClick={openModal}>
            <i className="bi bi-plus-lg me-1" aria-hidden="true"></i>
            New Budget
          </button>
        </div>
      </div>

      {summary && (
        <div className="row g-3 mb-4">
          <div className="col-12 col-sm-4">
            <div className="card h-100">
              <div className="card-body">
                <h6 className="card-subtitle text-muted mb-2">Total Allocated</h6>
                <p className="card-text fs-4 fw-semibold">${summary.totalAllocatedBudget.toLocaleString()}</p>
              </div>
            </div>
          </div>
          <div className="col-12 col-sm-4">
            <div className="card h-100">
              <div className="card-body">
                <h6 className="card-subtitle text-muted mb-2">Total Spent</h6>
                <p className="card-text fs-4 fw-semibold">${summary.totalActualExpenses.toLocaleString()}</p>
              </div>
            </div>
          </div>
          <div className="col-12 col-sm-4">
            <div className="card h-100">
              <div className="card-body">
                <h6 className="card-subtitle text-muted mb-2">Overall Utilization</h6>
                <p className="card-text fs-4 fw-semibold">{summary.overallBudgetUtilization.toFixed(1)}%</p>
              </div>
            </div>
          </div>
        </div>
      )}

      {createMutation.isError && (
        <ErrorAlert message={(createMutation.error as unknown as ApiError).message ?? 'Failed to create budget.'} />
      )}

      <div className="row g-3">
        {summary && summary.budgets.length > 0 ? (
          summary.budgets.map((budget) => (
            <div className="col-12 col-md-6" key={budget.id}>
              <div className="card h-100">
                <div className="card-body">
                  <div className="d-flex justify-content-between">
                    <h5 className="card-title">{budget.categoryName}</h5>
                    <span className="text-muted">
                      ${budget.actualExpenses.toLocaleString()} / ${budget.allocatedBudget.toLocaleString()}
                    </span>
                  </div>
                  <div className="progress" role="progressbar" aria-valuenow={budget.budgetUtilization} aria-valuemin={0} aria-valuemax={100}>
                    <div
                      className={`progress-bar ${utilizationColor(budget.budgetUtilization)}`}
                      style={{ width: `${Math.min(budget.budgetUtilization, 100)}%` }}
                    >
                      {budget.budgetUtilization.toFixed(0)}%
                    </div>
                  </div>
                  <p className="text-muted small mt-2 mb-0">
                    {budget.budgetVariance >= 0
                      ? `$${budget.budgetVariance.toLocaleString()} remaining`
                      : `$${Math.abs(budget.budgetVariance).toLocaleString()} over budget`}
                  </p>
                </div>
              </div>
            </div>
          ))
        ) : (
          <p className="text-muted">No budgets set for this month.</p>
        )}
      </div>

      <Modal
        title="New Monthly Budget"
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setIsModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" form="budget-form" className="btn btn-primary" disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Saving...' : 'Save'}
            </button>
          </>
        }
      >
        <form id="budget-form" onSubmit={onSubmit} noValidate>
          <div className="mb-3">
            <label className="form-label">Category</label>
            <select className={`form-select ${errors.transactionCategoryId ? 'is-invalid' : ''}`} {...register('transactionCategoryId')}>
              <option value="">Select a category</option>
              {categories?.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
            {errors.transactionCategoryId && <div className="invalid-feedback">{errors.transactionCategoryId.message}</div>}
          </div>
          <div className="mb-3">
            <label className="form-label">Budgeted Amount</label>
            <input
              type="number"
              step="0.01"
              className={`form-control ${errors.budgetedAmount ? 'is-invalid' : ''}`}
              {...register('budgetedAmount')}
            />
            {errors.budgetedAmount && <div className="invalid-feedback">{errors.budgetedAmount.message}</div>}
          </div>
        </form>
      </Modal>
    </div>
  )
}
