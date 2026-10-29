/*
================================================================================
View:        dbo.vw_UpcomingBills
Database:    BudgetFlowDB
Purpose:     Reports upcoming (and overdue) bills with the amount still owed
			 after payments already recorded against them.

Grain:       One row per Bills.Id.

Inputs:      dbo.Bills, dbo.BillPayments

Calculations:
  - TotalPaid        = SUM(BillPayments.AmountPaid) for that bill.
  - AmountRemaining  = Bills.Amount - TotalPaid (floored at 0 is intentionally
					   NOT applied, so overpayments are still visible as a
					   negative remaining amount for auditing purposes).
  - IsPaid           = 1 when TotalPaid >= Bills.Amount.
  - IsOverdue        = 1 when DueDateUtc < SYSUTCDATETIME() AND NOT fully paid.
  - DaysUntilDue     = DATEDIFF(DAY, SYSUTCDATETIME(), DueDateUtc); negative
					   values indicate the bill is already past due.

Business rules:
  - Only non-deleted bills are included (IsDeleted = 0).
  - Payments are aggregated once per bill via BillPayments (not re-derived
	from FinancialTransactions), preventing duplicate aggregation of the same
	cash movement.
  - "Upcoming" is left for the caller to filter further (e.g.
	WHERE DaysUntilDue BETWEEN 0 AND 30), since different consumers need
	different horizons; the view itself returns all non-deleted bills with
	the computed fields needed to apply any such filter.

Authorization:
  - Returns data across ALL users. Callers MUST filter by
	ApplicationUserId = @CurrentUserId before exposing results.

Performance:
  - A CTE aggregates BillPayments by BillId once, then a single left join
	attaches the payment total to each bill row.
================================================================================
*/
CREATE OR ALTER VIEW dbo.vw_UpcomingBills
AS
WITH PaymentAgg AS
(
	SELECT
		bp.BillId,
		SUM(bp.AmountPaid) AS TotalPaid,
		MAX(bp.PaidAtUtc)  AS LastPaidAtUtc
	FROM dbo.BillPayments AS bp
	GROUP BY bp.BillId
)
SELECT
	b.Id                    AS BillId,
	b.ApplicationUserId,
	b.Name                  AS BillName,
	b.Amount                AS BillAmount,
	b.DueDateUtc,
	b.IsRecurring,
	COALESCE(pa.TotalPaid, 0)          AS TotalPaid,
	b.Amount - COALESCE(pa.TotalPaid, 0) AS AmountRemaining,
	pa.LastPaidAtUtc,
	CASE WHEN COALESCE(pa.TotalPaid, 0) >= b.Amount THEN 1 ELSE 0 END AS IsPaid,
	CASE
		WHEN b.DueDateUtc < SYSUTCDATETIME() AND COALESCE(pa.TotalPaid, 0) < b.Amount THEN 1
		ELSE 0
	END AS IsOverdue,
	DATEDIFF(DAY, SYSUTCDATETIME(), b.DueDateUtc) AS DaysUntilDue
FROM dbo.Bills AS b
LEFT JOIN PaymentAgg AS pa
	ON pa.BillId = b.Id
WHERE b.IsDeleted = 0;
GO
