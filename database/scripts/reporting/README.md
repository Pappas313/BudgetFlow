# BudgetFlow Reporting Views

This folder contains SQL Server reporting views for the **BudgetFlowDB** database.
Each script is idempotent (`CREATE OR ALTER VIEW`) and can be run independently or
as part of a deployment pipeline, in any order, against a database that already
contains the BudgetFlow application schema (EF Core migrations).

## Views

| Script | View | Purpose |
|---|---|---|
| `01_vw_MonthlyIncomeExpenses.sql` | `dbo.vw_MonthlyIncomeExpenses` | Monthly income, expenses, and net cash flow per user/account/currency. |
| `02_vw_CategoryExpenses.sql` | `dbo.vw_CategoryExpenses` | Category-level spending totals per user/month/currency, with share-of-total via window functions. |
| `03_vw_BudgetVariance.sql` | `dbo.vw_BudgetVariance` | Budgeted vs. actual spend per category/month, with variance and utilization %. |
| `04_vw_AccountBalances.sql` | `dbo.vw_AccountBalances` | Current account balances, reconciled against the transaction ledger, including archived-account status. |
| `05_vw_UpcomingBills.sql` | `dbo.vw_UpcomingBills` | Bills due in the future (or overdue), with amount remaining after payments already recorded. |
| `06_vw_SavingsProgress.sql` | `dbo.vw_SavingsProgress` | Savings goal progress, contribution totals, and percent-to-target. |

## Design Notes

- **User scoping / authorization**: every view exposes `ApplicationUserId` as a
  column and is intended to always be queried with a `WHERE ApplicationUserId = @CurrentUserId`
  predicate (or joined to a caller-supplied authorized-user table/variable). Views
  do not embed `SUSER_SNAME()`/`CURRENT_USER` filtering because the API's
  authenticated user id is a domain concept (ASP.NET Core Identity GUID), not a
  SQL Server login. Application code and ad-hoc reporting **must** pass the
  caller's `ApplicationUserId` explicitly. Grant `SELECT` on these views only to
  the reporting/application roles that already enforce row-level authorization
  in the API layer.
- **Soft deletes**: all queries respect `IsDeleted = 0` on `BankAccounts`,
  `FinancialTransactions`, `TransactionCategories`, `Bills`, and `SavingsGoals`,
  mirroring the EF Core global query filters (`HasQueryFilter`).
- **Currency boundaries**: aggregations are always grouped by `Currency` (from
  `BankAccounts`) in addition to user/category/month, so multi-currency
  portfolios are never summed together. Consumers must display/compare amounts
  per currency group.
- **No duplicate aggregation**: transfers (`TransactionType = 2`) are excluded
  from income/expense/category aggregates because a transfer between two of the
  user's own accounts is not income or spending — it would otherwise be
  double-counted (once as a credit, once as a debit) across the two legs. Bill
  payments are aggregated once via `BillPayments`, not re-derived from
  `FinancialTransactions`, to avoid counting the same cash movement twice.
- **CTEs / window functions**: each view uses CTEs to pre-aggregate at the
  lowest necessary grain before joining dimension tables, and window functions
  (`SUM(...) OVER (PARTITION BY ...)`) are used instead of repeated self-joins
  to compute totals/shares, keeping the final join count minimal.
- **Efficient joins**: aggregation happens before joining to
  `BankAccounts`/`TransactionCategories`/`Bills`/`SavingsGoals` so dimension
  tables are joined once per grouped row, not once per fact row.

## Sample Data Verification

`99_SampleData_Verification.sql` seeds a disposable sample user/account/category/
bill/budget/savings-goal data set inside a transaction that is rolled back at the
end, and prints the computed view results next to manually calculated expected
values so the view logic can be verified without mutating real data.
