/*
================================================================================
View:        dbo.vw_MonthlyIncomeExpenses
Database:    BudgetFlowDB
Purpose:     Reports monthly income, monthly expenses, and net cash flow per
			 application user, bank account, and currency.

Grain:       One row per (ApplicationUserId, BankAccountId, Currency, Year, Month).

Inputs:      dbo.FinancialTransactions, dbo.BankAccounts

Calculations:
  - MonthlyIncome   = SUM(Amount) for TransactionType = 0 (Income)
  - MonthlyExpenses = SUM(ABS(Amount)) for TransactionType = 1 (Expense)
  - NetCashFlow     = MonthlyIncome - MonthlyExpenses

Business rules:
  - Transfers (TransactionType = 2) are excluded: a transfer between a user's
	own accounts is not income or an expense, and including both transfer legs
	would double-count cash movement that never left the user's overall
	position.
  - Soft-deleted transactions/accounts (IsDeleted = 1) are excluded, mirroring
	the EF Core global query filters.
  - Results are grouped by Currency so multi-currency accounts are never
	summed together (currency boundary).

Authorization:
  - This view returns data across ALL users. Callers MUST filter by
	ApplicationUserId (e.g. WHERE ApplicationUserId = @CurrentUserId) before
	returning results to any client. Grant SELECT only to roles that enforce
	this scoping in the application/reporting layer.

Performance:
  - A CTE pre-aggregates FinancialTransactions by user/account/currency/month
	before joining BankAccounts, so the dimension join happens once per
	grouped row rather than once per transaction row.
  - Supported by indexes on FinancialTransactions(BankAccountId) and
	FinancialTransactions(OccurredAtUtc).
================================================================================
*/
CREATE OR ALTER VIEW dbo.vw_MonthlyIncomeExpenses
AS
WITH TransactionAgg AS
(
	SELECT
		ba.ApplicationUserId,
		ft.BankAccountId,
		ba.Currency,
		YEAR(ft.OccurredAtUtc)  AS [Year],
		MONTH(ft.OccurredAtUtc) AS [Month],
		SUM(CASE WHEN ft.TransactionType = 0 THEN ft.Amount ELSE 0 END)       AS MonthlyIncome,
		SUM(CASE WHEN ft.TransactionType = 1 THEN ABS(ft.Amount) ELSE 0 END) AS MonthlyExpenses
	FROM dbo.FinancialTransactions AS ft
	INNER JOIN dbo.BankAccounts AS ba
		ON ba.Id = ft.BankAccountId
		AND ba.IsDeleted = 0
	WHERE ft.IsDeleted = 0
		AND ft.TransactionType IN (0, 1) -- Income, Expense only; Transfer excluded to prevent duplicate aggregation
	GROUP BY
		ba.ApplicationUserId,
		ft.BankAccountId,
		ba.Currency,
		YEAR(ft.OccurredAtUtc),
		MONTH(ft.OccurredAtUtc)
)
SELECT
	ta.ApplicationUserId,
	ta.BankAccountId,
	ba.Name                                    AS AccountName,
	ta.Currency,
	ta.[Year],
	ta.[Month],
	ta.MonthlyIncome,
	ta.MonthlyExpenses,
	ta.MonthlyIncome - ta.MonthlyExpenses       AS NetCashFlow,
	SUM(ta.MonthlyIncome - ta.MonthlyExpenses)
		OVER (PARTITION BY ta.ApplicationUserId, ta.Currency
			  ORDER BY ta.[Year], ta.[Month]
			  ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS RunningNetCashFlow
FROM TransactionAgg AS ta
INNER JOIN dbo.BankAccounts AS ba
	ON ba.Id = ta.BankAccountId;
GO
