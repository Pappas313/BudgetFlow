import { useQuery } from '@tanstack/react-query'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { LoadingSpinner } from '../components/LoadingSpinner'
import { ErrorAlert } from '../components/ErrorAlert'
import { apiClient } from '../api/apiClient'
import type { ApiError } from '../types/api'

interface MonthlyTrend {
  year: number
  month: number
  totalIncome: number
  totalExpense: number
  netCashFlow: number
}

interface CategoryExpense {
  categoryName: string
  totalAmount: number
}

const colors = ['#0d6efd', '#6610f2', '#d63384', '#fd7e14', '#198754', '#20c997', '#0dcaf0']

export function ReportsPage() {
  const trendsQuery = useQuery({
    queryKey: ['reports', 'monthly-trends'],
    queryFn: async () => {
      const { data } = await apiClient.get<MonthlyTrend[]>('/dashboard/monthly-trends', { params: { monthsBack: 6 } })
      return data
    },
  })

  const categoryQuery = useQuery({
    queryKey: ['reports', 'category-expenses'],
    queryFn: async () => {
      const { data } = await apiClient.get<CategoryExpense[]>('/dashboard/category-expenses')
      return data
    },
  })

  if (trendsQuery.isLoading || categoryQuery.isLoading) {
    return <LoadingSpinner label="Loading reports..." />
  }

  if (trendsQuery.isError || categoryQuery.isError) {
    const apiError = (trendsQuery.error ?? categoryQuery.error) as unknown as ApiError
    return (
      <ErrorAlert
        message={apiError.message ?? 'Failed to load reports.'}
        onRetry={() => {
          trendsQuery.refetch()
          categoryQuery.refetch()
        }}
      />
    )
  }

  const trendData = (trendsQuery.data ?? []).map((trend) => ({
    name: `${new Date(trend.year, trend.month - 1, 1).toLocaleString(undefined, { month: 'short' })} ${trend.year}`,
    Income: trend.totalIncome,
    Expenses: trend.totalExpense,
  }))

  const categoryData = categoryQuery.data ?? []

  return (
    <div>
      <h1 className="h3 mb-4">Reports</h1>

      <div className="row g-3">
        <div className="col-12 col-lg-7">
          <div className="card h-100">
            <div className="card-body">
              <h6 className="card-subtitle text-muted mb-3">Income vs. Expenses (last 6 months)</h6>
              <ResponsiveContainer width="100%" height={320}>
                <BarChart data={trendData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="Income" fill="#198754" radius={[4, 4, 0, 0]} />
                  <Bar dataKey="Expenses" fill="#dc3545" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>

        <div className="col-12 col-lg-5">
          <div className="card h-100">
            <div className="card-body">
              <h6 className="card-subtitle text-muted mb-3">Spending by Category</h6>
              {categoryData.length > 0 ? (
                <ResponsiveContainer width="100%" height={320}>
                  <PieChart>
                    <Pie data={categoryData} dataKey="totalAmount" nameKey="categoryName" outerRadius={110} label>
                      {categoryData.map((entry, index) => (
                        <Cell key={entry.categoryName} fill={colors[index % colors.length]} />
                      ))}
                    </Pie>
                    <Tooltip />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              ) : (
                <p className="text-muted">No category spending recorded yet.</p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
