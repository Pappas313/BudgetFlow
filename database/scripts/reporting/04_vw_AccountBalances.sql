/*
================================================================================
View:        dbo.vw_AccountBalances
Database:    BudgetFlowDB
Purpose:     Reports current account balances as stored on BankAccounts,
			 reconciled against the transaction ledger, for active and
			 archived accounts.

Grain:       One row per BankAccounts.Id.

Inputs:      dbo.BankAccounts, dbo.FinancialTransactions

Calculations:
  - StoredBalance       = BankAccounts.CurrentBalance (the authoritative,
						   incrementally-maintained balance).
  - LedgerNetMovement   = SUM(Amount) across all non-deleted transactions for
						   the account (Income/Expense/Transfer amounts are
						   already signed, so this is a simple SUM with no
						   CASE needed and no double counting).
  - ReconciledBalance   = StoredBalance (kept for transparency/debugging; see
						   BalanceDiscrepancy for drift detection).
  - BalanceDiscrepancy  = StoredBalance - LedgerNetMovement. Non-zero values
						   indicate the account's opening balance (captured at
						   creation, before any ledger rows existed) or a data
						   issue; this is expected to equal the account's
						   original opening balance in the common case.

Business rules:
  - Includes archived (IsDeleted = 1) accounts so historical balance exports
	remain available, flagged via IsArchived.
  - Excludes soft-deleted transactions from the ledger reconciliation.
  - Reports Currency per account (currency boundary preserved; no
	cross-currency summation is performed in this view).

Authorization:
  - Returns data across ALL users. Callers MUST filter by
	ApplicationUserId = @CurrentUserId before exposing results.

Performance:
  - A CTE aggregates FinancialTransactions by BankAccountId once, then a
	single left join attaches the ledger total to each account row.
  - Supported by the existing index on FinancialTransactions(BankAccountId).
================================================================================
*/
CREATE OR ALTER VIEW dbo.vw_AccountBalances
AS
WITH LedgerAgg AS
(
	SELECT
		ft.BankAccountId,
		SUM(ft.Amount) AS LedgerNetMovement
	FROM dbo.FinancialTransactions AS ft
	WHERE ft.IsDeleted = 0
	GROUP BY ft.BankAccountId
)
SELECT
	ba.Id                       AS BankAccountId,
	ba.ApplicationUserId,
	ba.Name                     AS AccountName,
	ba.Institution,
	ba.AccountType,
	ba.Currency,
	ba.CurrentBalance           AS StoredBalance,
	COALESCE(la.LedgerNetMovement, 0) AS LedgerNetMovement,
	ba.CurrentBalance - COALESCE(la.LedgerNetMovement, 0) AS BalanceDiscrepancy,
	CASE WHEN ba.IsDeleted = 1 THEN 1 ELSE 0 END AS IsArchived,
	ba.CreatedAtUtc,
	ba.LastModifiedAtUtc
FROM dbo.BankAccounts AS ba
LEFT JOIN LedgerAgg AS la
	ON la.BankAccountId = ba.Id;
GO
