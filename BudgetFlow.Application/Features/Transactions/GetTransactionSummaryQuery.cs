using BudgetFlow.Application.Abstractions;
using BudgetFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Transactions;

/// <summary>
/// Query to compute aggregated income/expense totals for the current user, optionally scoped to
/// a single account and/or date range. Transfers are excluded from both totals since they do not
/// represent real income or expense, only movement of funds between owned accounts.
/// </summary>
public sealed record GetTransactionSummaryQuery(Guid? BankAccountId, DateTimeOffset? FromUtc, DateTimeOffset? ToUtc)
    : IRequest<TransactionSummaryDto>;

public sealed class GetTransactionSummaryQueryHandler : IRequestHandler<GetTransactionSummaryQuery, TransactionSummaryDto>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionSummaryQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<TransactionSummaryDto> Handle(GetTransactionSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId is null)
        {
            return new TransactionSummaryDto(0, 0, 0, request.FromUtc, request.ToUtc);
        }

        var query = _dbContext.FinancialTransactions
            .AsNoTracking()
            .Where(t => t.BankAccount!.ApplicationUserId == userId.Value);

        if (request.BankAccountId is Guid accountId)
        {
            query = query.Where(t => t.BankAccountId == accountId);
        }

        if (request.FromUtc is DateTimeOffset fromUtc)
        {
            query = query.Where(t => t.OccurredAtUtc >= fromUtc);
        }

        if (request.ToUtc is DateTimeOffset toUtc)
        {
            query = query.Where(t => t.OccurredAtUtc <= toUtc);
        }

        // Transfers are intentionally excluded: they move funds between the user's own
        // accounts and do not represent real income or expense.
        var totalIncome = await query
            .Where(t => t.TransactionType == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var totalExpenseMagnitude = await query
            .Where(t => t.TransactionType == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var totalExpense = Math.Abs(totalExpenseMagnitude);

        return new TransactionSummaryDto(totalIncome, totalExpense, totalIncome - totalExpense, request.FromUtc, request.ToUtc);
    }
}
