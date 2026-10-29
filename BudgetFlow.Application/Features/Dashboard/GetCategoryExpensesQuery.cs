using BudgetFlow.Application.Abstractions;
using BudgetFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BudgetFlow.Application.Features.Dashboard;

/// <summary>
/// Query to compute total expenses per category for the current user within an optional date
/// range. Defaults to the current calendar month when no range is supplied.
/// </summary>
public sealed record GetCategoryExpensesQuery(DateTimeOffset? FromUtc, DateTimeOffset? ToUtc)
    : IRequest<IReadOnlyList<CategoryExpenseDto>>;

public sealed class GetCategoryExpensesQueryHandler : IRequestHandler<GetCategoryExpensesQuery, IReadOnlyList<CategoryExpenseDto>>
{
    private readonly IBudgetFlowDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetCategoryExpensesQueryHandler(IBudgetFlowDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CategoryExpenseDto>> Handle(GetCategoryExpensesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Array.Empty<CategoryExpenseDto>();
        }

        var utcNow = DateTimeOffset.UtcNow;
        var fromUtc = request.FromUtc ?? new DateTimeOffset(utcNow.Year, utcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var toUtc = request.ToUtc ?? fromUtc.AddMonths(1);

        var results = await _dbContext.FinancialTransactions
            .AsNoTracking()
            .Where(t => t.BankAccount!.ApplicationUserId == userId.Value
                && t.TransactionType == TransactionType.Expense
                && t.OccurredAtUtc >= fromUtc
                && t.OccurredAtUtc < toUtc
                && t.TransactionCategoryId != null)
            .GroupBy(t => new { t.TransactionCategoryId, t.TransactionCategory!.Name })
            .Select(g => new CategoryExpenseDto(g.Key.TransactionCategoryId!.Value, g.Key.Name, Math.Abs(g.Sum(t => t.Amount))))
            .OrderByDescending(c => c.TotalExpense)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results;
    }
}
