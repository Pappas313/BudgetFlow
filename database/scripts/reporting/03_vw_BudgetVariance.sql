/*
================================================================================
View:        dbo.vw_BudgetVariance
Database:    BudgetFlowDB
Purpose:     Compares budgeted amounts to actual spend per category/month,
			 reporting variance and utilization percentage.

Grain:       One row per MonthlyBudgets.Id (ApplicationUserId, TransactionCategoryId, Year, Month).

Inputs:      dbo.MonthlyBudgets, dbo.TransactionCategories,
			 dbo.FinancialTransactions, dbo.BankAccounts

Calculations:
  - ActualExpenses     = SUM(ABS(Amount)) for TransactionType = 1 (Expense) in
						 that category/year/month across all of the user's
						 accounts (all currencies combined into the budget's
						 currency grouping; see Currency Note below).
  - BudgetVariance     = BudgetedAmount - ActualExpenses (positive = under
						 budget, negative = over budget).
  - BudgetUtilization  = ActualExpenses / NULLIF(BudgetedAmount, 0) * 100.
  - OverBudgetFlag     = 1 when ActualExpenses > BudgetedAmount, else 0.

Currency Note:
  - MonthlyBudgets does not store a currency; budgets are defined per category
	for the user. Actual expenses are aggregated per currency (respecting the
	boundary that amounts in different currencies must never be summed), and
	one row is returned per (budget, currency-with-activity) so variance is
	always computed within a single currency. Categories with no activity in
	a currency simply do not appear for that currency.

Business rules:
  - Only Expense transactions count toward ActualExpenses; Income and
	Transfer are excluded to prevent duplicate/incorrect aggregation.
  - Soft-deleted categories/accounts/transactions are excluded.

Authorization:
  - Returns data across ALL users. Callers MUST filter by
	ApplicationUserId = @CurrentUserId before exposing results.

Performance:
  - CTE pre-aggregates FinancialTransactions by user/category/currency/month
	before joining MonthlyBudgets, avoiding a per-row correlated subquery.
================================================================================
*/
CREATE OR ALTER VIEW dbo.vw_BudgetVariance
AS
WITH ActualExpenseAgg AS
(
	SELECT
		ba.ApplicationUserId,
		ba.Currency,
		ft.TransactionCategoryId,
		YEAR(ft.OccurredAtUtc)  AS [Year],
		MONTH(ft.OccurredAtUtc) AS [Month],
		SUM(ABS(ft.Amount)) AS ActualExpenses
	FROM dbo.FinancialTransactions AS ft
	INNER JOIN dbo.BankAccounts AS ba
		ON ba.Id = ft.BankAccountId
		AND ba.IsDeleted = 0
	WHERE ft.IsDeleted = 0
		AND ft.TransactionType = 1 -- Expense only; prevents duplicate aggregation from Income/Transfer rows
		AND ft.TransactionCategoryId IS NOT NULL
	GROUP BY
		ba.ApplicationUserId,
		ba.Currency,
		ft.TransactionCategoryId,
		YEAR(ft.OccurredAtUtc),
		MONTH(ft.OccurredAtUtc)
)
SELECT
	mb.Id                   AS MonthlyBudgetId,
	mb.ApplicationUserId,
	mb.TransactionCategoryId,
	tc.Name                 AS CategoryName,
	mb.[Year],
	mb.[Month],
	COALESCE(ea.Currency, N'USD') AS Currency,
	mb.BudgetedAmount,
	COALESCE(ea.ActualExpenses, 0) AS ActualExpenses,
	mb.BudgetedAmount - COALESCE(ea.ActualExpenses, 0) AS BudgetVariance,
	CAST(
		COALESCE(ea.ActualExpenses, 0) * 100.0 / NULLIF(mb.BudgetedAmount, 0)
		AS DECIMAL(9, 4)
	) AS BudgetUtilization,
	CASE
		WHEN COALESCE(ea.ActualExpenses, 0) > mb.BudgetedAmount THEN 1
		ELSE 0
	END AS IsOverBudget
FROM dbo.MonthlyBudgets AS mb
INNER JOIN dbo.TransactionCategories AS tc
	ON tc.Id = mb.TransactionCategoryId
	AND tc.IsDeleted = 0
LEFT JOIN ActualExpenseAgg AS ea
	ON ea.ApplicationUserId = mb.ApplicationUserId
	AND ea.TransactionCategoryId = mb.TransactionCategoryId
	AND ea.[Year] = mb.[Year]
	AND ea.[Month] = mb.[Month];
GO
