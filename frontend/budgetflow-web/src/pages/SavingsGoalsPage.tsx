import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorAlert } from '../components/ErrorAlert'
import { Modal } from '../components/Modal'
import { savingsGoalService } from '../services/savingsGoalService'
import type { SavingsGoal } from '../types/savingsGoals'
import type { ApiError } from '../types/api'

const goalSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  targetAmount: z.coerce.number().positive('Target amount must be greater than zero'),
  targetDate: z.string().optional().or(z.literal('')),
})

type GoalFormValues = z.infer<typeof goalSchema>

const contributionSchema = z.object({
  amount: z.coerce.number().positive('Amount must be greater than zero'),
  contributedAt: z.string().min(1, 'Date is required'),
})

type ContributionFormValues = z.infer<typeof contributionSchema>

export function SavingsGoalsPage() {
  const queryClient = useQueryClient()
  const [isGoalModalOpen, setIsGoalModalOpen] = useState(false)
  const [contributingGoal, setContributingGoal] = useState<SavingsGoal | null>(null)

  const { data: goals, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['savings-goals'],
    queryFn: savingsGoalService.getAll,
  })

  const goalForm = useForm<GoalFormValues>({ resolver: zodResolver(goalSchema) })
  const contributionForm = useForm<ContributionFormValues>({ resolver: zodResolver(contributionSchema) })

  const createMutation = useMutation({
    mutationFn: savingsGoalService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['savings-goals'] })
      setIsGoalModalOpen(false)
      goalForm.reset()
    },
  })

  const contributionMutation = useMutation({
    mutationFn: ({ id, request }: { id: string; request: ContributionFormValues }) =>
      savingsGoalService.recordContribution(id, {
        amount: request.amount,
        contributedAtUtc: new Date(request.contributedAt).toISOString(),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['savings-goals'] })
      setContributingGoal(null)
      contributionForm.reset()
    },
  })

  function openGoalModal() {
    goalForm.reset({ name: '', targetAmount: 0, targetDate: '' })
    setIsGoalModalOpen(true)
  }

  function openContributionModal(goal: SavingsGoal) {
    contributionForm.reset({ amount: 0, contributedAt: new Date().toISOString().slice(0, 10) })
    setContributingGoal(goal)
  }

  const onSubmitGoal = goalForm.handleSubmit((values) => {
    createMutation.mutate({
      name: values.name,
      targetAmount: values.targetAmount,
      targetDateUtc: values.targetDate ? new Date(values.targetDate).toISOString() : null,
    })
  })

  const onSubmitContribution = contributionForm.handleSubmit((values) => {
    if (contributingGoal) {
      contributionMutation.mutate({ id: contributingGoal.id, request: values })
    }
  })

  if (isLoading) {
    return <LoadingSpinner label="Loading savings goals..." />
  }

  if (isError) {
    const apiError = error as unknown as ApiError
    return <ErrorAlert message={apiError.message ?? 'Failed to load savings goals.'} onRetry={() => refetch()} />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h3 mb-0">Savings Goals</h1>
        <button type="button" className="btn btn-primary" onClick={openGoalModal}>
          <i className="bi bi-plus-lg me-1" aria-hidden="true"></i>
          New Goal
        </button>
      </div>

      {contributionMutation.isError && (
        <ErrorAlert message={(contributionMutation.error as unknown as ApiError).message ?? 'Failed to record contribution.'} />
      )}

      <div className="row g-3">
        {goals && goals.length > 0 ? (
          goals.map((goal) => (
            <div className="col-12 col-md-6 col-lg-4" key={goal.id}>
              <div className="card h-100">
                <div className="card-body">
                  <h5 className="card-title">{goal.name}</h5>
                  {goal.targetDateUtc && (
                    <p className="text-muted small mb-2">Target: {new Date(goal.targetDateUtc).toLocaleDateString()}</p>
                  )}
                  <div
                    className="progress mb-2"
                    role="progressbar"
                    aria-valuenow={goal.progressPercentage}
                    aria-valuemin={0}
                    aria-valuemax={100}
                  >
                    <div className="progress-bar bg-primary" style={{ width: `${Math.min(goal.progressPercentage, 100)}%` }}>
                      {goal.progressPercentage.toFixed(0)}%
                    </div>
                  </div>
                  <p className="mb-3">
                    ${goal.currentAmount.toLocaleString()} of ${goal.targetAmount.toLocaleString()}
                  </p>
                  <button type="button" className="btn btn-sm btn-outline-primary" onClick={() => openContributionModal(goal)}>
                    <i className="bi bi-piggy-bank me-1" aria-hidden="true"></i>
                    Add Contribution
                  </button>
                </div>
              </div>
            </div>
          ))
        ) : (
          <p className="text-muted">No savings goals yet. Create one to start saving.</p>
        )}
      </div>

      <Modal
        title="New Savings Goal"
        isOpen={isGoalModalOpen}
        onClose={() => setIsGoalModalOpen(false)}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setIsGoalModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" form="goal-form" className="btn btn-primary" disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Saving...' : 'Save'}
            </button>
          </>
        }
      >
        {createMutation.isError && (
          <ErrorAlert message={(createMutation.error as unknown as ApiError).message ?? 'Failed to create savings goal.'} />
        )}
        <form id="goal-form" onSubmit={onSubmitGoal} noValidate>
          <div className="mb-3">
            <label className="form-label">Name</label>
            <input className={`form-control ${goalForm.formState.errors.name ? 'is-invalid' : ''}`} {...goalForm.register('name')} />
            {goalForm.formState.errors.name && <div className="invalid-feedback">{goalForm.formState.errors.name.message}</div>}
          </div>
          <div className="mb-3">
            <label className="form-label">Target Amount</label>
            <input
              type="number"
              step="0.01"
              className={`form-control ${goalForm.formState.errors.targetAmount ? 'is-invalid' : ''}`}
              {...goalForm.register('targetAmount')}
            />
            {goalForm.formState.errors.targetAmount && (
              <div className="invalid-feedback">{goalForm.formState.errors.targetAmount.message}</div>
            )}
          </div>
          <div className="mb-3">
            <label className="form-label">Target Date (optional)</label>
            <input type="date" className="form-control" {...goalForm.register('targetDate')} />
          </div>
        </form>
      </Modal>

      <Modal
        title={`Add Contribution${contributingGoal ? ` — ${contributingGoal.name}` : ''}`}
        isOpen={contributingGoal !== null}
        onClose={() => setContributingGoal(null)}
        footer={
          <>
            <button type="button" className="btn btn-outline-secondary" onClick={() => setContributingGoal(null)}>
              Cancel
            </button>
            <button type="submit" form="contribution-form" className="btn btn-primary" disabled={contributionMutation.isPending}>
              {contributionMutation.isPending ? 'Saving...' : 'Add'}
            </button>
          </>
        }
      >
        <form id="contribution-form" onSubmit={onSubmitContribution} noValidate>
          <div className="mb-3">
            <label className="form-label">Amount</label>
            <input
              type="number"
              step="0.01"
              className={`form-control ${contributionForm.formState.errors.amount ? 'is-invalid' : ''}`}
              {...contributionForm.register('amount')}
            />
            {contributionForm.formState.errors.amount && (
              <div className="invalid-feedback">{contributionForm.formState.errors.amount.message}</div>
            )}
          </div>
          <div className="mb-3">
            <label className="form-label">Date</label>
            <input
              type="date"
              className={`form-control ${contributionForm.formState.errors.contributedAt ? 'is-invalid' : ''}`}
              {...contributionForm.register('contributedAt')}
            />
            {contributionForm.formState.errors.contributedAt && (
              <div className="invalid-feedback">{contributionForm.formState.errors.contributedAt.message}</div>
            )}
          </div>
        </form>
      </Modal>
    </div>
  )
}
