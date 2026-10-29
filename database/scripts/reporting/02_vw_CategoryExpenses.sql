/*
================================================================================
View:        dbo.vw_CategoryExpenses
Database:    BudgetFlowDB
Purpose:     Reports category-level spending totals per application user,
			 month, and currency, including each category's share of that
			 month's total spend.

Grain:       One row per (ApplicationUserId, Currency, Year, Month, TransactionCategoryId).

Inputs:      dbo.FinancialTransactions, dbo.TransactionCategories, dbo.BankAccounts

Calculations:
  - CategoryExpense    = SUM(ABS(Amount)) for TransactionType = 1 (Expense)
  - MonthTotalExpense  = SUM(CategoryExpense) OVER (PARTITION BY user, currency, year, month)
  - PercentOfMonthSpend = CategoryExpense / NULLIF(MonthTotalExpense, 0) * 100

Business rules:
  - Only Expense transactions are included; Income and Transfer rows are
	excluded to avoid counting non-spending cash movement as category spend.
  - Transactions without a category (TransactionCategoryId IS NULL) are rolled
	up under an "Uncategorized" pseudo-category so totals still reconcile with
	vw_MonthlyIncomeExpenses.MonthlyExpenses.
  - Soft-deleted transactions/categories/accounts are excluded.
  - Grouped by Currency to respect currency boundaries.

Authorization:
  - Returns data across ALL users. Callers MUST filter by
	ApplicationUserId = @CurrentUserId before exposing results.

Performance:
  - CTE aggregates at the (user, account/currency, category, month) grain
	before joining TransactionCategories, so the dimension join is performed
	once per grouped row.
  - A window function computes the monthly total once per partition instead
	of a correlated subquery/self-join per category row.
================================================================================
*/
CREATE OR ALTER VIEW dbo.vw_CategoryExpenses
AS
WITH ExpenseAgg AS
(
	SELECT
		ba.ApplicationUserId,
		ba.Currency,
		YEAR(ft.OccurredAtUtc)  AS [Year],
		MONTH(ft.OccurredAtUtc) AS [Month],
		ft.TransactionCategoryId,
		SUM(ABS(ft.Amount)) AS CategoryExpense
	FROM dbo.FinancialTransactions AS ft
	INNER JOIN dbo.BankAccounts AS ba
		ON ba.Id = ft.BankAccountId
		AND ba.IsDeleted = 0
	WHERE ft.IsDeleted = 0
		AND ft.TransactionType = 1 -- Expense only; Income/Transfer excluded to prevent duplicate aggregation
	GROUP BY
		ba.ApplicationUserId,
		ba.Currency,
		YEAR(ft.OccurredAtUtc),
		MONTH(ft.OccurredAtUtc),
		ft.TransactionCategoryId
)
SELECT
	ea.ApplicationUserId,
	ea.Currency,
	ea.[Year],
	ea.[Month],
	ea.TransactionCategoryId,
	COALESCE(tc.Name, N'Uncategorized') AS CategoryName,
	ea.CategoryExpense,
	SUM(ea.CategoryExpense)
		OVER (PARTITION BY ea.ApplicationUserId, ea.Currency, ea.[Year], ea.[Month]) AS MonthTotalExpense,
	CAST(
		ea.CategoryExpense * 100.0
		/ NULLIF(SUM(ea.CategoryExpense) OVER (PARTITION BY ea.ApplicationUserId, ea.Currency, ea.[Year], ea.[Month]), 0)
		AS DECIMAL(9, 4)
	) AS PercentOfMonthSpend,
	RANK() OVER (PARTITION BY ea.ApplicationUserId, ea.Currency, ea.[Year], ea.[Month]
				 ORDER BY ea.CategoryExpense DESC) AS SpendRankInMonth
FROM ExpenseAgg AS ea
LEFT JOIN dbo.TransactionCategories AS tc
	ON tc.Id = ea.TransactionCategoryId
	AND tc.IsDeleted = 0;
GO
