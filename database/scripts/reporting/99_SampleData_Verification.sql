/*
================================================================================
Script:      99_SampleData_Verification.sql
Database:    BudgetFlowDB
Purpose:     Seeds a disposable sample data set inside a transaction, queries
			 each reporting view, and prints the results next to manually
			 calculated expected values so the view logic can be verified.
			 The transaction is ROLLED BACK at the end, so no sample data is
			 persisted.

Usage:
  1. Run 01_*.sql through 06_*.sql first to create/refresh the views.
  2. Run this script against a copy of BudgetFlowDB (or the real database,
	 since all inserts are rolled back).
  3. Inspect the PRINT output and SELECT result sets; compare the "actual"
	 view output against the "expected" values documented in each section.
================================================================================
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION SampleDataVerification;

DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
DECLARE @CheckingAccountId UNIQUEIDENTIFIER = NEWID();
DECLARE @SavingsAccountId UNIQUEIDENTIFIER = NEWID();
DECLARE @GroceriesCategoryId UNIQUEIDENTIFIER = NEWID();
DECLARE @SalaryCategoryId UNIQUEIDENTIFIER = NEWID();
DECLARE @BillId UNIQUEIDENTIFIER = NEWID();
DECLARE @SavingsGoalId UNIQUEIDENTIFIER = NEWID();
DECLARE @Now DATETIMEOFFSET = SYSUTCDATETIME();
DECLARE @ThisYear INT = YEAR(@Now);
DECLARE @ThisMonth INT = MONTH(@Now);

-- NOTE: Identity (AspNetUsers) row is required for FK integrity in BudgetFlowDB.
-- Adjust column list if the Identity schema differs from the default template.
INSERT INTO dbo.AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
	PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled,
	LockoutEnabled, AccessFailedCount, DisplayName)
VALUES (@UserId, N'sample.verification@budgetflow.test', N'SAMPLE.VERIFICATION@BUDGETFLOW.TEST',
	N'sample.verification@budgetflow.test', N'SAMPLE.VERIFICATION@BUDGETFLOW.TEST', 1,
	N'x', NEWID(), NEWID(), 0, 0, 1, 0, N'Sample Verification User');

INSERT INTO dbo.BankAccounts (Id, ApplicationUserId, Name, Institution, AccountType, Currency, CurrentBalance, CreatedAtUtc, IsDeleted)
VALUES
	(@CheckingAccountId, @UserId, N'Sample Checking', N'Sample Bank', 0, N'USD', 2450.00, @Now, 0),
	(@SavingsAccountId,  @UserId, N'Sample Savings',  N'Sample Bank', 1, N'USD', 5000.00, @Now, 0);

INSERT INTO dbo.TransactionCategories (Id, ApplicationUserId, Name, Description, CreatedAtUtc, IsDeleted)
VALUES
	(@GroceriesCategoryId, @UserId, N'Groceries', N'Food and household', @Now, 0),
	(@SalaryCategoryId,    @UserId, N'Salary',    N'Employment income', @Now, 0);

-- Income: 3000.00 salary this month
INSERT INTO dbo.FinancialTransactions
	(Id, BankAccountId, TransactionCategoryId, TransactionType, Amount, Description, OccurredAtUtc, CreatedAtUtc, IsDeleted)
VALUES
	(NEWID(), @CheckingAccountId, @SalaryCategoryId, 0, 3000.00, N'Monthly salary',
		DATETIMEOFROMPARTS(@ThisYear, @ThisMonth, 1, 9, 0, 0, 0, 0), @Now, 0);

-- Expenses: 120.50 + 80.25 groceries this month = 200.75 total
INSERT INTO dbo.FinancialTransactions
	(Id, BankAccountId, TransactionCategoryId, TransactionType, Amount, Description, OccurredAtUtc, CreatedAtUtc, IsDeleted)
VALUES
	(NEWID(), @CheckingAccountId, @GroceriesCategoryId, 1, -120.50, N'Grocery run 1',
		DATETIMEOFROMPARTS(@ThisYear, @ThisMonth, 3, 10, 0, 0, 0, 0), @Now, 0),
	(NEWID(), @CheckingAccountId, @GroceriesCategoryId, 1, -80.25, N'Grocery run 2',
		DATETIMEOFROMPARTS(@ThisYear, @ThisMonth, 10, 10, 0, 0, 0, 0), @Now, 0);

-- Transfer between own accounts: must NOT appear as income/expense
INSERT INTO dbo.FinancialTransactions
	(Id, BankAccountId, TransactionCategoryId, TransactionType, Amount, Description, OccurredAtUtc, CreatedAtUtc, IsDeleted, TransferGroupId)
VALUES
	(NEWID(), @CheckingAccountId, NULL, 2, -500.00, N'Transfer to savings',
		DATETIMEOFROMPARTS(@ThisYear, @ThisMonth, 5, 11, 0, 0, 0, 0), @Now, 0, NEWID()),
	(NEWID(), @SavingsAccountId, NULL, 2, 500.00, N'Transfer from checking',
		DATETIMEOFROMPARTS(@ThisYear, @ThisMonth, 5, 11, 0, 0, 0, 0), @Now, 0, NEWID());

-- Budget: 300.00 allocated for Groceries this month; actual spend expected = 200.75
INSERT INTO dbo.MonthlyBudgets (Id, ApplicationUserId, TransactionCategoryId, [Year], [Month], BudgetedAmount, CreatedAtUtc)
VALUES (NEWID(), @UserId, @GroceriesCategoryId, @ThisYear, @ThisMonth, 300.00, @Now);

-- Bill due in 10 days, partially paid
INSERT INTO dbo.Bills (Id, ApplicationUserId, Name, Amount, DueDateUtc, IsRecurring, CreatedAtUtc, IsDeleted)
VALUES (@BillId, @UserId, N'Sample Electric Bill', 150.00, DATEADD(DAY, 10, @Now), 1, @Now, 0);

INSERT INTO dbo.BillPayments (Id, BillId, AmountPaid, PaidAtUtc, CreatedAtUtc)
VALUES (NEWID(), @BillId, 50.00, @Now, @Now);
-- Expected: TotalPaid = 50.00, AmountRemaining = 100.00, IsPaid = 0, IsOverdue = 0

-- Savings goal: target 1000.00, two contributions of 150.00 and 100.00 = 250.00
INSERT INTO dbo.SavingsGoals (Id, ApplicationUserId, Name, TargetAmount, CurrentAmount, TargetDateUtc, CreatedAtUtc, IsDeleted)
VALUES (@SavingsGoalId, @UserId, N'Sample Vacation Fund', 1000.00, 250.00, DATEADD(MONTH, 6, @Now), @Now, 0);

INSERT INTO dbo.SavingsContributions (Id, SavingsGoalId, Amount, ContributedAtUtc, CreatedAtUtc)
VALUES
	(NEWID(), @SavingsGoalId, 150.00, DATEADD(DAY, -20, @Now), @Now),
	(NEWID(), @SavingsGoalId, 100.00, DATEADD(DAY, -5, @Now), @Now);
-- Expected: TotalContributed = 250.00, PercentToTarget = 25.0000

PRINT N'--- vw_MonthlyIncomeExpenses (expect Checking: Income 3000.00, Expenses 200.75, Net 2799.25) ---';
SELECT * FROM dbo.vw_MonthlyIncomeExpenses WHERE ApplicationUserId = @UserId;

PRINT N'--- vw_CategoryExpenses (expect Groceries 200.75 = 100% of month spend) ---';
SELECT * FROM dbo.vw_CategoryExpenses WHERE ApplicationUserId = @UserId;

PRINT N'--- vw_BudgetVariance (expect Budgeted 300.00, Actual 200.75, Variance 99.25, Utilization 66.9167%) ---';
SELECT * FROM dbo.vw_BudgetVariance WHERE ApplicationUserId = @UserId;

PRINT N'--- vw_AccountBalances (expect StoredBalance vs LedgerNetMovement reconciliation) ---';
SELECT * FROM dbo.vw_AccountBalances WHERE ApplicationUserId = @UserId;

PRINT N'--- vw_UpcomingBills (expect TotalPaid 50.00, AmountRemaining 100.00, IsPaid 0, IsOverdue 0) ---';
SELECT * FROM dbo.vw_UpcomingBills WHERE ApplicationUserId = @UserId;

PRINT N'--- vw_SavingsProgress (expect TotalContributed 250.00, PercentToTarget 25.0000) ---';
SELECT * FROM dbo.vw_SavingsProgress WHERE ApplicationUserId = @UserId;

ROLLBACK TRANSACTION SampleDataVerification;
PRINT N'Sample data rolled back; no changes persisted.';
