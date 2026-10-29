import { useQuery } from '@tanstack/react-query'
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorAlert } from '../components/ErrorAlert'
import { dashboardService } from '../services/dashboardService'
import type { ApiError } from '../types/api'

const chartData = [
  { name: 'Income', value: 0 },
  { name: 'Expenses', value: 0 },
  { name: 'Savings', value: 0 },
]

export function DashboardPage() {
  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['dashboard-summary'],
    queryFn: dashboardService.getSummary,
    retry: 1,
  })

  if (isLoading) {
    return <LoadingSpinner label="Loading dashboard..." />
  }

  if (isError) {
    const apiError = error as unknown as ApiError
    return <ErrorAlert message={apiError.message ?? 'Failed to load dashboard data.'} onRetry={() => refetch()} />
  }

  const summaryData = data
    ? [
        { name: 'Income', value: data.totalIncome },
        { name: 'Expenses', value: data.totalExpenses },
        { name: 'Savings', value: data.totalSavings },
      ]
    : chartData

  return (
    <div>
      <h1 className="h3 mb-4">Dashboard</h1>

      <div className="row g-3 mb-4">
        <div className="col-12 col-sm-4">
          <div className="card h-100">
            <div className="card-body">
              <h6 className="card-subtitle text-muted mb-2">Total Income</h6>
              <p className="card-text fs-4 fw-semibold">${data?.totalIncome.toLocaleString() ?? 0}</p>
            </div>
          </div>
        </div>
        <div className="col-12 col-sm-4">
          <div className="card h-100">
            <div className="card-body">
              <h6 className="card-subtitle text-muted mb-2">Total Expenses</h6>
              <p className="card-text fs-4 fw-semibold">${data?.totalExpenses.toLocaleString() ?? 0}</p>
            </div>
          </div>
        </div>
        <div className="col-12 col-sm-4">
          <div className="card h-100">
            <div className="card-body">
              <h6 className="card-subtitle text-muted mb-2">Total Savings</h6>
              <p className="card-text fs-4 fw-semibold">${data?.totalSavings.toLocaleString() ?? 0}</p>
            </div>
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-body">
          <h6 className="card-subtitle text-muted mb-3">Overview</h6>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={summaryData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="value" fill="#0d6efd" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  )
}
