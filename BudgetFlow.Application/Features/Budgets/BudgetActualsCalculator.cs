using BudgetFlow.Application.Abstractions;
using BudgetFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Budgets;

/// <summary>
/// Computes actual spending for monthly budgets from recorded expense transactions.
/// </summary>
internal static class BudgetActualsCalculator
{
    public static async Task<decimal> GetActualExpensesAsync(
        IBudgetFlowDbContext dbContext,
        Guid userId,
        Guid transactionCategoryId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var periodStart = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = periodStart.AddMonths(1);

        var totalExpenseMagnitude = await dbContext.FinancialTransactions
            .AsNoTracking()
            .Where(t => t.BankAccount!.ApplicationUserId == userId
                && t.TransactionCategoryId == transactionCategoryId
                && t.TransactionType == TransactionType.Expense
                && t.OccurredAtUtc >= periodStart
                && t.OccurredAtUtc < periodEnd)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        return Math.Abs(totalExpenseMagnitude);
    }
}
