/*
================================================================================
View:        dbo.vw_SavingsProgress
Database:    BudgetFlowDB
Purpose:     Reports savings goal progress, including total contributions and
			 percent-to-target, derived from the contribution ledger.

Grain:       One row per SavingsGoals.Id.

Inputs:      dbo.SavingsGoals, dbo.SavingsContributions

Calculations:
  - TotalContributed    = SUM(SavingsContributions.Amount) for that goal.
  - CurrentAmount       = SavingsGoals.CurrentAmount (authoritative running
						  total maintained by the application).
  - ContributionCount   = COUNT(*) of contribution rows.
  - PercentToTarget     = CurrentAmount / NULLIF(TargetAmount, 0) * 100,
						  capped at 100 only for display via CappedPercent;
						  the raw PercentToTarget is left uncapped so
						  over-funded goals remain visible.
  - AmountRemaining     = TargetAmount - CurrentAmount (can be negative if the
						  goal was over-funded).
  - IsGoalReached       = 1 when CurrentAmount >= TargetAmount.
  - DaysUntilTargetDate = DATEDIFF(DAY, SYSUTCDATETIME(), TargetDateUtc), NULL
						  when the goal has no target date.

Business rules:
  - Only non-deleted (active/archived toggle via IsDeleted) goals are
	included; IsArchived flag is surfaced instead of filtering them out so
	historical goal exports remain possible.
  - CurrentAmount on SavingsGoals is the source of truth for progress
	(maintained transactionally by SavingsGoal.RecordContribution); the
	contribution ledger total is also surfaced for reconciliation/audit via
	TotalContributed, preventing silent drift from going unnoticed.

Authorization:
  - Returns data across ALL users. Callers MUST filter by
	ApplicationUserId = @CurrentUserId before exposing results.

Performance:
  - A CTE aggregates SavingsContributions by SavingsGoalId once, then a
	single left join attaches the contribution total/count to each goal row.
================================================================================
*/
CREATE OR ALTER VIEW dbo.vw_SavingsProgress
AS
WITH ContributionAgg AS
(
	SELECT
		sc.SavingsGoalId,
		SUM(sc.Amount) AS TotalContributed,
		COUNT(*)       AS ContributionCount,
		MAX(sc.ContributedAtUtc) AS LastContributionAtUtc
	FROM dbo.SavingsContributions AS sc
	GROUP BY sc.SavingsGoalId
)
SELECT
	sg.Id                   AS SavingsGoalId,
	sg.ApplicationUserId,
	sg.Name                 AS GoalName,
	sg.TargetAmount,
	sg.CurrentAmount,
	sg.TargetAmount - sg.CurrentAmount AS AmountRemaining,
	CAST(
		sg.CurrentAmount * 100.0 / NULLIF(sg.TargetAmount, 0)
		AS DECIMAL(9, 4)
	) AS PercentToTarget,
	CAST(
		CASE
			WHEN sg.TargetAmount <= 0 THEN 0
			WHEN sg.CurrentAmount * 100.0 / sg.TargetAmount > 100 THEN 100
			ELSE sg.CurrentAmount * 100.0 / sg.TargetAmount
		END AS DECIMAL(9, 4)
	) AS CappedPercentToTarget,
	CASE WHEN sg.CurrentAmount >= sg.TargetAmount THEN 1 ELSE 0 END AS IsGoalReached,
	COALESCE(ca.TotalContributed, 0) AS TotalContributed,
	COALESCE(ca.ContributionCount, 0) AS ContributionCount,
	ca.LastContributionAtUtc,
	sg.TargetDateUtc,
	DATEDIFF(DAY, SYSUTCDATETIME(), sg.TargetDateUtc) AS DaysUntilTargetDate,
	CASE WHEN sg.IsDeleted = 1 THEN 1 ELSE 0 END AS IsArchived
FROM dbo.SavingsGoals AS sg
LEFT JOIN ContributionAgg AS ca
	ON ca.SavingsGoalId = sg.Id;
GO
